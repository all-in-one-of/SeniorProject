﻿using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections;
using RootMotion.FinalIK;
using DG.Tweening;

public class PlayerController : MonoBehaviour
{
	[Header("Movement Settings")]
    [SerializeField]
    private float groundedThreshold;
    [SerializeField]
    private Animator playerAnimator;

    [Header("Tag Settings")]
    [SerializeField]
    private string interactiveTag;
    [SerializeField]
    private string landTag;
    [SerializeField]
    private string waterTag;

    [Header("HUD Settings")]
    /// <summary>
    /// The events that fire when hunger is updated.
    /// </summary>
    public UnityEvent HungerUpdatedEvent;

    /// <summary>
    /// The events that fire when health is updated.
    /// </summary>
    public UnityEvent HealthUpdatedEvent;

    /// <summary>
    /// The events that fire when warmth is updated.
    /// </summary>
    public UnityEvent WarmthUpdatedEvent;

    [Header("Resource Settings")]
    [SerializeField]
	public StatResourceSettings StatSettings;

	[Header("Sickness Settings")]
	[SerializeField]
	[Tooltip("The chance of the player getting pnuemonia.")]
	private float pnuemoniaChance;

	[SerializeField]
	[Tooltip("The value the player's warmth must reach before pneumonia can happen.")]
	private float pnuemoniaWarmthThreshold;

	[SerializeField]
	[Tooltip("How many seconds per pnuemonia check.")]
	private float pneumoniaCheckTime;


    [Header("DebugMode Settings")]
    [SerializeField]
    private float verticalSpeed;
    [SerializeField]
    private KeyCode debugFlightUp;
    [SerializeField]
    private KeyCode debugFlightDown;

    [Header("Field of View Setting")]
    // public so the editor can touch them
    public float ViewRadius;
    [Range(0, 360)]
    public float ViewAngle;
    [SerializeField]
    private LayerMask interactablesMask;
    [SerializeField]
    [Tooltip("Any object that blocks player's view to interactable object.")]
    private LayerMask obstacleMask;

    [SerializeField]
    private LayerMask groundedMask;
    [SerializeField]
    [Tooltip("How deep the player can be in water before they start swimming.")]
    private float waterWadeHeight;

    [SerializeField]
    [Tooltip("Time for the player to tween to be on the raft")]
    private float raftBoardTime = .5f;


    private const float groundedRaycastHeight = 0.01f;
	private const float distanceToCheckWater = 0.5f;

    [Header("Sound Settings")]
    [SerializeField]
    private string roofFootstepSoundEvent = "event:/Player/Movement/Walking/Concrete";

    public bool IsGrounded
    {
        get
        {
            return isGrounded;
        }
    }
    private bool isGrounded;
    private bool updateStats;
    private bool isFlying;
    private bool isInShelter;
    private bool isByFire;
    private bool isReading;
	private bool isWaterInView;

	public PlayerStatManager PlayerStatManager;

    private float buttonZoomAmount = 0.1f;

    private InteractableObject interactable;
    public InteractableObject Interactable
    {
        get
        {
            return interactable;
        }
    }


    // the closest interactable as well as the distance
    private Collider closestInteractable;

    private float closestDistance;

    // the previous closest collider
    private Collider prevInteractable;

    private Movement movement;
    private LandMovement landMovement;
    private WaterMovement waterMovement;

    private PlayerTool equippedTool;
    private CameraController playerCamera;
    private Rigidbody playerRigidbody;

    private PlayerTool fishingRod;

    private Transform defaultParent;

    [SerializeField]
    private ControlScheme controlScheme;

    /// <summary>
    /// The event emitter for player sounds.
    /// </summary>
    private FMOD.Studio.EventInstance eventEmitter;

    public Animator PlayerAnimator
    {
        get
        {
            return playerAnimator;
        }
    }

    [Header("Climbing Variables")]
    public BipedIK PlayerIKSetUp;
    public LayerMask ClimbingRaycastMask;
    [SerializeField]
    [Tooltip("The distance between both hands when grabbing a ledge. About shoulder length apart.")]
    // This value is also used in raycasting to help the player face a wall.
    private float handSpacing = 0.386f;
    [SerializeField]
    [Tooltip("The max distance the player can be from the wall to climb")]
    private float climbDistance;
    // We seperate these variables so we know when to tween the hands/body to the ledge, and to tween them away from the ledge.
    [SerializeField]
    [Tooltip("Time To Animate the player to the wall")]
    private float startClimbTime;
    [SerializeField]
    [Tooltip("Time To Animate the player up the wall")]
    private float ClimbTime;
    [SerializeField]
    [Tooltip("Time To Animate the player from the top of the wall to walking again")]
    private float endClimbTime;
    [SerializeField]
    [Tooltip("How far forward to move the player after climbing")]
    private float climbForward;
    [SerializeField]
    [Tooltip("Don't let the player climb distances less than this. This prevents climbing on slopes/things the player can just walk over.")]
    private float minClimbHeight;

