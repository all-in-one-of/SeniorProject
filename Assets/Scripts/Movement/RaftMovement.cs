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

    public string BoardRaftText;
    public string DisembarkRaftText;

	[SerializeField]
	[Tooltip("Time till coroutine ends and FixedUpdate checks if the raft is stopped again.")]
    private float updateIsStoppedTimer = 2.0f;
	private bool isStopped = false;

    [SerializeField]
    private float updatePlayerPositionTimer = 2.0f;
    private bool updatingPlayerMovement = false;

    /// <summary>
    /// Fixed update for physical movement of the raft
    /// </summary>
    void FixedUpdate ()
    {
		Debug.Log(Game.Instance.PlayerInstance.Controller.IsOnRaft);
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

        // Make sure we are floating on top of the water as it rises
        // Leave a little space so there is no friction
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit))
        {
            float diff = heightAboveWater - hit.distance;
            RigidBody.position = RigidBody.position + new Vector3(0, diff, 0);
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
		// TODO: update this in the future so it will work with more than 
        //       one raft in the scene.
        while(Game.Instance.PlayerInstance.Controller.IsOnRaft)
        {
			Game.Instance.PlayerInstance.Controller.BoardRaft(this);
			yield return new WaitForSeconds(this.updatePlayerPositionTimer);
    	}

		this.updatingPlayerMovement = false;
    }

    /// <summary>
    /// Applies a force the raft in the specified direction.
    /// </summary>
    /// <param name="direction"></param>
    public override void Move(Vector3 direction, bool sprinting, Animator playerAnimator)
    {
        RigidBody.AddForce(direction.normalized * acceleration);
        StartCoroutine(this.updateIsStopped());

		if(!this.updatingPlayerMovement)
		{
			this.updatingPlayerMovement = true;
			StartCoroutine(this.updatePlayerRaftPosition());
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
}
