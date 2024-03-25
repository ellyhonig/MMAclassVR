using System.Collections.Generic;
using UnityEngine;

public class KeyPointSpawner : MonoBehaviour
{
    public GameObject hmd;
    public GameObject conR;
    public GameObject conL;
    public GameObject kneeConR;
    public GameObject kneeConL;
    private delegate void UpdateDelegate();
    private Recorder recorder; // Assigned in Unity Editor; the recorder holding the recording
    private UpdateDelegate currentUpdate;
    public List<KeyFrame> keyFrameList;
    public int publicIndex;
    public int currentlySelectedKeyFrame;

     void Start()
    {
            recorder = GetComponent<Recorder>();
            keyFrameList = new List<KeyFrame>();
            if (recorder == null)
            {
                Debug.LogError("Recorder component not found on the same GameObject.");
            }
    }

    void Update()
    {
        currentUpdate?.Invoke();
    }


    public void moveKeyFrame()
    {
        
            if (currentlySelectedKeyFrame < 0 || currentlySelectedKeyFrame >= keyFrameList.Count)
            {
                Debug.LogError($"currentlySelectedKeyFrame index {currentlySelectedKeyFrame} is out of bounds.");
                return;
            }
            if (publicIndex < 0 || publicIndex >= recorder.currentRecord.frames.Count)
            {
                Debug.LogError($"Frame index {publicIndex} is out of bounds.");
                return;
            }
            //apply the frame to the keyframe player
            recorder.currentRecord.frames[publicIndex].ApplyToPlayer(keyFrameList[currentlySelectedKeyFrame].keyFramePlayer);
    }
    public void addKeyFrame()
    {
        keyFrameList.Add(new KeyFrame(this));
        keyFrameList[keyFrameList.Count].indexInList = keyFrameList.Count;
        currentlySelectedKeyFrame = keyFrameList.Count;
        currentUpdate = moveKeyFrame;
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
}
public class KeyFrame
{
    public KeyPointSpawner spawner;
    public player keyFramePlayer;
    public int indexInList;
    public KeyFrame(KeyPointSpawner Spawner)
    {
        spawner = Spawner;
        keyFramePlayer = new player(spawner.hmd.transform, spawner.conR.transform, spawner.conL.transform, spawner.kneeConR.transform, spawner.kneeConL.transform);
    }
}
