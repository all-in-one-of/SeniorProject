﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DistrictItemConfiguration
{
	/// <summary>
	/// Initializes a new instance of the <see cref="DistrictItemInfo"/> class.
	/// </summary>
	public DistrictItemConfiguration()
	{
		ItemTemplates = new List<GameObject>();
		ItemExtents = new List<float>();
	}

	/// <summary>
	/// Gets or sets the item templates that will be duplicated to create objects.
	/// </summary>
	/// <value>The item templates.</value>
	public List<GameObject> ItemTemplates
	{
		get;
		set;
	}

	/// <summary>
	/// Gets or sets the item extents.
	/// </summary>
	/// <value>The item extents.</value>
	public List<float> ItemExtents
	{
		get;
		set;
	}
}