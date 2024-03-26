using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class Recorder : MonoBehaviour
{
    public player PlayerToRecord;
    private MirroredPlayer mirroredPlayer;
    public Record currentRecord;
    private delegate void UpdateDelegate();
    private UpdateDelegate currentUpdate;
    private string saveFilePath;

    void Start()
    {
        var personComponent = GetComponent<person>();
        if (personComponent != null)
        {
            PlayerToRecord = personComponent.player1;
            mirroredPlayer = personComponent.mirroredPlayer;
        }

        currentRecord = new Record();
        currentUpdate = null;

        // Initialize save file path
        saveFilePath = Path.Combine(Application.persistentDataPath, "recording.json");
    }

    void Update()
    {
        currentUpdate?.Invoke();
    }

    public void StartRecording()
    {
        currentRecord = new Record();
        currentUpdate = RecordFrame;
    }

    public void PauseRecording()
    {
        currentUpdate = null;
    }

    public void PlayRecording()
    {
        currentRecord.currentFrame = 0;
        currentUpdate = PlayFrame;
    }

    private void RecordFrame()
    {
        var frame = new Frame();
        frame.CapturePlayerState(PlayerToRecord);
        currentRecord.frames.Add(frame);
    }

    private void PlayFrame()
    {
        if (currentRecord.currentFrame >= currentRecord.frames.Count)
        {
            PlayRecording();//to reset current frame to 0
            return;
        }

        var frame = currentRecord.frames[currentRecord.currentFrame];
        frame.ApplyToPlayer(mirroredPlayer.mirroredPlayer);
        currentRecord.currentFrame++;
    }

    public void SaveRecording()
    {
        string json = JsonUtility.ToJson(currentRecord, true);
        File.WriteAllText(saveFilePath, json);
    }

    public void LoadRecording()
    {
        if (File.Exists(saveFilePath))
        {
            string json = File.ReadAllText(saveFilePath);
            currentRecord = JsonUtility.FromJson<Record>(json);
        }
    }
}
[System.Serializable]
public class Frame
{
    public List<FrameData> data = new List<FrameData>();

    public Frame() { }

    public void CapturePlayerState(player playerToCapture)
{
    // Clear existing data
    data.Clear();

    if (playerToCapture == null) 
    {
        Debug.LogError("playerToCapture is null");
        return;
    }

    // Automatically capture data for all body parts in the dictionary
    foreach (var item in playerToCapture.bodyPartsDictionary)
    {
        Transform partTransform = item.Value.transform; // Access the transform of the GameObject
        data.Add(new FrameData(item.Key, partTransform.localPosition, partTransform.localRotation));
    }
}


    public void ApplyToPlayer(player playerToApply)
    {
        foreach (FrameData frameData in data)
        {
            // You'll need to implement this part based on how you can map `frameData.partName` to your player's transforms
            // For example:
            Transform targetTransform = playerToApply.GetTransformByName(frameData.partName);
            if (targetTransform != null)
            {
                targetTransform.localPosition = frameData.position;
                targetTransform.localRotation = frameData.rotation;
            }
        }
    }
}
[System.Serializable]
public class FrameData
{
    public string partName;
    public Vector3 position;
    public Quaternion rotation;

    public FrameData(string name, Vector3 pos, Quaternion rot)
    {
        partName = name;
        position = pos;
        rotation = rot;
    }
}
[System.Serializable]
public class Record
{
    public List<Frame> frames = new List<Frame>();
    public int currentFrame;

    public Record()
    {
        currentFrame = 0;
    }
}
