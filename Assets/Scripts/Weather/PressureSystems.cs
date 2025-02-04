﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Pressure constants that access readonly array pressureConstants
/// in PressureSystem.
/// </summary>
public enum PressureConstantIndexes
{
	PressureSiberia,
	PressureHighMax,
	PressureHighMin,
	PressureLowMax,
	PressureLowMin,
	PressureHurricane
};

public class PressureSystems 
{
	// Values found at: http://www.theweatherprediction.com/habyhints2/410/ and can 
	// be accessed by enumeration PressureConstants
	public readonly float[] PressureConstants           = {1086f, 1020f, 1000f, 1000f, 980f, 870f};
	private const int startNumberOfPressureSystems       = 10;
	private const float highPressureCenterAttractorForce = 2f;
	private const float lowPressureCenterAttractorForce  = 4f;

    // flag for if the class has been initialized before
    public bool Initialized
    {
        get;
        private set;
    }

	/// <summary>
	/// Gets the local pressure systems.
	/// </summary>
	/// <value>The local pressure systems.</value>
	public List<PressureSystem> LocalPressureSystems
	{
		get;
		private set;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="PressureSystems"/> class.
	/// </summary>
	/// <param name="bounds">Bounds.</param>
	public void Initialize(CityBoundaries bounds)
	{
        // This class can only be initialized once
        if (this.Initialized)
        {
            // print error but don't break
            Debug.LogError("This class should only be initialized once after loading.");
            return;
        }

		this.LocalPressureSystems = new List<PressureSystem>();

		for(int i = 0; i < PressureSystems.startNumberOfPressureSystems; ++i)
		{
			PressureSystem ps = new PressureSystem();

            if (i < bounds.Edges.Length)
            {
                // put the pressure city in a corner of the map 
                ps.Position = bounds.Edges[i];
            }
            else
            {
                // else put it an a random place in the city
                ps.Position = bounds.RandomVector2d;
            }

			// add vector to pressure system
			this.LocalPressureSystems.Add(ps);

			// determine if high or low and set pressure
			this.setVectorPressure(i);
		}

        this.Initialized = true;
	}

	/// <summary>
	/// Subscribed to clock delegate for every second to update. This method
	/// will move pressure systems around the world.
	/// </summary>
	public void UpdatePressureSystem()
	{
        // only run if this class has been initialized
        if (!Initialized)
        {
            return;
        }

        this.GetClosestPressureSystem(VectorUtility.XZ(Game.Player.WorldPosition)).UpdatePressure();

		// initialize new pressure system to replace current one
		List<PressureSystem> newPressureSystem = new List<PressureSystem>();

		// update each pressure system
		for(int i = 0; i < this.LocalPressureSystems.Count; ++i)
		{
            // get unit vector of vector at index i
            Vector2 normalizedIVector = this.LocalPressureSystems[i].Position.normalized;

			// initialize forces that will be put on the pressure system
			Vector2 forces = Vector2.zero;

			// loop through pressure systems
			for(int j = 0; j < this.LocalPressureSystems.Count; ++j)
			{	
				// go to next index if this is the current pressure system for index i
				if(i == j)
				{
					continue;
				}

				// get unit vector at index j
				Vector2 normalizedJVector = this.LocalPressureSystems[j].Position.normalized;

				// calculate vector differences between index i and j
				Vector2 force = normalizedIVector - normalizedJVector;

				// if this is not the same kind of pressure system, apply a rotational force
				// rather than detracting force
				if(this.LocalPressureSystems[i].IsHighPressure != this.LocalPressureSystems[j].IsHighPressure)
				{
					force = this.calculatePerpendicularVector(-force);
				}

				// apply force to current acting forces on the pressure system
				forces += (force / Vector3.Distance(this.LocalPressureSystems[i].Position, this.LocalPressureSystems[j].Position)).normalized;
			}

			// apply forces applied by center of map.
			// Note: this assumes the center is at 0,0. This can be easily updated 
			//       in the future.
			if(this.LocalPressureSystems[i].IsHighPressure)
			{
				forces += -normalizedIVector * PressureSystems.highPressureCenterAttractorForce;
			}
			else
			{
				forces += -normalizedIVector * PressureSystems.lowPressureCenterAttractorForce;
			}


			// add system to list
			newPressureSystem.Add(this.LocalPressureSystems[i]);

			// change position and bound vector to be with in boundaries of the map
			newPressureSystem[i].Position = Game.Instance.CityBounds.BoundVector2d(forces + newPressureSystem[i].Position);
		}

		// replace old system with new
		this.LocalPressureSystems = newPressureSystem;
	}

	/// <summary>
	/// Gets the closest pressure system.
	/// </summary>
	/// <returns>The closest pressure system.</returns>
	/// <param name="position">Position.</param>
	public PressureSystem GetClosestPressureSystem(Vector2 position)
	{
		PressureSystem closest = this.LocalPressureSystems[0];
		float dist             = Vector2.Distance(closest.Position, position);

		for(int i = 1; i < this.LocalPressureSystems.Count; ++i)
		{
			// calculate distance
			float newDist = Vector2.Distance(this.LocalPressureSystems[i].Position, position);

			// check if closer pressure system is found
			if(newDist < dist)
			{
				dist    = newDist;
				closest = this.LocalPressureSystems[i]; 
			}
		}

		return closest;
	}

	/// <summary>
	/// Return a list of low pressure systems.
	/// </summary>
	/// <returns>The pressure systems.</returns>
	public List<PressureSystem> LowPressureSystems()
	{
		List<PressureSystem> lowPressureSystems = new List<PressureSystem>();

		for(int i = 0; i < this.LocalPressureSystems.Count; ++i)
		{
			if(this.LocalPressureSystems[i].IsHighPressure == false)
			{
				lowPressureSystems.Add(this.LocalPressureSystems[i]);
			}
		}

		return lowPressureSystems;
	}

	/// <summary>
	/// Calculates the perpindicular vector to the vector passed in.
	/// </summary>
	/// <returns>The perpindicular vector.</returns>
	/// <param name="vec">Vec.</param>
	private Vector2 calculatePerpendicularVector(Vector2 vector)
	{
		// http://mathworld.wolfram.com/PerpendicularVector.html
		return new Vector2(-vector.y, vector.x);
	}

	/// <summary>
	/// Sets the vector pressure.
	/// </summary>
	/// <param name="index">Index.</param>
	private void setVectorPressure(int index)
	{
		if(RandomUtility.RandomBool)
		{
			// high pressure
			this.LocalPressureSystems[index].IsHighPressure = true;
			this.LocalPressureSystems[index].Pressure = Random.Range(this.PressureConstants[(int) PressureConstantIndexes.PressureHighMin],
			                                                         this.PressureConstants[(int) PressureConstantIndexes.PressureHighMax]);

        }
		else
		{
			// low pressure
			this.LocalPressureSystems[index].IsHighPressure = false;
			this.LocalPressureSystems[index].Pressure = Random.Range(this.PressureConstants[(int) PressureConstantIndexes.PressureLowMin],
			                                                         this.PressureConstants[(int) PressureConstantIndexes.PressureLowMax]);
        }
	}

    /// <summary>
    /// Create instance of pressure systems that has not yet been
    /// initialized. Initialize can be called after the game has
    /// been loaded
    /// </summary>
    public PressureSystems()
    {
        this.Initialized = false;
    }
}