    private IEnumerator healthCoroutine;
    private IEnumerator warmthCoroutine;
    private IEnumerator hungerCoroutine;

    // How far forward a raycast should be to check ledge height.
    const float raycastClimbForward = .2f;

    // If true, the player won't be able to move. Used when the player is being moved by some other means, like a cutscene or climbing
    private bool freezePlayer;

    /// <summary>
    /// If true, the camera can not be moved. Used during pause.
    /// </summary>
    private bool freezeCamera;

    // Some Animator tags
    private const string playerAnimatorTurn = "Turn";
    private const string playerAnimatorForward = "Forward";
    private const string playerAnimatorJump = "Jump";
    private const string playerAnimatorSwimming = "Swimming";
    private const string playerAnimatorClimb = "Climb";
    private const string playerAnimatorFalling = "Falling";
    private const string playerAnimatorRafting = "Rafting";

    private CapsuleCollider playerCollider;
    private CharacterController characterController;

    /// <summary>
    /// Set up player movement
    /// </summary>
	void Start()
    {
		// set up player stats
		PlayerStatManager = new PlayerStatManager();

        isGrounded = false;
        updateStats = true;
        isInShelter = false;
        IsByFire = false;

        defaultParent = transform.parent;

        // set the closest distance as nothing
        closestDistance = 0;

        // set up movement components
        landMovement = GetComponent<LandMovement>();
        waterMovement = GetComponent<WaterMovement>();
        movement = landMovement;
        movement.OnStateEnter();

        // set up tools
        PlayerTool[] tools = GetComponentsInChildren<PlayerTool>();
        Game.Instance.PlayerInstance.Toolbox = new PlayerTools(tools);

        // get main camera component
        playerCamera = Camera.main.GetComponent<CameraController>();

        //start checking for pneumonia
        StartCoroutine(CheckPneumonia());
        
        // set up rigidbody
        playerRigidbody = GetComponent<Rigidbody>();

        // Link this to the player instance
        // and update accessable player transform
        Game.Instance.PlayerInstance.WorldTransform = transform;
        Game.Instance.PlayerInstance.Controller = this;
        controlScheme = Game.Instance.Scheme;

        // subscribe to events
        Game.Instance.DebugModeSubscription += this.toggleDebugMode;
        Game.Instance.PauseInstance.ResumeUpdate += this.Resume;
        Game.Instance.PauseInstance.PauseUpdate += this.Pause;
        Game.Instance.PauseInstance.MenuPauseUpdate += this.MenuPause;

        // create event emitter
        eventEmitter = FMODUnity.RuntimeManager.CreateInstance(roofFootstepSoundEvent);

        playerCollider = GetComponent<CapsuleCollider>();
        characterController = GetComponent<CharacterController>();

    }

    /// <summary>
    /// Get player input and update accordingly.
    /// </summary>
    void Update()
    {
        UpdatePlayerStats();
        FindVisibleInteractables();

        if(!freezeCamera)
        {
	        // check for camera related input
	        if (Input.GetKeyDown(controlScheme.CameraLeft))
	        {
	            playerCamera.RotateLeft();
	        }
	        if (Input.GetKeyDown(controlScheme.CameraRight))
	        {
	            playerCamera.RotateRight();
	        }
	        if (Input.GetKey(controlScheme.CameraZoomInKey))
	        {
	            playerCamera.Zoom(buttonZoomAmount);
	        }
	        if (Input.GetKey(controlScheme.CameraZoomOutKey))
	        {
	            playerCamera.Zoom(-buttonZoomAmount);
	        }
	        playerCamera.Zoom(Input.GetAxis(controlScheme.CameraZoomAxis));
	    }

        if (!freezePlayer)
        {
            // if the player has a tool equipped
            PlayerTools toolbox = Game.Instance.PlayerInstance.Toolbox;
            if (toolbox.HasEquipped)
            {
                if (Input.GetKeyDown(controlScheme.UseTool))
                {
                    // Check if the mouse was clicked over a UI element
                    if (!EventSystem.current.IsPointerOverGameObject())
                    {
                        // TODO: Check to see if Use returns and item
                        // if so maybe show it off then put it in the player's inventory
                        toolbox.EquippedTool.Use();
                    }
                }

                // don't check for other input since we are currently using a tool
                if (toolbox.EquippedTool.InUse)
                {
                    return;
                }
            }

            // if the player is near an interactable item
            if (interactable != null)
            {
                if (Input.GetKeyDown(controlScheme.Action))
                {
                    interactable.PerformAction();
                }
            }
        }

        // Debug mode flight controls
        if (Game.Instance.DebugMode)
        {
            if (Input.GetKey(debugFlightUp))
            {
                playerRigidbody.transform.Translate(Vector3.up * verticalSpeed);
            }

            if (Input.GetKey(debugFlightDown))
            {
                playerRigidbody.transform.Translate(Vector3.down * verticalSpeed);
            }
        }
    }

