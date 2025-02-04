﻿using UnityEngine;
using System.Collections;
using System;

public class RaftMovement : Movement 
{
	// TODO: this is to be used in the future when telling the raft movement
	//       whether it is active or not.
	[HideInInspector]
	/// <summary>
	/// Indicates whether the raft is active. Currently not in use.
	/// </summary>
	public bool IsActive = false;

    [SerializeField]
    private float acceleration;
    [SerializeField]
    private float maxSpeed;
    [SerializeField]
    private float stopThreshold;
    [SerializeField]
    private float heightAboveWater;
    [SerializeField]
    private float playerStandHeight = 0.33f;

    /// <summary>
    /// How high the player should be on the raft
    /// </summary>
    /// <value>The height of the player on the raft.</value>
    public float PlayerStandHeight
    {
        get
        {
            return playerStandHeight;
        }
    }

    // The player's Animator.
    public Animator PlayerAnimator;

    private const string playerAnimatorTurn = "Turn";
    private const string playerAnimatorForward = "Forward";
    private const string playerAnimatorFall = "Falling";
    private const string playerAnimatorSwimming = "Swimming";
	private float againstWindSpeedModifier = 0.3f;
	private float windResistanceAngleThreshold = 0.25f;

	private Rigidbody raftBody;

    [SerializeField]
	[Tooltip("Time till coroutine ends and FixedUpdate checks if the raft is stopped again.")]
    private float updateIsStoppedTimer = 2.0f;
	private bool isStopped = false;

	/// <summary>
	/// Gets or sets a value indicating whether this <see cref="RaftMovement"/> raft is moored.
	/// </summary>
	/// <value><c>true</c> if raft moored; otherwise, <c>false</c>.</value>
	public bool RaftMoored
	{
		get;
		set;
	}

    [SerializeField]
    private float updatePlayerPositionTimer = 2.0f;
   
    /// <summary>
    /// Get the raft's rigidbody component. 
    /// </summary>
    void Awake()
    {
        raftBody = GetComponent<Rigidbody>();
        raftBody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    }

    /// <summary>
    /// Fixed update for physical movement of the raft
    /// </summary>
    void FixedUpdate ()
    {
        float speed = RigidBody.velocity.magnitude;

        // cap the speed of the raft
        if (speed > maxSpeed)
        {
            RigidBody.velocity = RigidBody.velocity.normalized * maxSpeed;
        }

        // if the speed of the raft is near zero, lock it to zero 
        // (so it doesn't drift unneccisarily)
        if (!this.isStopped && speed < stopThreshold)
        {
        	this.isStopped = true;
            RigidBody.velocity = Vector3.zero;
        }
    }

    /// <summary>
    /// Runs the raft idle animation.
    /// </summary>
    public override void Idle(Animator playerAnimator)
    {
         //Raft Idle animation code
    }

    /// <summary>
    /// Updates the is stopped.
    /// </summary>
    /// <returns>The is stopped.</returns>
    private IEnumerator updateIsStopped()
    {
    	yield return new WaitForSeconds(this.updateIsStoppedTimer);
    	this.isStopped = false;
    }

    /// <summary>
    /// Updates the players position on the raft.
    /// </summary>
    /// <returns>The raft position.</returns>
    private IEnumerator updatePlayerRaftPosition()
    {
        while (Game.Instance.PlayerInstance.Controller.IsOnRaft)
        {
            Game.Instance.PlayerInstance.Controller.BoardRaft(this);
            yield return new WaitForSeconds(this.updatePlayerPositionTimer);
        }
    }

    /// <summary>
    /// Applies a force the raft in the specified direction.
    /// </summary>
    /// <param name="direction"></param>
    public override void Move(Vector3 direction, bool sprinting, Animator playerAnimator)
    {
    	if(!RaftMoored)
    	{
	    	// the maximum resistance is -1, so the direction angle returns by CompareDirectionToOctantWind must be a negative
	    	// number such that the difference between it and -1 is less than the windResistanceAngleThreshold
			if (Game.Instance.WeatherInstance.CompareDirectionToOctantWind(direction) + 1 < windResistanceAngleThreshold)
	        {
	            Speed *= againstWindSpeedModifier;
	        }

	        RigidBody.AddForce(direction.normalized * acceleration);
	        StartCoroutine(this.updateIsStopped());
	    }
    }

    /// <summary>
    /// Doesn't do anything on boats for now.
    /// </summary>
    public override void Jump(Animator playerAnimator)
    {
       
    }

    /// <summary>
    /// Doesn't do anything on boats for now
    /// </summary>
    public override void Climb(Animator playerAnimator)
    {
       
    }

    /// <summary>
    /// The height the player can climb while in this movement state (which is 0!)
    /// </summary>
    public override float GetClimbHeight()
    {
        return 0f;
    }

    /// <summary>
    /// Called when the player enters the state.
    /// </summary>
    public override void OnStateEnter()
    {
        if(PlayerAnimator != null)
        {
            PlayerAnimator.SetBool(playerAnimatorFall, false);
            PlayerAnimator.SetFloat(playerAnimatorForward, 0f);
            PlayerAnimator.SetBool(playerAnimatorSwimming, false);
            PlayerAnimator.SetFloat(playerAnimatorTurn, 0f);
        }

    }

    /// <summary>
    /// Called when the player exits the state.
    /// </summary>
    public override void OnStateExit()
    {
       isStopped = true;
    }

    /// <summary>
    /// UNUSED
    /// The height of the climbing raycast while in this movement state
    /// </summary>
    public override float GetRaycastHeight()
    {
        return 0f;
    }

    /// <summary>
    /// Sets the max speed.
    /// </summary>
    /// <param name="speed">Speed.</param>
    public void SetMaxSpeed(float speed)
    {
        maxSpeed = speed;
    }
}
