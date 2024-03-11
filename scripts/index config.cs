using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using System.Linq;


public class ControllerIndexAssignment : MonoBehaviour
{
    private person PersonScript;
    private List<int> activeControllerIndices = new List<int>();
    private List<SteamVR_TrackedObject> trackedObjects = new List<SteamVR_TrackedObject>();

    void Start()
    {
        PersonScript = GetComponent<person>();
        trackedObjects.AddRange(GetComponents<SteamVR_TrackedObject>());

        if (PersonScript == null || trackedObjects.Count == 0)
        {
            Debug.LogError("Required components not found on the GameObject.");
            return;
        }

        StartCoroutine(AssignActiveIndices());
    }

    private IEnumerator AssignActiveIndices()
{
    bool isCalibrated = false;

    while (!isCalibrated)
    {
        // Assume starting calibration check.
        isCalibrated = PersonScript.player1.currentUpdate.Method.Name == "PostCalibrationUpdate";

        for (int i = 1; i <= 16 && !isCalibrated; i++)
        {
            foreach (var trackedObject in trackedObjects)
            {
                trackedObject.SetDeviceIndex(i);

                yield return null; // Wait for the next frame so the position and rotation can update.

                Vector3 startPosition = trackedObject.transform.position;
                Quaternion startRotation = trackedObject.transform.rotation;

                yield return new WaitForSeconds(0.1f); // Wait a bit for any potential movement.

                if (startPosition != trackedObject.transform.position || startRotation != trackedObject.transform.rotation)
                {
                    if (!activeControllerIndices.Contains(i))
                    {
                        Debug.Log($"Index {i} is active.");
                        activeControllerIndices.Add(i);
                    }
                }
            }
        }

        // Call PickIndices after each cycle of checking, before checking calibration status again.
        StartCoroutine(PickIndices());
   
        // Here, consider adding a slight delay to avoid overly frequent re-checks,
        // especially if you expect users might turn on trackers sequentially within short periods.
        yield return new WaitForSeconds(1); // Adjust this delay as needed.

        // Refresh the calibration status after performing assignments and before the next cycle.
        isCalibrated = PersonScript.player1.currentUpdate.Method.Name == "PostCalibrationUpdate";
    }

    Debug.Log($"Calibration complete. Active indices found: {activeControllerIndices.Count}");
    // Additional functionality related to the person script can be implemented here after calibration.
}

   private IEnumerator PickIndices()
{
    var indexPositions = new Dictionary<int, Vector3>();

    // Ensure trackedObjects are up-to-date.
    trackedObjects.Clear();
    trackedObjects.AddRange(GetComponentsInChildren<SteamVR_TrackedObject>());

    foreach (var index in activeControllerIndices)
    {
        SteamVR_TrackedObject tempTrackedObject = trackedObjects.FirstOrDefault();
        if(tempTrackedObject != null)
        {
            tempTrackedObject.SetDeviceIndex(index);
            
            // Wait a frame to ensure the position updates with the new index.
            yield return null;

            // Now capture the position.
            Vector3 currentPosition = tempTrackedObject.transform.position;

            // Store the index and position for sorting.
            indexPositions[index] = currentPosition;
        }
    }

    // After capturing positions, sort the indices based on Y position.
    var sortedIndices = indexPositions.OrderByDescending(kvp => kvp.Value.y).Select(kvp => kvp.Key).ToList();

    // Assign sorted indices to controllers. This part must also comply with coroutine logic.
    if (sortedIndices.Count > 0) AssignToController(PersonScript.conR, sortedIndices[0]);
    if (sortedIndices.Count > 1) AssignToController(PersonScript.conL, sortedIndices[1]);
    if (sortedIndices.Count > 2) AssignToController(PersonScript.kneeConR, sortedIndices[2]);
    if (sortedIndices.Count > 3) AssignToController(PersonScript.kneeConL, sortedIndices[3]);
}

private void AssignToController(GameObject controller, int index)
{
    SteamVR_TrackedObject trackedObject = controller.GetComponent<SteamVR_TrackedObject>();
    if (trackedObject != null)
    {
        trackedObject.SetDeviceIndex(index);
        Debug.Log($"Assigned index {index} to {controller.name}");
    }
}
}