    /// <summary>
    /// Get player input and update accordingly
    /// </summary>
    void FixedUpdate()
    {
        if (!freezePlayer)
        {
            // don't move if a tool is currently in use or if the player is set to be frozen.
            PlayerTools toolbox = Game.Instance.PlayerInstance.Toolbox;
            if (toolbox != null && (toolbox.HasEquipped && toolbox.EquippedTool.InUse || freezePlayer))
            {
                return;
            }

            Vector3 direction = Vector3.zero;
            bool sprinting = Input.GetKey(controlScheme.Sprint);

            // Determine current direction of movement relative to camera
            if (Input.GetKey(controlScheme.Forward)
                || Input.GetKey(controlScheme.ForwardSecondary))
            {
                direction += getDirection(playerCamera.CurrentView.forward);
            }
            if (Input.GetKey(controlScheme.Back)
                || Input.GetKey(controlScheme.BackSecondary))
            {
                direction += getDirection(-playerCamera.CurrentView.forward);
            }
            if (Input.GetKey(controlScheme.Left)
                || Input.GetKey(controlScheme.LeftSecondary))
            {
                direction += getDirection(-playerCamera.CurrentView.right);
            }
            if (Input.GetKey(controlScheme.Right)
                || Input.GetKey(controlScheme.RightSecondary))
            {
                direction += getDirection(playerCamera.CurrentView.right);
            }

            CheckGround();

            if (direction != Vector3.zero)
            {
                movement.Move(direction, sprinting, playerAnimator);

				if (isGrounded) 
				{
					eventEmitter.setPitch (movement.Speed);
					StartWalkingSound ();
				}
            }
            else
            {
                movement.Idle(playerAnimator);
                StopWalkingSound();
            }

            // can't jump while in debug mode
            if ( (isGrounded || IsInWater) && !Game.Instance.DebugMode && Input.GetKeyDown(controlScheme.Jump))
            {
                freezePlayer = true;
                StartCoroutine(ClimbCoroutine());
            }

			// Check if the player is close to water
			RaycastHit hit;
			// We have to raycast in front of the player.
			if (Physics.Raycast (transform.position + (playerAnimator.transform.forward * distanceToCheckWater), -Vector3.up, out hit)) 
			{
				if (hit.collider.CompareTag(waterTag))
				{
					isWaterInView = true;
				}
                else
                {
                    isWaterInView = false;
                }
			}
        }
    }

    /// <summary>
    /// Gets the direction without accounting for the y axis.
    /// </summary>
    /// <returns>The direction.</returns>
    /// <param name="direction">Direction.</param>
    private Vector3 getDirection(Vector3 direction)
    {
        Vector3 scratchDirection = direction;
        scratchDirection.y = 0;
        return scratchDirection;
    }

    /// <summary>
    /// Updates the player's stats.
    /// </summary>
    private void UpdatePlayerStats()
    {
        // Only calculate fall damage when landing on the ground
        if (isGrounded)
        {
			PlayerStatManager.HealthRate.TakeFallDamage ((int)movement.CurrentFallDammage);
            HealthUpdatedEvent.Invoke();
        }
    }

	/// <summary>
	/// Updates the health.
	/// </summary>
	/// <returns>The health.</returns>
	private IEnumerator UpdateHealth()
	{
		while (updateStats) 
		{
			yield return new WaitForSeconds(PlayerStatManager.HealthRate.HealthDelay);	
			if (!PlayerStatManager.StopStats) 
			{
				Game.Instance.PlayerInstance.Health = Mathf.Clamp (
					Game.Instance.PlayerInstance.Health + PlayerStatManager.HealthRate.HealthAmount, 
					0,
					Game.Instance.PlayerInstance.MaxHealth);
			}
		}
	}

