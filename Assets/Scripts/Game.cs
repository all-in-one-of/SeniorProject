﻿public class Game 
{
    private static Game instance;
	private bool debugMode;

    private Game ()
    {
        PlayerInstance = new Player();
		WeatherInstance = new WeatherSystem();
		PauseInstance = new PauseSystem();
		WorldItemFactoryInstance = new WorldItemFactory();
		ItemFactoryInstance = new ItemFactory();
		GameSettingsInstance = new GameSettings ();
		Scheme = GameSettingsInstance.Scheme;
        Loader = new GameLoader();
    }
 
    /// <summary>
    /// The current game instance. If there is no instance, one is created.
    /// </summary>
    public static Game Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new Game();
            }
            return instance;
        }
    }

    /// <summary>
    /// The current player instance.
    /// </summary>
    public Player PlayerInstance
    {
        get;
        private set;
    }

    /// <summary>
    /// The current city. Returns null if the city has not yet been populated.
    /// </summary>
    public City CityInstance
    {
        get;
        set;
    }

	/// <summary>
	/// Turns on debug mode.
	/// </summary>
	public bool DebugMode
	{
		get
		{
			return debugMode;
		}
		set
		{
			debugMode = value;
			if (DebugModeSubscription != null)
			{
				DebugModeSubscription();
			}
		}
	}

	public delegate void DebugModeDelegate();
	public event DebugModeDelegate DebugModeSubscription;

	/// <summary>
	/// Gets the weather instance.
	/// </summary>
	/// <value>The weather instance.</value>
	public WeatherSystem WeatherInstance
	{
		get;
		private set;
	}

    public bool IsRadioActive
    {
        get
        {
            return RadioInstance != null;
        }
    }

    public Radio RadioInstance
    {
        get;
        set;
    }

	/// <summary>
	/// The clock instance.
	/// </summary>
	private Clock clockInstance;

	/// <summary>
	/// Gets the clock instance.
	/// </summary>
	/// <value>The clock instance.</value>
	public Clock ClockInstance
	{
		get
		{
			if(this.clockInstance == null)
			{
				this.clockInstance = UnityEngine.MonoBehaviour.FindObjectOfType<Clock>();

				// check if clock instance was found
				if(this.clockInstance == null)
				{
					UnityEngine.Debug.LogError("No clock instance found in scene.");
					
					// throw error if not found
					throw new System.NullReferenceException();
				}
			}

			return this.clockInstance;
		}
	}

	/// <summary>
	/// The flood water instance.
	/// </summary>
	private FloodWater floodWaterInstance;

	/// <summary>
	/// Gets the height of the water level.
	/// </summary>
	/// <value>The height of the water level.</value>
	public float WaterLevelHeight
	{
		get
		{
			if(this.floodWaterInstance == null)
			{
				this.floodWaterInstance = UnityEngine.MonoBehaviour.FindObjectOfType<FloodWater>();

				// check if object was found in scene 
				if(this.floodWaterInstance == null)
				{
					UnityEngine.Debug.LogError("No flood water instance found in scene.");

					// throw error
					throw new System.NullReferenceException();
				}
			}

			return this.floodWaterInstance.WaterLevelHeight;
		}
	}

	/// <summary>
	/// Gets or private sets the pause instance.
	/// </summary>
	/// <value>The pause instance.</value>
	public PauseSystem PauseInstance
	{
		get;
		private set;
	}

	/// <summary>
	/// Gets the item factory instance which stores all data about items.
	/// </summary>
	/// <value>The item factory instance.</value>
	public ItemFactory ItemFactoryInstance
	{
		get;
		private set;
	}

	/// Gets the game settings instance.
	/// </summary>
	/// <value>The game settings instance.</value>
	public GameSettings GameSettingsInstance 
	{
		get;
		private set;
	}

	/// <summary>
	/// Gets the item factory instance which creates items in the world.
	/// </summary>
	/// <value>The world item factory instance.</value>
	public WorldItemFactory WorldItemFactoryInstance
	{
		get;
		private set;
	}

	/// <summary>
	/// Gets the control scheme configured for the game.
	/// </summary>
	/// <value>Player control scheme</value>
	public ControlScheme Scheme 
	{
		get;
		set;
	}

	/// <summary>
	/// Gets or sets the game view instance.
	/// </summary>
	/// <value>The game view instance.</value>
    public GameViewBehavior GameViewInstance
    {
    	get;
    	set;
    }

    /// <summary>
    /// Tracks and reports game loading.
    /// </summary>
    public GameLoader Loader
    {
        get;
        private set;
    }
}
