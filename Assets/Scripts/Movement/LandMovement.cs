﻿using UnityEngine;
using System.Collections;
using System;
using DG.Tweening;

[RequireComponent(typeof(CharacterController))]
public class LandMovement : Movement
{
    [SerializeField]
    private float walkingSpeed;
    [SerializeField]
    private float sprintingSpeed;
    [SerializeField]
    private float jumpForce;
    [SerializeField]
    private float minFallDamageVelocity;
    [SerializeField]
    private float fallDamageModifier;

    [SerializeField]
    [Tooltip("Height, starting from the floor to raycast towards walls")]
    private float raycastHeight;

    private const string playerAnimatorTurn = "Turn";
    private const string playerAnimatorForward = "Forward";
    private const string playerAnimatorJump = "Jump";
    private const string playerAnimatorClimb = "Climb";
    private const float playerAnimatorSprint = 1f;
    private const float playerAnimatorWalk = .5f;
    private const float playerAnimatorIdle = 0f;
    private CharacterController controller;

    private bool jumping;
    [SerializeField]
    private bool airControll;
    private float lastSpeed;
    private Vector3 lastDirection;

    /// <summary>
    /// Get the character controller so we can call move functions
    /// </summary>
    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    /// <summary>
    /// Calculate accumulated fall damage
    /// </summary>
    void FixedUpdate()
    {
        if (controller.velocity.y < -minFallDamageVelocity)
        {
            AccumulatedFallDamage += fallDamageModifier;
        }
    }

    /// <summary>
    /// Walks the player in the specified direction.
    /// </summary>
    /// <param name="direction">The direction to walk the player.</param>
    public override void Move(Vector3 direction, bool sprinting, Animator playerAnimator)
    {
        if(!jumping || airControll)
        {
            lastDirection = direction;
        }
        else
        {
            direction = Vector3.zero;
        }

        Speed = lastSpeed;

        // Walking
        if (!sprinting && (!jumping || airControll))
        {
			Speed = walkingSpeed;
            playerAnimator.SetFloat(playerAnimatorForward, playerAnimatorWalk);
        }
        // Sprinting
        else if (!jumping || airControll)
        {
			Speed = sprintingSpeed;
            playerAnimator.SetFloat(playerAnimatorForward, playerAnimatorSprint);
        }

        Vector3 moveVector = lastDirection * Speed;
        if (!jumping)
        {
            moveVector += Physics.gravity;
        }
        moveVector *= Time.fixedDeltaTime;

        controller.Move(moveVector);
        if(!jumping || airControll)
        { 
            Vector3 facingRotation = Vector3.Normalize(new Vector3(controller.velocity.x, 0f, controller.velocity.z));
            if (facingRotation != Vector3.zero)
            {
                playerAnimator.transform.forward = facingRotation;                
            }
        }
        lastSpeed = Speed;
    }

    /// <summary>
    /// Plays the idle animation
    /// </summary>
    /// <param name="playerAnimator">The player's animator</param>
    public override void Idle(Animator playerAnimator)
	{
        playerAnimator.SetFloat(playerAnimatorForward, playerAnimatorIdle);
        controller.Move(Physics.gravity * Time.fixedDeltaTime);
    }

    /// <summary>
    /// The player aesthetic jumps
    /// </summary>
    /// <param name="playerAnimator">The player's animator</param>
    public override void Jump(Animator playerAnimator)
    {
        jumping = true;
        playerAnimator.SetTrigger(playerAnimatorJump);
    }

    /// <summary>
    /// The player's climb animation plays
    /// </summary>
    /// <param name="playerAnimator">The player's animator</param>
    public override void Climb(Animator playerAnimator)
    {
        // TODO: Switch this to the climb animation.
        playerAnimator.SetTrigger(playerAnimatorClimb);
    }

    /// <summary>
    /// The player's rigidbody gets the jump force applied. Called via the animator.
    /// </summary>
    public void JumpForce()
    {
        controller.Move(Vector3.up * jumpForce);
    }

    /// <summary>
    /// The sets jumping back to false. Called via the animator.
    /// </summary>
    public void JumpLand()
    {
        jumping = false;
    }

    /// <summary>
    /// The height the player can climb while in this movement state
    /// </summary>
    public override float GetClimbHeight()
    {
        return climbHeight;
    }

    /// <summary>
    /// The height of the climbing raycast while in this movement state
    /// </summary>
    public override float GetRaycastHeight()
    {
        return raycastHeight;
    }

    /// <summary>
    /// Called when the player enters the state.
    /// </summary>
    public override void OnStateEnter()
    {

    }

    /// <summary>
    /// Called when the player exits the state.
    /// </summary>
    public override void OnStateExit()
    {

    }

}