    /// <summary>
    /// Updates hunger.
    /// </summary>
    /// <returns>The hunger.</returns>
    private IEnumerator UpdateHunger()
    {
		while (updateStats)
		{
			yield return new WaitForSeconds (PlayerStatManager.HungerRate.HungerDelay);
			if (!PlayerStatManager.StopStats) 
			{
				Game.Instance.PlayerInstance.Hunger = Mathf.Clamp (
					Game.Instance.PlayerInstance.Hunger + PlayerStatManager.HungerRate.HungerAmount,
					0,
					Game.Instance.PlayerInstance.MaxHunger);
			}
		}
    }

    /// <summary>
    /// Updates warmth.
    /// TOOD: Refactor to use more intuitive decrease/increase rate system.
    /// </summary>
    /// <returns>The warmth.</returns>
	private IEnumerator UpdateWarmth()
    {
		while (updateStats)
		{
			yield return new WaitForSeconds(PlayerStatManager.WarmthRate.WarmthDelay);

			if (!PlayerStatManager.StopStats) 
			{
				Game.Instance.PlayerInstance.Warmth = Mathf.Clamp (
					Game.Instance.PlayerInstance.Warmth + PlayerStatManager.WarmthRate.WarmthAmount,
					0,
					Game.Instance.PlayerInstance.MaxWarmth);
			}
		}
    }

    private IEnumerator CheckPneumonia()
    {
        while (updateStats)
        {
            yield return new WaitForSeconds(pneumoniaCheckTime);

            // if player's health reaches a certain point they have a chance of sickness
            if (Game.Player.Controller.IsSick && Game.Player.Warmth < pnuemoniaWarmthThreshold)
            {
                if (RandomUtility.RandomPercent <= pnuemoniaChance)
                {
                    Game.Player.HealthStatus = PlayerHealthStatus.Pneumonia;
                }
            }
        }
    }

    /// <summary>
    /// Check if grounded, as well as set animation states for if we're swimming, walking or falling
    /// </summary>
    private void CheckGround()
    {
        bool belowWater = (Game.Instance.WaterLevelHeight > PlayerIKSetUp.transform.position.y + waterWadeHeight);


        if (IsOnRaft)
        {
            playerAnimator.SetBool(playerAnimatorRafting, true);
            PlayerAnimator.SetBool(playerAnimatorFalling, false);
            PlayerAnimator.SetFloat(playerAnimatorForward, 0f);
            PlayerAnimator.SetBool(playerAnimatorSwimming, false);
            PlayerAnimator.SetFloat(playerAnimatorTurn, 0f);
        }
        // If the player is low enough to be in the water
        else if (belowWater)
        {
            if (movement != waterMovement)
            {
                movement.OnStateExit();
                movement = waterMovement;
                movement.OnStateEnter();
            }
            movement.Idle(playerAnimator);
            playerAnimator.SetBool(playerAnimatorSwimming, true);
            playerAnimator.SetBool(playerAnimatorRafting, false);
        }
        else
        { 
            if(movement != landMovement)
            {
                movement.OnStateExit();
                movement = landMovement;
                movement.OnStateEnter();
            }

            // Check if the player is close enough to the ground
            RaycastHit hit;
            // We have to raycast SLIGHTLY above the player's bottom. Because if we start at the bottom there's a good chance it'll end up going through the ground.
            if (Physics.Raycast(transform.position + new Vector3(0f, groundedRaycastHeight, 0f), Vector3.down, out hit, groundedThreshold, groundedMask))
            {
                isGrounded = true;
                playerAnimator.SetBool(playerAnimatorFalling, false);
                // update movement
                if (hit.collider.gameObject.layer == groundedMask && movement != landMovement && !belowWater)
                {
                    playerAnimator.SetBool(playerAnimatorSwimming, false);
                    movement.Idle(playerAnimator);
                }
            }
            else if(!belowWater)
            {
                isGrounded = false;
                playerAnimator.SetBool(playerAnimatorFalling, true);
            }
            else
            {
                isGrounded = true;
            }
            if(!belowWater)
            {
                playerAnimator.SetBool(playerAnimatorSwimming, false);
            }
            playerAnimator.SetBool(playerAnimatorRafting, false);
        }
    }

    /// <summary>
    /// Player boards the raft and assumes raft controls until the player dismounts.
    /// </summary>
    public void BoardRaft(RaftMovement raftMovement)
    {
        if (movement != raftMovement)
        {
            movement = raftMovement;

            // place player on raft
            Vector3 position = raftMovement.gameObject.transform.position;
            transform.parent = raftMovement.transform;
            transform.DOLocalMove(new Vector3(0f, raftMovement.PlayerStandHeight, 0f), raftBoardTime);
            setPlayerCollision(false);
        }
        else
        {
            DisembarkRaft(raftMovement);
        }

    }

