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
    public Recorder recorder; // Assigned in Unity Editor; the recorder holding the recording
    private UpdateDelegate currentUpdate;
    public List<KeyFrame> keyFrameList;
    public int publicIndex;
    public int currentlySelectedKeyFrame;
    public TraceChecker traceChecker;
    public bool rewind = false;

     void Start()
    {
            recorder = GetComponent<Recorder>();
            keyFrameList = new List<KeyFrame>();
            if (recorder == null)
            {
                Debug.LogError("Recorder component not found on the same GameObject.");
            }
            traceChecker = new TraceChecker(this);
    }

    private float updateRate = 0.01f; // Run 10x per second
    private float nextUpdateTime = 0f;

void Update()
{
    if (Time.time >= nextUpdateTime)
    {
        currentUpdate?.Invoke();
        nextUpdateTime = Time.time + updateRate;
    }
}


   public void moveKeyFrame()
{
    // Check if the currently selected key frame is within bounds
    if (currentlySelectedKeyFrame < 0 || currentlySelectedKeyFrame >= keyFrameList.Count)
    {
        Debug.LogError($"currentlySelectedKeyFrame index {currentlySelectedKeyFrame} is out of bounds.");
        return;
    }

    // Check if the recorder's frame list is empty or if the publicIndex is out of bounds
    if (recorder.currentRecord.frames == null || recorder.currentRecord.frames.Count == 0 || publicIndex < 0 || publicIndex >= recorder.currentRecord.frames.Count)
    {
        Debug.LogError($"Frame index {publicIndex} is out of bounds or frames list is empty.");
        // Consider resetting publicIndex or handling this case more gracefully
        return;
    }

    // Apply the frame to the keyframe player
    recorder.currentRecord.frames[publicIndex].ApplyToPlayer(keyFrameList[currentlySelectedKeyFrame].keyFramePlayer);
    keyFrameList[currentlySelectedKeyFrame].assignedFrame = publicIndex;
}

    public void addKeyFrame()
    {
        keyFrameList.Add(new KeyFrame(this));
        keyFrameList[keyFrameList.Count - 1].indexInList = keyFrameList.Count - 1;
        currentlySelectedKeyFrame = keyFrameList.Count - 1;
        currentUpdate = moveKeyFrame;
    }
    public void Pause()
    {
        currentUpdate = null;
    }
    

    
public class KeyFrame
{
    public KeyPointSpawner spawner;
    public player keyFramePlayer;
    public int indexInList;
    public int assignedFrame;
    public KeyFrame(KeyPointSpawner Spawner)
    {
        spawner = Spawner;
        keyFramePlayer = new player(spawner.hmd.transform, spawner.conR.transform, spawner.conL.transform, spawner.kneeConR.transform, spawner.kneeConL.transform);

       // keyFramePlayer.Calibrate();
    }
}
}

public class TraceChecker
{
    public KeyPointSpawner spawner;
    public int currentKeyFrame = 0;

    // Constructor for TraceChecker
    public TraceChecker(KeyPointSpawner spawner)
    {
        this.spawner = spawner;
    }

    public void startTestTrace()
    {
        currentKeyFrame = 0; // Start from the first keyframe
        spawner.recorder.currentUpdate = testTrace; // Set the recorder to call testTrace on update
    }

    public void testTrace()
    {
        if (currentKeyFrame >= spawner.keyFrameList.Count)
        {
            //Debug.Log("Finished testing all keyframes.");
            spawner.Pause(); // Stop the update loop
            return;
        }

        // Perform the limb check
        if (CheckAllLimbsAtKeyPoint(currentKeyFrame))
        {
            // If all limbs are close enough, disable the keyframe player's body parts parent GameObject
            spawner.keyFrameList[currentKeyFrame].keyFramePlayer.bodyPartsParent.SetActive(false);
            Debug.Log($"Keyframe {currentKeyFrame} passed the trace check.");
            currentKeyFrame++;
            //play success sound
        }
       

         // Move to the next keyframe for the next update
    }

    private bool CheckAllLimbsAtKeyPoint(int keyPointIndex)
    {
        // Assuming the existence of a method to fetch the current player state for comparison
        // and that both keyFramePlayer and PlayerToRecord have their limbs stored in a similar dictionary structure for direct comparison

        var keyFramePlayer = spawner.keyFrameList[keyPointIndex].keyFramePlayer;
        var playerToRecord = spawner.recorder.PlayerToRecord; // Assuming this is accessible

        foreach (var limbName in keyFramePlayer.bodyPartsDictionary.Keys)
        {
            // Fetch corresponding Transforms to be checked
            Transform keyFrameLimbTransform = keyFramePlayer.bodyPartsDictionary[limbName].transform;
            Transform recordLimbTransform = playerToRecord.bodyPartsDictionary[limbName].transform;

            // Use some predefined tolerances or calculate them based on your needs
            Vector3 positionTolerance = new Vector3(0.1f, 0.1f, 0.1f);
            float rotationToleranceDegrees = 5f;

            if (!closeEnough(keyFrameLimbTransform, recordLimbTransform, positionTolerance, rotationToleranceDegrees))
            {
                Debug.Log(limbName);
                return false; // If any limb is not close enough, return false immediately
            }
        }

        // If all limbs are close enough
         Debug.Log("all good");
        return true;
    }

    public bool closeEnough(Transform reference, Transform toBeChecked, Vector3 positionTolerance, float rotToleranceDegrees)
    {
        // Check position tolerance
        bool isPositionCloseEnough = (reference.position - toBeChecked.position).magnitude <= positionTolerance.magnitude;

        // Check rotation tolerance. Quaternion.Angle returns the angle in degrees between two rotations.
        bool isRotationCloseEnough = Quaternion.Angle(reference.rotation, toBeChecked.rotation) <= rotToleranceDegrees;

        // Both position and rotation must be within tolerances for the function to return true
        return isPositionCloseEnough && isRotationCloseEnough;
    }
}

