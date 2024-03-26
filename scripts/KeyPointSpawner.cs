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
    public bool rewind = false;

     void Start()
    {
            recorder = GetComponent<Recorder>();
            keyFrameList = new List<KeyFrame>();
            if (recorder == null)
            {
                Debug.LogError("Recorder component not found on the same GameObject.");
            }
    }

    private float updateRate = 0.01f; // Run twice per second
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
        
            if (currentlySelectedKeyFrame < 0 || currentlySelectedKeyFrame >= keyFrameList.Count)
            {
                Debug.LogError($"currentlySelectedKeyFrame index {currentlySelectedKeyFrame} is out of bounds.");
                return;
            }
            if (publicIndex < 0 || publicIndex >= recorder.currentRecord.frames.Count)
            {
                Debug.LogError($"Frame index {publicIndex} is out of bounds.");
                publicIndex = 0;
                return;
            }
            //apply the frame to the keyframe player
            recorder.currentRecord.frames[publicIndex].ApplyToPlayer(keyFrameList[currentlySelectedKeyFrame].keyFramePlayer);
            
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
    public KeyFrame(KeyPointSpawner Spawner)
    {
        spawner = Spawner;
        keyFramePlayer = new player(spawner.hmd.transform, spawner.conR.transform, spawner.conL.transform, spawner.kneeConR.transform, spawner.kneeConL.transform);
        keyFramePlayer.Calibrate();
    }
}
}