    /// <summary>
    /// Player disembarks the raft and resumes player movement.
    /// </summary>
    /// <param name="raftMovement"></param>
    public void DisembarkRaft(RaftMovement raftMovement)
    {
        movement = landMovement;
        PlayerAnimator.SetBool(playerAnimatorFalling, false);
        PlayerAnimator.SetFloat(playerAnimatorForward, 0f);
        PlayerAnimator.SetBool(playerAnimatorSwimming, false);
        PlayerAnimator.SetFloat(playerAnimatorTurn, 0f);
        transform.parent = defaultParent;
        setPlayerCollision(true);

    }

    /// <summary>
    /// When the player is on the raft, or other times when it shouldn't have collision.
    /// </summary>
    private void setPlayerCollision(bool enable)
    {
            playerCollider.enabled = enable;
            characterController.enabled = enable;
    }

    /// <summary>
    /// Returns true of the player is on solid ground
    /// </summary>
    public bool IsOnLand
    {
        get { return movement is LandMovement; }
    }

    /// <summary>
    /// Returns true if the player is swimming in water
    /// </summary>
    public bool IsInWater
    {
        get { return movement is WaterMovement; }
    }

    /// <summary>
    /// Returns true if the player is currently on a raft
    /// </summary>
    public bool IsOnRaft
    {
        get { return movement is RaftMovement; }
    }

    /// <summary>
    /// Returns true if player is sick.
    /// </summary>
    public bool IsSick
    {
        get
        {
            return Game.Player.HealthStatus != PlayerHealthStatus.None;
        }
    }

    /// <summary>
    /// Finds all the interactable objects within the player's field of view
    /// </summary>
    void FindVisibleInteractables()
    {
        if (IsOnLand || IsInWater)
        {
            // find the interactable objects within a sphere around the character
            Collider[] interactablesInRadius = Physics.OverlapSphere(playerAnimator.transform.position, ViewRadius, interactablesMask);

            // check all the items within the radius 
            for (int i = 0; i < interactablesInRadius.Length; ++i)
            {
                // get the direction to the interactable
                Transform target = interactablesInRadius[i].transform;
                Vector3 targetDir = (target.position - playerAnimator.transform.position).normalized;

                // check if angle between item is within view angle. The view angle divided by 2 should make up the 
                // the negative and postive of the view angle, so if the angle is less than half the view angle than 
                // it is in view.
                if (Vector3.Angle(playerAnimator.transform.forward, targetDir) < ViewAngle / 2)
                {
                    float targetDist = Vector3.Distance(playerAnimator.transform.position, target.position);
              
                    CheckClosestInteractable(interactablesInRadius[i], targetDist);
                }
            }

            // show item if closest item and stop showing previous item
            if (closestInteractable != prevInteractable)
            {
                // only stop showing if there was a previous collider
                if (prevInteractable != null && prevInteractable.CompareTag(interactiveTag) && interactable != null && interactable.GetComponentInChildren<InteractableObject>() != null)
                {
                    interactable.Show = false;
                    if (interactable.GetComponent<GlowObjectCmd>() != null)
                    {
						interactable.GetComponent<GlowObjectCmd> ().OutOfViewColor ();
                    }
                    interactable = null;
                }

                if (closestInteractable != null && closestInteractable.CompareTag(interactiveTag))
                {
                    interactable = closestInteractable.GetComponent<InteractableObject>();
                    interactable.Show = true;
                    if (interactable.GetComponent<GlowObjectCmd>() != null)
                    {
						interactable.GetComponent<GlowObjectCmd> ().InViewColor ();
                    }
                }
            }

            closestDistance = 0;
            prevInteractable = closestInteractable;
            closestInteractable = null;
        }
    }

    /// <summary>
    /// Check if an interactable is the closest interactable in view.
    /// </summary>
    /// <param name="interactable"></param>
    /// <param name="distance"></param>
    public void CheckClosestInteractable(Collider target, float targetDist)
    {
        // set first found interactable object as the closest item
        if (closestDistance == 0)
        {
            closestInteractable = target;
            closestDistance = targetDist;
        }

        // if an interactable object is closer than previous closest object, set it as the closest
        else if (targetDist < closestDistance)
        {
            closestInteractable = target;
            closestDistance = targetDist;
        }
    }

    /// <summary>
    /// Gets the direction of the angle. Used for editor mode to see how big the field of view will be.
    /// </summary>
    /// <param name="angleInDegrees"></param>
    /// <returns></returns>
    public Vector3 DirFromAngle(float angleInDegrees)
    {
        // shifts the angle to the front of the character.
        angleInDegrees += transform.eulerAngles.y;
        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }

