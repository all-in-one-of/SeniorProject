﻿
using UnityEngine;
using System.Collections.Generic;

[RequireComponent (typeof (FishAgentConfig))]
[RequireComponent (typeof (Rigidbody))]
[RequireComponent (typeof (Collider))]
public class FishAgent : MonoBehaviour 
{
	// Move towards when wondering
	private Vector3 WanderTarget;
    
    // Rigid body
    private Rigidbody rb;

    // layers
	protected LayerMask predatorLayer;
	protected LayerMask agentLayer;

	// configuration
	protected FishAgentConfig config;

	// constant layer strings
	private const string predatorLayerString   = "Predator";
	private const string fisghAgentLayerString = "Agent";

	/// <summary>
	/// The velocity of the agent
	/// </summary>
    public Vector3 Velocity
    {
    	get;
    	private set;
    }

    /// <summary>
    /// Gets or sets the attractor to fish should swim towards when wandering.
    /// </summary>
    /// <value>The attractor transform.</value>
    public Transform Attractor
    {
    	get;
    	set;
    }

    /// <summary>
    /// Sets the layers.
    /// </summary>
	protected virtual void setLayers()
	{
		// Get predator layer
		this.predatorLayer = LayerMask.GetMask(new string[] {FishAgent.predatorLayerString});
		
		// Get agent layer
		this.agentLayer = LayerMask.GetMask(new string[] {FishAgent.predatorLayerString});
	}

	/// <summary>
	/// Defines the configuration.
	/// </summary>
	private void defineConfiguration()
	{
		if(this.config == null)
		{
			this.config = this.GetComponent<FishAgentConfig>();
		}
	}

    /// <summary>
    /// Initialize with basic info
    /// </summary>
    void Start ()
    {
   		// set layers
		this.setLayers();

		// Get configuration
		this.config = this.GetComponent<FishAgentConfig>();
		this.config.RandomizeSelf();

		// Start with random velocity
		this.randomizeVelocity();
	}

	/// <summary>
	/// Raises the enable event.
	/// </summary>
	void OnEnable()
	{
		this.defineConfiguration();
		this.randomizeVelocity();
	}
	
    /// <summary>
    /// Update movement of agent
    /// </summary>
	void FixedUpdate ()
    {
		// Update acceleration
        Vector2 acceleration2d = Vector2.ClampMagnitude(this.Combine(), this.config.MaxAcceleration);

        // Euler Forward Integration
		Vector2 velocity = Vector2.ClampMagnitude(VectorUtility.XZ(this.Velocity) + acceleration2d * Time.deltaTime, this.config.MaxVelocity);

		// convert 2d calculations to 3d
		this.Velocity    = VectorUtility.twoDimensional3d(velocity);

        // Set new position
        this.transform.position = this.transform.position + (this.Velocity * Time.deltaTime);

        // set alignment of fish to face in the direction of the movement. We set it to the right
        // because of how the project from the 2d XY plane to 3d XZ plane works.
        this.transform.right  = this.Velocity;
	}

    /// <summary>
    /// Go to center of neighbors
    /// </summary>
    /// <returns>Center Point</returns>
    public Vector2 Cohesion()
    {
		// Cohesion behaavior
        Vector3 result = new Vector3();
        
        // Get all neighbors
		Collider2D[] neighbors = Physics2D.OverlapCircleAll(this.transform.position, this.config.CohesionRadius, this.agentLayer);

        // check if neighbors is full or not
        if (neighbors.Length > 0)
        {
			// Num agents
			int agentCount = 0;

            for (int i = 0; i < neighbors.Length; ++i)
            {
                result += neighbors[i].transform.position;
                ++agentCount;
            }

            // Divide by count
            if (agentCount > 0)
            {
                result /= agentCount;
            }
            
            // Look center
            result -= this.transform.position;

            // normalize vector
            result.Normalize();
        }

        // Return result
        return result;
    }

    /// <summary>
    /// Move away from neighbors
    /// </summary>
    /// <returns></returns>
    public Vector2 Separation()
    {
        // Separation result
        Vector2 result = new Vector2();

        // Get all neighbors
		Collider2D[] neighbors = Physics2D.OverlapCircleAll(this.transform.position, this.config.SeparationRadius, this.gameObject.layer);

        // check if neighbors is full or not
        for (int i = 0; i < neighbors.Length; ++i)
        {
			Vector2 towardsMe = VectorUtility.XZ(this.transform.position) - VectorUtility.XZ(neighbors[i].transform.position);

            // Contribution depends on distance
            if (towardsMe.magnitude > 0)
            {
                result += towardsMe.normalized / towardsMe.magnitude;
            }

            // Normalize
            result.Normalize();
        }

        // return result
        return result;
    }

    /// <summary>
    /// Rotate in direction of neighboring agents movement
    /// </summary>
    /// <returns></returns>
	public Vector2 Alignment()
    {
		Vector2 result = new Vector2();

        // Get all neighbors
		Collider2D[] neighbors = Physics2D.OverlapCircleAll(this.transform.position, this.config.AlignmentRadius, this.gameObject.layer);

        // check if neighbors is full or not
        if (neighbors.Length > 0)
        {
            for (int i = 0; i < neighbors.Length; ++i)
            {
            	// TODO in the future, look for a better way to do this
                result += VectorUtility.XZ(neighbors[i].gameObject.GetComponent<FishAgent>().Velocity);
            }

            // Nomalize vector
            result.Normalize();
        }

        // return result
        return result;
    }

	/// <summary>
	/// Smooth out movement
	/// </summary>
	/// <returns></returns>
	public Vector2 Wander()
	{
		if (Attractor != null)
		{
			this.WanderTarget = this.Attractor.position - this.transform.position;
		}
		else
		{
			float jitter = this.config.Jitter * Time.deltaTime;
			this.WanderTarget += new Vector3(RandomUtility.RandomBinomial * jitter, RandomUtility.RandomBinomial * jitter, 0);
		}

		this.WanderTarget.Normalize();
		this.WanderTarget *= this.config.WanderRadius;

		Vector3 targetInLocalSpace = this.WanderTarget + new Vector3(0, 0, this.config.WanderDistanceRadius);
		Vector3 targetInWorldSpace = this.transform.TransformPoint(targetInLocalSpace);
		return (targetInWorldSpace - this.transform.position).normalized;
	}

    /// <summary>
	/// Use alignment, Cohesion, and Separation to define behavior with diferent proportions based on importance
    /// </summary>
    /// <returns>Vector with correct behavior</returns>
    public virtual Vector2 Combine()
    {
        return this.config.CohesionWeight * this.Cohesion() 
             + this.config.SeparationWeight * this.Separation()
			 + this.config.AlignmentWeight * this.Alignment()
             + this.config.WanderWeight * this.Wander();
    }

    /// <summary>
    /// Check if agent is in field of fiew for this agent
    /// </summary>
    /// <param name="agent"></param>
    /// <returns></returns>
    public bool InFieldOfView(Vector3 agent)
    {
		return Vector3.Angle(this.Velocity, agent - this.transform.position) <= this.config.MaxFieldOfViewAngle;
    } 

    /// <summary>
    /// Randomizes the velocity.
    /// </summary>
    private void randomizeVelocity()
    {
    	// initialize min and max vectors
		Vector2 min = new Vector2(-this.config.MaxVelocity, -this.config.MaxVelocity);
		Vector2 max = new Vector2(this.config.MaxVelocity,   this.config.MaxVelocity);

		// set the velocity to a random 3d vector with a y coordinate of 0
		this.Velocity = VectorUtility.twoDimensional3d(RandomUtility.RandomVector2d(min, max));
    }
}