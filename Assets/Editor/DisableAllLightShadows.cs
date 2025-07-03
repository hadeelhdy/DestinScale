using UnityEngine;
using UnityEditor;

public class DisableAllLightShadows
{
    [MenuItem("Tools/Disable All Light Shadows in Scene")]
    static void DisableShadows()
    {
        Light[] allLights = GameObject.FindObjectsByType<Light>(FindObjectsSortMode.None); // Updated method
        int count = 0;

        foreach (Light light in allLights)
        {
            if (light.shadows != LightShadows.None)
            {
                light.shadows = LightShadows.None;
                count++;
            }
        }

        Debug.Log($"Disabled shadows on {count} lights.");
    }
}