    /// <summary>
    /// Returns the closest interactable item.
    /// </summary>
    /// <returns></returns>
    public GameObject ClosestItem()
    {
        return closestInteractable.gameObject;
    }

    /// <summary>
    /// Returns true if the player is currently in a shelter
    /// </summary>
    public bool IsInShelter
    {
        get
        {
            return isInShelter;
        }
        set
		{
            isInShelter = value;
        }
    }

    /// <summary>
    /// If the player is near a fire it returns true.
    /// </summary>
    public bool IsByFire
    {
        get
        {
            return isByFire;
        }
        set
        {
            isByFire = value;
        }
    }

	/// <summary>
	/// Gets a value indicating whether player is by water.
	/// </summary>
	/// <value><c>true</c> if this instance is by water; otherwise, <c>false</c>.</value>
	public bool IsWaterInView
	{
		get
		{
			return isWaterInView;
		}
	}

    /// <summary>
    /// If the player is reading returns true.
    /// </summary>
    public bool IsReading
    {
        get
        {
            return isReading;
        }
        set
        {
            if (value)
            {
                freezePlayer = true;
            }
            else
            {
                freezePlayer = false;
            }

            isReading = value;
        }
    }

    /// <summary>
    /// Resume stat changes.
    /// </summary>
    public void Resume()
    {
        updateStats = true;
		freezePlayer = false;
		freezeCamera = false;

		System.Func<IEnumerator> updateWarmthFunction = UpdateWarmth;
		System.Func<IEnumerator> updateHungerFunction = UpdateHunger;
		System.Func<IEnumerator> updateHealthFunction = UpdateHealth;

		if(warmthCoroutine != null)
		{
			StopCoroutine(warmthCoroutine);
		}

		if(hungerCoroutine != null)
		{
			StopCoroutine(hungerCoroutine);
		}

		if(healthCoroutine != null)
		{
			StopCoroutine(healthCoroutine);
		}

		warmthCoroutine = updateWarmthFunction();
		healthCoroutine = updateHealthFunction();
		hungerCoroutine = updateHungerFunction();

		StartCoroutine(warmthCoroutine);
		StartCoroutine(healthCoroutine);
		StartCoroutine(hungerCoroutine);
    }

    /// <summary>
    /// Pause stat changes.
    /// </summary>
    public void Pause()
    {
        updateStats = false;
		MenuPause();
    }

    /// <summary>
    /// Handles the pausing for menus.
    /// </summary>
    public void MenuPause()
    {
		freezePlayer = true;
		freezeCamera = true;
    }

    /// <summary>
    /// Set up or tear down any configuration neccisary for debug mode.
    /// </summary>
    private void toggleDebugMode()
    {
        if (Game.Instance.DebugMode)
        {
            playerRigidbody.useGravity = false;
        }
        else
        {
            playerRigidbody.useGravity = true;
        }
    }

    /// <summary>
    /// Starts the walking sound.
    /// </summary>
    public void StartWalkingSound()
    {
        if (eventEmitter != null)
        {
            FMOD.Studio.PLAYBACK_STATE state = FMOD.Studio.PLAYBACK_STATE.STOPPED;
            eventEmitter.getPlaybackState(out state);

            if (state != FMOD.Studio.PLAYBACK_STATE.PLAYING)
            {
                eventEmitter.start();
            }
        }
    }

    /// <summary>
    /// Stops the walking sound.
    /// </summary>
    public void StopWalkingSound()
    {
        if (eventEmitter != null)
        {
            eventEmitter.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        }
    }

