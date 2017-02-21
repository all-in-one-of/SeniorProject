﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WaterItemGeneration : ItemGenerator 
{
	[SerializeField]
    [Tooltip("Max number of items to be generated in the water")]
	private int maxNumberOfItems;

	[SerializeField]
	[Tooltip("Generally, the more initial points generated, the more evenly spread out the items will be")]
	private int numberOfInitialPoints;

	[SerializeField]
	[Tooltip("Generally, the more points per previous point and less number of initial points, the more clustered item generation will be")]
	private int newPointsPerSamplingPoint;

	[SerializeField]
	[Tooltip("The number of attempts to place a point. Generally, the higher the number, the closer to the maximum number of items will be generated")]
	private int maxAttempts;

	[SerializeField]
	[Tooltip("How many times it will generate an item for a point without checking for district before checking for district again")]
	private int districtCheck;

	/// <summary>
	/// The point generator.
	/// </summary>
	private WaterPointGenerator pointGenerator;

	/// <summary>
	/// Awake this instance.
	/// </summary>
	void Awake () 
	{
		Dictionary<string, List<GameObject>> itemTemplates = Game.Instance.WorldItemFactoryInstance.GetAllInteractableItemsByDistrict(false, true);
		districtItemInfo = new Dictionary<string, DistrictItemConfiguration>();

		// get district name here
		foreach(string key in itemTemplates.Keys)
		{
			districtItemInfo.Add(key, new DistrictItemConfiguration());
			districtItemInfo[key].ItemTemplates = itemTemplates[key];
			districtItemInfo[key].ItemNames = Game.Instance.ItemFactoryInstance.WaterItemsByDistrict[key];
			districtItemInfo[key].ItemExtents = GetItemExtents(itemTemplates[key]);
		}
	}

	/// <summary>
	/// Sets the city information. Must be called before points are generated.
	/// </summary>
	/// <param name="cityWidth">City width.</param>
	/// <param name="cityDepth">City depth.</param>
	/// <param name="districts">Districts.</param>
	public void SetCityInformation(float cityWidth, float cityDepth, Vector3 cityCenter, District[] districts)
	{
		pointGenerator = new WaterPointGenerator(cityWidth, cityDepth, cityCenter);
		pointGenerator.MaxNumberOfItems = maxNumberOfItems;
		pointGenerator.NumberOfInitialPoints = numberOfInitialPoints;
		pointGenerator.NewPointsPerSamplingPoint = newPointsPerSamplingPoint;
		pointGenerator.MaxAttempts = maxAttempts;
		pointGenerator.DistrictCheck = districtCheck;
		pointGenerator.Districts = districts;
	}

	/// <summary>
	/// Generates the items in water.
	/// </summary>
	public void GenerateInWater()
	{
		Dictionary<string, List<float>> generatableItemExtents = new Dictionary<string, List<float>>();

		if(pointGenerator != null)
		{
			foreach(string key in districtItemInfo.Keys)
			{
				generatableItemExtents.Add(key, districtItemInfo[key].ItemExtents);
			}

			List<ItemPlacementSamplePoint> points = pointGenerator.GetPointsInWater(generatableItemExtents);

			if (points.Count > 0) 
			{
				addObjectsToPoolManager(points);
			}
		}
	}

	/// <summary>
	/// Adds the building to water point generation's map of blocked off space.
	/// </summary>
	/// <param name="buildingBound">Building bound.</param>
	public void AddBuildingToWaterGenerationMap(Bounds buildingBound)
	{
		pointGenerator.AddBuildingBounds(buildingBound);
	}

	/// <summary>
	/// Generates the objects that needs to be placed on the building's surface.
	/// </summary>
	/// <param name="hasDoor">If set to <c>true</c>, there is a door that needs to be created.</param>
	/// <param name="district">Name of the district for which generation is occuring.</param>
	/// <param name="points">Points.</param>
	private void addObjectsToPoolManager(List<ItemPlacementSamplePoint> points)
	{
		for (int i = 0; i < points.Count; ++i) 
		{
			if(points[i].ItemIndex < districtItemInfo[points[i].District].ItemNames.Count)
			{
				poolManager.AddToGrid(points[i].WorldSpaceLocation, districtItemInfo[points[i].District].ItemNames[points[i].ItemIndex], false);
			}
		}
	}
}
