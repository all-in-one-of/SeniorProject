﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;


/// <summary>
/// This class handles the rain effect that is present throughout the entire game.
/// 
/// 
/// CURRENT PROBLEMS
/// Currently there seems to be an issue with Unity running out of memory and crashing when editing the particle system.
/// I have plenty of ram to spare so I think something else is going on. People online have said it may have to do with sub-emitters so I'm going to forgoe them for now.
/// There is still a sub emitter on RainMain you can turn on but it may make the system unstable.
/// </summary>

[ExecuteInEditMode]
public class RainController : MonoBehaviour
{
    // Turn this on for testing. Ideally in the future other scripts will call functions in this script to change the weather
    public bool UseCustomValues;

    // I'm setting these systems as public so that the script can run in Edit Mode.
    // The main rain drops.
    public ParticleSystem MainRain;

    // The fog that falls to simulate extra rain.
    public ParticleSystem RainFog;

    private const float minRainLevel = 5f;

    // The ammount of rain that can fall on a range between 0 to 100
    public float RainLevel
    {
        get
        {
            return rainLevel;
        }
        set
        {
			// keep value between 0 and 100
			rainLevel = Mathf.Clamp(value, minRainLevel, 100f);
            UpdateParticleSystem();
        }
    }

    [SerializeField]
    [Range(0,20)]
    private float windMitigation = 8f;

    [SerializeField]
    [Range(0, 100)]
    private float rainLevel;

    //  When should rainFog kick in?
    [Range(0, 100)]
    [SerializeField]
    public float FogStartThreshold = 50f;

    // How much fog to rain there should be
    [SerializeField]
    private float fogMod;

    private float rainLevelModifier = 100;

    // The wind vector in XZ (considering Y is Up, but we're only concerned with 2d wind)
    public Vector2 WindVectorXZ
    {
        get
        {
            return windVectorXZ;
        }
        set
        {
            windVectorXZ = value;
            UpdateParticleSystem();
        }
    }
    [SerializeField]
    private Vector2 windVectorXZ;

    // How Much wind should effect the raind fog (Probably set this to a low number since it falls slower than the main rain. So we want it to move less. .4 seems close)
    [SerializeField]
    private float fogWindMod = .4f;

    private ParticleSystemRenderer MainRainRenderer;

    // The attribute associated with the particle shader that controlls the blend between the rain textures.
    private const string particleMaterialBlendAttribute = "_Opacity";

    // Used to create splashes on rain hits.
    [SerializeField]
    private FXSplashManager splashManager;
    [SerializeField]
    private float splashSize = .4f;
    private ParticleSystem rainParticleSystem;
    private List<ParticleCollisionEvent> collisionEvents = new List<ParticleCollisionEvent>();

    /// <summary>
    /// Grab the renderer to set the material's blend amount.
    /// </summary>
    void Awake()
    {
        // Grab the renderer.
        MainRainRenderer = MainRain.GetComponent<ParticleSystemRenderer>();

        rainParticleSystem = GetComponent<ParticleSystem>();

        if (splashManager == null)
        {
            splashManager = transform.parent.GetComponentInChildren<FXSplashManager>();
        }
    }

	/// <summary>
	/// Update this instance.
	/// </summary>
    void Update()
    {
		if (UseCustomValues)
        {
            UpdateParticleSystem();
        }
        else
        {
			this.WindVectorXZ = Game.Instance.WeatherInstance.WindDirection2d / this.windMitigation;
			this.RainLevel    = Game.Instance.WeatherInstance.StormStrength * rainLevelModifier;
        }

        // Rain level in the shader is evaluated between 0-1 where here it's 1-100
        Shader.SetGlobalFloat("_RainIntensity", RainLevel / 100f);
    }

    /// <summary>
    /// Sets values in the associated particle systems based off of public values on the script.
    /// </summary>
    private void UpdateParticleSystem()
    {
        //Set RainMain Emission to double rain level. At 200 emission we almost max out particles. (Depending on the height the rain spawns at this may change, but it'll be based on the camera in the future)
        ParticleSystem.EmissionModule RainEmission = MainRain.emission;
        RainEmission.rateOverTime = RainLevel * 2f;

        //set the RainFog emission rate, if it's less than the threshold turn off emission.
        RainEmission = RainFog.emission;
        if (RainLevel < FogStartThreshold)
        {
            RainEmission.rateOverTime = 0f;
        }
        else
        {
            RainEmission.rateOverTime = RainLevel * fogMod;
        }

        //If in editor, there's a chance the script is running in edit mode and we won't have the renderer set.   
        #if (UNITY_EDITOR)
        if (MainRainRenderer == null)
        {
            MainRainRenderer = GetComponent<ParticleSystemRenderer>();
        }
        #endif

        // Set the blend to be equal to the rain ammount
        MainRainRenderer.sharedMaterial.SetFloat(particleMaterialBlendAttribute, RainLevel);

        // Set the wind speed via velocity over lifetime
        ParticleSystem.VelocityOverLifetimeModule ParticleVelocity = MainRain.velocityOverLifetime;
        ParticleVelocity.x = WindVectorXZ.x;
        ParticleVelocity.y = -WindVectorXZ.y;
        ParticleVelocity = RainFog.velocityOverLifetime;
        ParticleVelocity.x = WindVectorXZ.x * fogWindMod;
        ParticleVelocity.z = -WindVectorXZ.y * fogWindMod;


        // Set the rotation of the rain based on the wind.
        ParticleSystem.MainModule rainMainModule = MainRain.main;
        float windVectorX = 0f;
        if (windVectorXZ.x == 0f)
        {
            windVectorX = .01f;
        }
        else
        {
            windVectorX = WindVectorXZ.x;
        }
        rainMainModule.startRotationX = Mathf.Deg2Rad * (-WindVectorXZ.magnitude - 90f);
        rainMainModule.startRotationY = (Mathf.Tan(WindVectorXZ.y/windVectorX));
    }

    /// <summary>
    /// Called when the rain hits something. Spawns a splash.
    /// </summary>
    /// <param name="other"></param>
    void OnParticleCollision(GameObject other)
    {
        if(splashManager.SplashPoolSize > 0)
        { 
            rainParticleSystem.GetCollisionEvents(other, collisionEvents);

            for (int i = 0; i < collisionEvents.Count; ++i)
            {
                splashManager.CreateSplash(collisionEvents[0].intersection, splashSize, 1f);
            }
        }
    }
}