    /// <summary>
    /// If the player can climb, we run code to get the player up the ledge
    /// If the player can not climb, we call the code to jump.
    /// </summary>
    /// <returns></returns>
    IEnumerator ClimbCoroutine()
    {
        Transform lH, rH, handHolder;
        rH = PlayerIKSetUp.GetGoalIK(AvatarIKGoal.RightHand).target;
        rH.transform.localPosition = transform.right * handSpacing;
        lH = PlayerIKSetUp.GetGoalIK(AvatarIKGoal.LeftHand).target;
        lH.transform.localPosition = -transform.right * handSpacing;
        handHolder = lH.parent;

        // Raycast to see if there is a ledge in front of the player.
        RaycastHit hit1 = new RaycastHit(), hit2 = new RaycastHit(), hit3 = new RaycastHit(), heightPoint = new RaycastHit();
  
        if (ClimbingRaycasts(lH, rH, ref hit1, ref hit2, ref hit3, ref heightPoint))
        {
            // From here on out things get complicated. The following math is used to get the angle needed to rotate the player to face the wall.
            float cLine, l1, l2, l3, d1, a1, p, d2;
            l3 = Vector3.Distance(hit1.point, rH.transform.position + new Vector3(0f, movement.GetRaycastHeight(), 0f));
            l1 = Vector3.Distance(hit3.point, lH.transform.position + new Vector3(0f, movement.GetRaycastHeight(), 0f));
            l2 = Vector3.Distance(hit2.point, PlayerIKSetUp.transform.position + new Vector3(0f, movement.GetRaycastHeight(), 0f));
            d1 = Vector3.Distance(rH.transform.position, PlayerIKSetUp.transform.position);
            // The player is already facing the wall perfectly.
            if (l1 == l3)
            {
                cLine = l2;
            }
            else
            {
                bool rotateClockwise = false;
                // Player needs to rotate counterclockwise
                if (l1 > l3)
                {
                    cLine = l3;
                    rotateClockwise = false;
                }
                // Player needs to rotate clockwise
                else
                {
                    cLine = l1;
                    rotateClockwise = true;
                }

                // Distance of triangle sides made by subdividing the trapezoid defined by the center raycast line, and the shorter one and the wall.
                a1 = Mathf.Sqrt(Mathf.Pow(handSpacing, 2f) + Mathf.Pow(cLine, 2f));

                d2 = Mathf.Sqrt(Mathf.Pow(l2 - cLine, 2f) + Mathf.Pow(d1, 2f));

                p = (Mathf.Pow(d2, 2f) + Mathf.Pow(l2 - cLine, 2f) - Mathf.Pow(d1, 2f)) / (2 * (l2 - cLine) * d2);
                p = Mathf.Acos(p) * Mathf.Rad2Deg;

                p = 180 - (p + 90f);

                //rotate the player to face the wall.
                if (rotateClockwise)
                {
                    playerAnimator.transform.DORotate(playerAnimator.transform.eulerAngles + new Vector3(0f, -p, 0f), startClimbTime);
                }
                else
                {
                    playerAnimator.transform.DORotate(playerAnimator.transform.eulerAngles + new Vector3(0f, p, 0f), startClimbTime);
                }
            }
            // Call the animator to play the climb animation
            movement.Climb(playerAnimator);
            // We're not swimming anymore.
            playerAnimator.SetBool(playerAnimatorSwimming, false);

            // Move hand targets up!
            if (movement == waterMovement)
            {
                lH.transform.position = new Vector3(lH.transform.position.x, heightPoint.point.y - waterMovement.SwimmingHeight, lH.transform.position.z);
                rH.transform.position = new Vector3(rH.transform.position.x, heightPoint.point.y - waterMovement.SwimmingHeight, rH.transform.position.z);
            }
            else
            {
                lH.transform.position = new Vector3(lH.transform.position.x, heightPoint.point.y, lH.transform.position.z);
                rH.transform.position = new Vector3(rH.transform.position.x, heightPoint.point.y, rH.transform.position.z);

            }

            // Code to move the players hands and the player forward to the wall.
            DOTween.To(() => PlayerIKSetUp.GetGoalIK(AvatarIKGoal.RightHand).IKPositionWeight, x => PlayerIKSetUp.GetGoalIK(AvatarIKGoal.RightHand).IKPositionWeight = x, 1f, startClimbTime);
            DOTween.To(() => PlayerIKSetUp.GetGoalIK(AvatarIKGoal.LeftHand).IKPositionWeight, x => PlayerIKSetUp.GetGoalIK(AvatarIKGoal.LeftHand).IKPositionWeight = x, 1f, startClimbTime);
            Tween tween = transform.DOMove(hit2.point, startClimbTime);

            yield return tween.WaitForCompletion();

            // Unparent the hands so they don't move up with the player.
            handHolder.SetParent(null);

            // Move the player up like they're climbing.
            // Height of the climb
            float climbUpY = heightPoint.point.y - (hit2.point.y - movement.GetRaycastHeight());
            tween = transform.DOMoveY(heightPoint.point.y, ClimbTime);
            yield return tween.WaitForCompletion();

            // Move the player forward and move the hand iks back to 0.
            transform.DOMove(climbForward * playerAnimator.transform.forward + transform.position, endClimbTime);
            DOTween.To(() => PlayerIKSetUp.GetGoalIK(AvatarIKGoal.RightHand).IKPositionWeight, x => PlayerIKSetUp.GetGoalIK(AvatarIKGoal.RightHand).IKPositionWeight = x, 0f, endClimbTime);
            tween = DOTween.To(() => PlayerIKSetUp.GetGoalIK(AvatarIKGoal.LeftHand).IKPositionWeight, x => PlayerIKSetUp.GetGoalIK(AvatarIKGoal.LeftHand).IKPositionWeight = x, 0f, endClimbTime);

            yield return tween.WaitForCompletion();

            //Reparent the hands now that the player has moved up.
            handHolder.SetParent(playerAnimator.transform);
            handHolder.transform.localPosition = Vector3.zero;

            freezePlayer = false;

            // If I climbed I am on land
            movement = landMovement;

        }
        // Call the normal jump.
        else
        {
            freezePlayer = false;
            movement.Jump(playerAnimator);
        }
    }

