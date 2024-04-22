using System.Collections.Generic;
using UnityEngine;

public class KeyPointSpawner : MonoBehaviour
{
    public GameObject hmd;
    public GameObject conR;
    public GameObject conL;
    public GameObject kneeConR;
    public GameObject kneeConL;
    public delegate void UpdateDelegate();
    public Recorder recorder; // Assigned in Unity Editor; the recorder holding the recording
    public UpdateDelegate currentUpdate;
    public List<KeyFrame> keyFrameList;
    public int publicIndex;
    public int currentlySelectedKeyFrame;
    public TraceChecker traceChecker;
    public bool rewind = false;
    public Level currentLevel;
    public LevelEditor levelEditor;

void Start()
{
    recorder = GetComponent<Recorder>();
    keyFrameList = new List<KeyFrame>();
    if (recorder == null)
    {
        Debug.LogError("Recorder component not found on the same GameObject.");
    }
    traceChecker = new TraceChecker(this);
    currentLevel = new Level();
    levelEditor = new LevelEditor(this);
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
        //MakePlayerGreen(keyFramePlayer);
       // keyFramePlayer.Calibrate();
    }
    public void MakePlayerGreen(player playerToModify)
{
    foreach (GameObject bodyPart in playerToModify.bodyPartsDictionary.Values)
    {
        Renderer renderer = bodyPart.GetComponent<Renderer>();
        if (renderer != null)
        {
            Color currentColor = renderer.material.color;
            // Change the color to green while preserving the current alpha (transparency) value
            renderer.material.color = new Color(0f, 1f, 0f, .1f);
        }
    }
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
        // Check if there are more sections left in the level to test
        if (spawner.levelEditor.currentSection + 1 < spawner.currentLevel.keyframeIndicesSections.Count)
        {
            
            // Move to the next section
            spawner.levelEditor.switchToNextSection();

            // Reset the keyframe counter for the new section
            currentKeyFrame = 0;

            // Ensure all keyframes are initially deactivated
            DeactivateAllKeyframePlayers();

            // Start testing the next section
            startTestTrace();
        }
        else
        {
            // No more sections left in the level, stop the update loop
            Debug.Log("Finished testing all sections of the level.");
            spawner.Pause();
            spawner.recorder.PauseRecording();
        }
        return;
    }

    // Deactivate all keyframe players before checking the current one

    // Ensure the current keyframe's player model is visible
    if(spawner.keyFrameList.Count > currentKeyFrame) // Additional check to ensure index is in range
    {
        DeactivateAllKeyframePlayers();

        spawner.keyFrameList[currentKeyFrame].keyFramePlayer.bodyPartsParent.SetActive(true);
    }

    // Perform the limb check
    if (CheckAllLimbsAtKeyPoint(currentKeyFrame))
    {
        Debug.Log($"Keyframe {currentKeyFrame} passed the trace check.");
        spawner.keyFrameList[currentKeyFrame].keyFramePlayer.bodyPartsParent.SetActive(false);

        currentKeyFrame++;
       
    spawner.keyFrameList[currentKeyFrame].keyFramePlayer.bodyPartsParent.SetActive(true);

        // Optionally, play success sound here
    }

    // Optionally, you might want to deactivate the current keyframe player immediately or after some delay
    // For immediate deactivation (comment out if you want to keep it visible until the next update):
     
}

private void DeactivateAllKeyframePlayers()
{
    foreach (var keyFrame in spawner.keyFrameList)
    {
        if(keyFrame.keyFramePlayer.bodyPartsParent != null) // Check if the bodyPartsParent is set
        {
            keyFrame.keyFramePlayer.bodyPartsParent.SetActive(false);
        }
    }
}

  private bool CheckAllLimbsAtKeyPoint(int keyPointIndex)
{
    bool allLimbsCloseEnough = true;

    var keyFramePlayer = spawner.keyFrameList[keyPointIndex].keyFramePlayer;
    var playerToRecord = spawner.recorder.PlayerToRecord;

    // Iterate through the playerToRecord's limbs
    foreach (var limbName in playerToRecord.bodyPartsDictionary.Keys)
    {
        if (!keyFramePlayer.bodyPartsDictionary.ContainsKey(limbName))
        {
            Debug.LogWarning($"Limb {limbName} not found in keyFramePlayer's bodyPartsDictionary.");
            continue; // Skip this iteration if the limb doesn't exist in keyFramePlayer
        }

        // Fetch corresponding Transforms to be checked
        Transform recordLimbTransform = playerToRecord.bodyPartsDictionary[limbName].transform;
        Transform keyFrameLimbTransform = keyFramePlayer.bodyPartsDictionary[limbName].transform;

        Renderer recordLimbRenderer = playerToRecord.bodyPartsDictionary[limbName].GetComponent<Renderer>();
        Renderer keyFrameLimbRenderer = keyFramePlayer.bodyPartsDictionary[limbName].GetComponent<Renderer>();

        // Use some predefined tolerances or calculate them based on your needs
        Vector3 positionTolerance = new Vector3(0.5f, 0.5f, 0.5f);
        float rotationToleranceDegrees = 15f;

        if (!closeEnough(recordLimbTransform, keyFrameLimbTransform, positionTolerance, rotationToleranceDegrees))
        {
            // If not close enough, turn both limbs red and keyFramePlayer transparent
            if (recordLimbRenderer != null)
            {
                recordLimbRenderer.material.color = Color.red;
            }
            if (keyFrameLimbRenderer != null)
            {
                keyFrameLimbRenderer.material.color = new Color(1, 0, 0, 0.5f); // Red with 50% transparency
            }
            allLimbsCloseEnough = false; // At least one limb is not close enough
        }
        else
        {
            // If close enough, turn both limbs green and keyFramePlayer transparent
            if (recordLimbRenderer != null)
            {
                recordLimbRenderer.material.color = Color.green;
            }
            if (keyFrameLimbRenderer != null)
            {
                keyFrameLimbRenderer.material.color = new Color(0, 1, 0, 0.5f); // Green with 50% transparency
            }
        }
    }

    return allLimbsCloseEnough;
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

