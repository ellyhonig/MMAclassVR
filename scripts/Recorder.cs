using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class Recorder : MonoBehaviour
{
    private player PlayerToRecord;
    private MirroredPlayer mirroredPlayer;
    private Record currentRecord;
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
            PauseRecording();
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
if (playerToCapture == null) Debug.LogError("playerToCapture is null");
if (playerToCapture.hmd == null) Debug.LogError("playerToCapture.hmd is null");
if (data == null) Debug.LogError("data is null");
    // Capture the main body parts as well as the torso
    data.Add(new FrameData("HMD", playerToCapture.hmd.localPosition, playerToCapture.hmd.localRotation));
    data.Add(new FrameData("Chest", playerToCapture.Chest.bp.transform.localPosition, playerToCapture.Chest.bp.transform.localRotation));
    data.Add(new FrameData("Hip", playerToCapture.Hip.bp.transform.localPosition, playerToCapture.Hip.bp.transform.localRotation));
    data.Add(new FrameData("Torso", playerToCapture.playerTorso.upperTorso.transform.localPosition, playerToCapture.playerTorso.upperTorso.transform.localRotation));
    
    // Capture shoulders, elbows, and hands for both chest and hips
    CaptureLimbData(playerToCapture.Chest, "Chest");
    CaptureLimbData(playerToCapture.Hip, "Hip");
}
private void CaptureLimbData(chest chestPart, string prefix)
{
    // Existing captures
    data.Add(new FrameData(prefix + "ShoulderR", chestPart.shoulderR.bp.transform.localPosition, chestPart.shoulderR.bp.transform.localRotation));
    data.Add(new FrameData(prefix + "ShoulderL", chestPart.shoulderL.bp.transform.localPosition, chestPart.shoulderL.bp.transform.localRotation));
    data.Add(new FrameData(prefix + "ElbowR", chestPart.shoulderR.elbow.bp.transform.localPosition, chestPart.shoulderR.elbow.bp.transform.localRotation));
    data.Add(new FrameData(prefix + "ElbowL", chestPart.shoulderL.elbow.bp.transform.localPosition, chestPart.shoulderL.elbow.bp.transform.localRotation));
    data.Add(new FrameData(prefix + "HandR", chestPart.shoulderR.hand.bp.transform.position, chestPart.shoulderR.hand.bp.transform.localRotation));
    data.Add(new FrameData(prefix + "HandL", chestPart.shoulderL.hand.bp.transform.position, chestPart.shoulderL.hand.bp.transform.localRotation));

        // Assuming each tricep and forearm has a direct Transform reference in your structure
    data.Add(new FrameData(prefix + "TricepR", chestPart.shoulderR.elbow.tricep.transform.localPosition, chestPart.shoulderR.elbow.tricep.transform.localRotation));
    data.Add(new FrameData(prefix + "TricepL", chestPart.shoulderL.elbow.tricep.transform.localPosition, chestPart.shoulderL.elbow.tricep.transform.localRotation));

    data.Add(new FrameData(prefix + "ForearmR", chestPart.shoulderR.hand.forearm.transform.localPosition, chestPart.shoulderR.hand.forearm.transform.localRotation));
    data.Add(new FrameData(prefix + "ForearmL", chestPart.shoulderL.hand.forearm.transform.localPosition, chestPart.shoulderL.hand.forearm.transform.localRotation));

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