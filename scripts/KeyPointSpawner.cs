using System.Collections.Generic;
using UnityEngine;

public class KeyPointSpawner : MonoBehaviour
{
    public GameObject hmd;
    public GameObject conR;
    public GameObject conL;
    public GameObject kneeConR;
    public GameObject kneeConL;
    private Recorder recorder; // Assigned in Unity Editor; the recorder holding the recording

     void Start()
    {
       recorder = GetComponent<Recorder>();
    if (recorder == null)
    {
        Debug.LogError("Recorder component not found on the same GameObject.");
    }
    }

    private void SpawnAndPosePlayerAtFrame(int frameIndex)
    {
        if (frameIndex < 0 || frameIndex >= recorder.currentRecord.frames.Count)
        {
            Debug.LogError($"Frame index {frameIndex} is out of bounds.");
            return;
        }

        // Using the parameterless constructor for player
        player playerInstance =new player(hmd.transform, conR.transform, conL.transform, kneeConR.transform, kneeConL.transform);
        // Apply the pose from the specified frame to the player
        recorder.currentRecord.frames[frameIndex].ApplyToPlayer(playerInstance);

        // Note: Additional setup or adjustments to playerInstance can be made here if necessary
    }

    public void SpawnBasicTrace()
    {
        if (recorder == null || recorder.currentRecord == null || recorder.currentRecord.frames.Count == 0)
        {
            Debug.LogError("Recorder or recorder's current record is not properly set up.");
            return;
        }

        // Ensure there's at least one frame to record
        if (recorder.currentRecord.frames.Count < 1)
        {
            Debug.LogError("The current record does not contain enough frames.");
            return;
        }

        int firstFrameIndex = 0;
        int lastFrameIndex = recorder.currentRecord.frames.Count - 1;
        int middleFrameIndex = lastFrameIndex / 2; // This will automatically floor the division for odd counts

        // Spawn and pose players at the first, middle, and last frames
        SpawnAndPosePlayerAtFrame(firstFrameIndex);
        if (middleFrameIndex != firstFrameIndex && middleFrameIndex != lastFrameIndex) // Check to avoid duplication in case of very short records
        {
            SpawnAndPosePlayerAtFrame(middleFrameIndex);
        }
        SpawnAndPosePlayerAtFrame(lastFrameIndex);
    }
}
