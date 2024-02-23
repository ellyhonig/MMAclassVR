using System.Collections;
using UnityEngine;
using Valve.VR;
using System.Collections.Generic;

public class BodyPartConfiguration : MonoBehaviour
{
    public enum ModeSelection
    {
        ArmsOnly,
        LegsOnly
        // Add more modes as needed
    }

    public ModeSelection mode = ModeSelection.ArmsOnly;

    // Reference to SteamVR_TrackedObject components on the controllers
    public SteamVR_TrackedObject[] trackedObjects;

    private void Start()
    {
        StartCoroutine(ConfigureBasedOnMode());
    }

    private IEnumerator ConfigureBasedOnMode()
    {
        // Wait for SteamVR to initialize and for trackers to be active
        yield return new WaitForSeconds(2);

        // Filter active controllers
        // Filter active controllers and store them in a List<SteamVR_TrackedObject> instead of ArrayList
var activeControllers = new List<SteamVR_TrackedObject>();
foreach (var trackedObj in trackedObjects)
{
    if (trackedObj.isValid && trackedObj.transform.localRotation != Quaternion.identity)
    {
        activeControllers.Add(trackedObj);
    }
}

// Sort active controllers by y-coordinate using a lambda expression
activeControllers.Sort((a, b) => a.transform.position.y.CompareTo(b.transform.position.y));


        SteamVR_TrackedObject leftCon = null, rightCon = null;

        // Determine which controllers to use based on the selected mode
        switch (mode)
        {
            case ModeSelection.ArmsOnly:
                // Top two controllers
                leftCon = (SteamVR_TrackedObject)activeControllers[activeControllers.Count - 2];
                rightCon = (SteamVR_TrackedObject)activeControllers[activeControllers.Count - 1];
                break;
            case ModeSelection.LegsOnly:
                // Bottom two controllers
                leftCon = (SteamVR_TrackedObject)activeControllers[0];
                rightCon = (SteamVR_TrackedObject)activeControllers[1];
                break;
            // Add more modes as needed
        }

        // Determine left and right based on the forward direction of HMD
        if (leftCon.transform.position.x > rightCon.transform.position.x)
        {
            // Swap if leftCon is actually on the right side
            (leftCon, rightCon) = (rightCon, leftCon);
        }

        // Assign the detected controllers to person script components
        var personScript = GetComponent<person>();
        if (personScript != null)
        {
            personScript.conL = leftCon.gameObject;
            personScript.conR = rightCon.gameObject;
            // Adjust the person script or its members as needed
        }
    }
}

