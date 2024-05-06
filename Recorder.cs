using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class Recorder : MonoBehaviour
{
    public player PlayerToRecord;
    public MirroredPlayer mirroredPlayer;
    public Record currentRecord;
    public delegate void UpdateDelegate();
    public UpdateDelegate currentUpdate;
    private AudioSource audioSource;

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
        audioSource = gameObject.AddComponent<AudioSource>();
    }

    void Update()
    {
        currentUpdate?.Invoke();
    }

    public void StartRecording()
    {
        currentRecord = new Record();
        audioSource.clip = Microphone.Start(null, true, 10, 44100); // Start a new audio recording
        currentUpdate = RecordFrame;
    }

    public void PauseRecording()
    {
        currentUpdate = null;
        if (Microphone.IsRecording(null))
        {
            Microphone.End(null); // Stop the microphone when pausing
        }
    }

    public void PlayRecording()
    {
        mirroredPlayer.EnableMirroredPlayerParent();
        currentRecord.currentFrame = 0;
        currentUpdate = PlayFrame;
    }

    private void RecordFrame()
    {
        var frame = new Frame();
        frame.CapturePlayerState(PlayerToRecord);

        // Capture and store audio data
        if (audioSource.clip && Microphone.IsRecording(null))
        {
            var audioData = new float[audioSource.clip.samples * audioSource.clip.channels];
            audioSource.clip.GetData(audioData, 0);
            currentRecord.audioSamples.Add(audioData); // Add to currentRecord's audio data list
        }

        currentRecord.frames.Add(frame);
    }

    private void PlayFrame()
    {
        if (currentRecord.currentFrame >= currentRecord.frames.Count)
        {
            PauseRecording(); // Stop playback when done
            return;
        }

        var frame = currentRecord.frames[currentRecord.currentFrame];
        frame.ApplyToPlayer(mirroredPlayer.mirroredPlayer);

        if (currentRecord.audioSamples.Count > currentRecord.currentFrame)
        {
            PlayAudio(currentRecord.audioSamples[currentRecord.currentFrame]);
        }

        currentRecord.currentFrame++;
    }

    private void PlayAudio(float[] audioSample)
    {
        if (audioSample.Length > 0)
        {
            AudioClip clip = AudioClip.Create("Playback", audioSample.Length, 1, 44100, false);
            clip.SetData(audioSample, 0);
            audioSource.clip = clip;
            audioSource.Play();
        }
    }
}

public class Frame
{
    public List<FrameData> data = new List<FrameData>();

    public void CapturePlayerState(player playerToCapture)
    {
        data.Clear();
        if (playerToCapture == null)
        {
            Debug.LogError("playerToCapture is null");
            return;
        }

        foreach (var item in playerToCapture.bodyPartsDictionary)
        {
            Transform partTransform = item.Value.transform;
            data.Add(new FrameData(item.Key, partTransform.localPosition, partTransform.localRotation));
        }
    }

    public void ApplyToPlayer(player playerToApply)
    {
        foreach (FrameData frameData in data)
        {
            Transform targetTransform = playerToApply.GetTransformByName(frameData.partName);
            if (targetTransform != null)
            {
                targetTransform.localPosition = frameData.position;
                targetTransform.localRotation = frameData.rotation;
            }
        }
    }
}

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

public class Record
{
    public List<Frame> frames = new List<Frame>();
    public List<float[]> audioSamples = new List<float[]>();
    public int currentFrame;

    public Record()
    {
        currentFrame = 0;
    }
}
