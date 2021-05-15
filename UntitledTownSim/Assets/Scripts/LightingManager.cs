using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// this is based on "Probably Spoonie's" video at https://www.youtube.com/watch?v=m9hj9PdO328
// see also the original source code at https://www.youtube.com/redirect?event=video_description&redir_token=QUFFLUhqbS1Ca2lBZnl2azYxNS1FZG9ySzEtc3RaVW1FQXxBQ3Jtc0trQ0tTLUR1bkd2X0V3QmdIQ2NSbkMyeG05bUFFSTQ2MW9wM2JHRVhCMkRNaHRDdFBFcnB4OHJXbmhGMFAtRTBNMGpWQmFpQzBQcGhNcHBMM1UwaXVxRTQteklTcDNEOGNoTDV5eFJjemhERHBmOWtQWQ&q=https%3A%2F%2Fgist.github.com%2FGlynn-Taylor%2F08da28896147faa6ba8f9654057d38e6

[ExecuteAlways]
public class LightingManager : MonoBehaviour
{
    //Scene References
    [SerializeField] private Light DirectionalLight;
    [SerializeField] private LightingPreset Preset;
    //Variables
    [SerializeField, Range(0, 24)] private float TimeOfDay;
    [SerializeField, Range(-10, 10)] private float speedMultiplier;  // used to adjust the cycle time. Note that values < 0 will reverse it!
    [SerializeField, Range(1, 10)] private float nightSpeed; // how much to speed up late-night hours
    [SerializeField] private float maxIntensity = 1.5f;
     private float baseIntensity = 0f;
    [SerializeField] private float maxShadowStrength = 1f;
    [SerializeField] private float minShadowStrength = 0.2f;
    private float nightSpeedUpStart = 20f;
    private float nightSpeedUpEnd = 4f;
    private float dawn = 6f;
    private float dusk = 18f;
    private float noon = 12f;

    //Justin Added
    private int lastHour = 0;

    private GameObject[] allAgents;
 
    private void Start()
    {
        // default values
        speedMultiplier = 0.1f; 
        nightSpeed = 10.0f;
        baseIntensity = maxIntensity / 2f;

        allAgents = GameObject.FindGameObjectsWithTag("Agent");
    }

    private void Update()
    {
        if (Preset == null)
            return;

        if (Application.isPlaying)
        {
            //(Replace with a reference to the game time)
            // speed up the time in dead of night
            if (TimeOfDay > nightSpeedUpStart || TimeOfDay < nightSpeedUpEnd) // speed up the passage of night from 9pm to 3am
            {
                // TimeOfDay += Time.deltaTime * speedMultiplier * nightSpeed;
                TimeOfDay += Time.deltaTime * nightSpeed;
            }
            else
            {
                TimeOfDay += Time.deltaTime * speedMultiplier;

                // adjust light intensity and shadow softness for time of day
                if (TimeOfDay >= dawn && TimeOfDay <= noon)
                {
                    DirectionalLight.intensity = baseIntensity + (baseIntensity / (noon - dawn)) * (TimeOfDay - dawn);
                    DirectionalLight.shadowStrength = minShadowStrength + ((maxShadowStrength - minShadowStrength) / (noon - dawn)) * (TimeOfDay - dawn);
                }
                else if (TimeOfDay > noon && TimeOfDay <= dusk)
                {
                    DirectionalLight.intensity = baseIntensity + (baseIntensity / (dusk - noon)) * (dusk - TimeOfDay);
                    DirectionalLight.shadowStrength = minShadowStrength + ((maxShadowStrength - minShadowStrength) / (dusk - noon)) * (dusk - TimeOfDay);
                }
                else
                {
                    DirectionalLight.intensity = baseIntensity;
                    DirectionalLight.shadowStrength = minShadowStrength;

                }
            }
            TimeOfDay %= 24; //Modulus to ensure always between 0-24
            UpdateLighting(TimeOfDay / 24f);

            //Has the hour changed to a new hour?
            if(Mathf.FloorToInt(TimeOfDay) != lastHour)
            {
                lastHour = Mathf.FloorToInt(TimeOfDay);
                //Send a signal to all agents that an hour has passed.
                UpdateAgentsClocks();
            }

        }
        //When the application isn't playing
        else
        {
            UpdateLighting(TimeOfDay / 24f);
            //Has the hour changed to a new hour?
            if (Mathf.FloorToInt(TimeOfDay) != lastHour)
            {
                lastHour = Mathf.FloorToInt(TimeOfDay);
                //Send a signal to all agents that an hour has passed.
                UpdateAgentsClocks();
            }
        }
    }


    private void UpdateLighting(float timePercent)
    {
        //Set ambient and fog
        RenderSettings.ambientLight = Preset.AmbientColor.Evaluate(timePercent);
        RenderSettings.fogColor = Preset.FogColor.Evaluate(timePercent);

        //If the directional light is set then rotate and set it's color, I actually rarely use the rotation because it casts tall shadows unless you clamp the value
        if (DirectionalLight != null)
        {
            DirectionalLight.color = Preset.DirectionalColor.Evaluate(timePercent);

            DirectionalLight.transform.localRotation = Quaternion.Euler(new Vector3((timePercent * 360f) - 90f, 170f, 0));
        }

    }

    //Try to find a directional light to use if we haven't set one
    private void OnValidate()
    {
        if (DirectionalLight != null)
            return;

        //Search for lighting tab sun
        if (RenderSettings.sun != null)
        {
            DirectionalLight = RenderSettings.sun;
        }
        //Search scene for light that fits criteria (directional)
        else
        {
            Light[] lights = GameObject.FindObjectsOfType<Light>();
            foreach (Light light in lights)
            {
                if (light.type == LightType.Directional)
                {
                    DirectionalLight = light;
                    return;
                }
            }
        }
    }

    //Tell all AI agents what hour it is.
    private void UpdateAgentsClocks()
    {
        //Loop through them all
        foreach(GameObject agent in allAgents)
        {
            //Call the function to update them.
            //All agents should have a BT script so it's fine
            agent.GetComponent<PandaBTScripts>().UpdateTime(lastHour);
        }
    }
}