    /// <summary>
    /// Raycasts to the wall, if any of these casts fail we can't climb...
    /// Raycast order is Right hand, left hand, player center, then we find out if the ledge's height is within our max climb height
    /// The second to last cast uses a 9999f to represent a height above everything.
    /// The last raycast is to check to make sure there's no cieling above us to prevent climbing while indoors.
    /// Lastly check to make sure the ledge's height is within the max climb height for the movement type, and then make sure it's greater than the controller's step offset (this prevents climbing up things the player can just walk over)
    /// </summary>
    /// <param name="lH">The left hand's transform</param>
    /// <param name="rH">The right hand's transform</param>
    /// <param name="hit1">first raycast hit</param>
    /// <param name="hit2">second raycast hit</param>
    /// <param name="hit3">third raycast hit</param>
    /// <param name="heightPoint">fourth raycast hit</param>
    /// <returns></returns>
    private bool ClimbingRaycasts(Transform lH, Transform rH, ref RaycastHit hit1, ref RaycastHit hit2, ref RaycastHit hit3, ref RaycastHit heightPoint)
    {
        if (
            Physics.Raycast(rH.transform.position + new Vector3(0f, movement.GetRaycastHeight(), 0f), rH.transform.forward, out hit1, climbDistance, ClimbingRaycastMask, QueryTriggerInteraction.Ignore) &&
            Physics.Raycast(lH.transform.position + new Vector3(0f, movement.GetRaycastHeight(), 0f), lH.transform.forward, out hit3, climbDistance, ClimbingRaycastMask, QueryTriggerInteraction.Ignore) &&
            Physics.Raycast(PlayerIKSetUp.transform.position + new Vector3(0f, movement.GetRaycastHeight(), 0f), PlayerIKSetUp.transform.forward, out hit2, climbDistance, ClimbingRaycastMask, QueryTriggerInteraction.Ignore) &&
            Physics.Raycast(hit2.point + new Vector3(0f, 9999f, 0f) + PlayerIKSetUp.transform.forward * raycastClimbForward, Vector3.down, out heightPoint, Mathf.Infinity, ClimbingRaycastMask, QueryTriggerInteraction.Ignore) &&
            !Physics.Raycast(PlayerIKSetUp.transform.position + new Vector3(0f, movement.GetRaycastHeight(), 0f), Vector3.up, Vector3.Distance(PlayerIKSetUp.transform.position, heightPoint.point), ClimbingRaycastMask, QueryTriggerInteraction.Ignore) &&
            movement.GetClimbHeight() > heightPoint.point.y - PlayerIKSetUp.transform.position.y &&
            heightPoint.point.y - PlayerIKSetUp.transform.position.y > minClimbHeight &&
            heightPoint.point.y > PlayerIKSetUp.transform.position.y
        )
        {
            return true;
        }
        else
        {
            return false;
        }
    }


    /// <summary>
    /// Makes the player behave like they're on land. Called when the player teleports. Or by other things that'd move the player to be on land.
    /// </summary>
    public void SetIsOnLand()
    {
        if(IsOnRaft)
        {
            movement = landMovement;
            PlayerAnimator.SetBool(playerAnimatorFalling, false);
            PlayerAnimator.SetFloat(playerAnimatorForward, 0f);
            PlayerAnimator.SetBool(playerAnimatorSwimming, false);
            PlayerAnimator.SetFloat(playerAnimatorTurn, 0f);
            transform.parent = defaultParent;
            setPlayerCollision(true);
        }
        else
        { 
            movement = landMovement;
            PlayerAnimator.SetBool(playerAnimatorFalling, false);
            PlayerAnimator.SetFloat(playerAnimatorForward, 0f);
            PlayerAnimator.SetBool(playerAnimatorSwimming, false);
            PlayerAnimator.SetFloat(playerAnimatorTurn, 0f);
        }
    }
}
