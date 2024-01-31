using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Recorder : MonoBehaviour
{
    private player PlayerToRecord;
    private MirroredPlayer mirroredPlayer;

    private Record currentRecord;
    private delegate void UpdateDelegate();
    private UpdateDelegate currentUpdate;

    void Start()
    {
        // Fetch the player object from the attached person class
        var personComponent = GetComponent<person>();
        if (personComponent != null)
        {
            PlayerToRecord = personComponent.player1;
            mirroredPlayer = personComponent.mirroredPlayer;
        }

        currentRecord = new Record();
        currentUpdate = null;
    }

    void Update()
    {
        currentUpdate?.Invoke();
    }

    public void StartRecording()
    {
        mirroredPlayer.SetUpdateTransformsMethod(mirroredPlayer.UpdateDummyTransforms);
        currentRecord = new Record();
        currentUpdate = RecordFrame;
    }

    public void PauseRecording()
    {
        mirroredPlayer.SetUpdateTransformsMethod(mirroredPlayer.UpdateDummyTransforms);
        Debug.Log("paused");
        currentUpdate = null;
    }

    public void PlayRecording()
    {
        currentRecord.currentFrame = 0;
        currentUpdate = PlayFrame;
        mirroredPlayer.SetUpdateTransformsMethod(null);
    }

    private void RecordFrame()
    {
        var frame = new Frame();
        frame.CapturePlayerState(PlayerToRecord);
        currentRecord.frames.Add(frame);
        Debug.Log("recording");
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
        Debug.Log("applying");
        currentRecord.currentFrame++;
    }
}

public class Record
{
    public List<Frame> frames;
    public int currentFrame;

    public Record()
    {
        frames = new List<Frame>();
        currentFrame = 0;
    }
}


public class Frame
{
    private Dictionary<string, (Vector3 position, Quaternion rotation)> data;

    public Frame()
    {
        data = new Dictionary<string, (Vector3, Quaternion)>();
    }

    public void CapturePlayerState(player playerToCapture)
    {
        data["hmd"] = (playerToCapture.hmd.localPosition, playerToCapture.hmd.localRotation);
        data["conR"] = (playerToCapture.conR.localPosition, playerToCapture.conR.localRotation);
        data["conL"] = (playerToCapture.conL.localPosition, playerToCapture.conL.localRotation);
        data["kneeConR"] = (playerToCapture.kneeConR.localPosition, playerToCapture.kneeConR.localRotation);
        data["kneeConL"] = (playerToCapture.kneeConL.localPosition, playerToCapture.kneeConL.localRotation);
    }

    public void ApplyToPlayer(player playerToApply)
    {
        // Mapping from part names to their respective Transform references
        var bodyParts = new Dictionary<string, Transform>
        {
            ["hmd"] = playerToApply.hmd,
            ["conR"] = playerToApply.conR,
            ["conL"] = playerToApply.conL,
            ["kneeConR"] = playerToApply.kneeConR,
            ["kneeConL"] = playerToApply.kneeConL
        };

        foreach (var part in data)
        {
            if (bodyParts.TryGetValue(part.Key, out Transform bodyPartTransform))
            {
                bodyPartTransform.localPosition = part.Value.position;
                bodyPartTransform.localRotation = part.Value.rotation;
            }
        }
    }
}
