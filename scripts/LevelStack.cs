using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HomeDojo;

public class LevelQueue : MonoBehaviour
{
    private KeyPointSpawner keyPointSpawner;
    public List<Record> records = new List<Record>();
    public List<List<int>> keyFrameLists = new List<List<int>>();
    public Dictionary<Record, List<int>> recordToKeyFrameLists = new Dictionary<Record, List<int>>();
    public int currentRecordIndex = 0;
    public delegate void UpdateDelegate();
    public UpdateDelegate currentUpdate;

    // Start is called before the first frame update
    void Start()
    {
        keyPointSpawner = this.GetComponent<KeyPointSpawner>();  
        currentUpdate = null; // Initialize currentUpdate to null
    }

    // Update is called once per frame
    void Update()
    {
        currentUpdate?.Invoke(); // Call the current update delegate if it's not null
    }

    public void addRecord()
    {
        Record newRecord = keyPointSpawner.recorder.currentRecord;
        records.Add(newRecord);
        recordToKeyFrameLists.Add(newRecord, null);
    }

    public void addTest()
    {
        Record currentRecord = keyPointSpawner.recorder.currentRecord;
        records.Add(currentRecord);

        List<int> currentKeyFrames = new List<int>();
        foreach (var keyFrame in keyPointSpawner.keyFrameList)
        {
            currentKeyFrames.Add(keyFrame.assignedFrame);
        }
        keyFrameLists.Add(currentKeyFrames);
        recordToKeyFrameLists[currentRecord] = currentKeyFrames;
        resetKeyPointSpawner();
    }

    public void resetKeyPointSpawner()
    {
        keyPointSpawner.traceChecker.DeactivateAllKeyframePlayers();
        keyPointSpawner.keyFrameList.Clear();
        keyPointSpawner.publicIndex = 0;
    }

    public void populateKeyFrameList(int recordIndex)
    {
        resetKeyPointSpawner();

        if (recordIndex < 0 || recordIndex >= records.Count)
        {
            Debug.LogError("Record index is out of bounds.");
            return;
        }

        Record selectedRecord = records[recordIndex];
        if (!recordToKeyFrameLists.ContainsKey(selectedRecord))
        {
            Debug.LogError("Selected record does not have an associated keyframe list.");
            return;
        }

        List<int> keyFrameIndices = recordToKeyFrameLists[selectedRecord];
        foreach (int frameIndex in keyFrameIndices)
        {
            keyPointSpawner.addKeyFrame();
            keyPointSpawner.keyFrameList[keyPointSpawner.keyFrameList.Count - 1].assignedFrame = frameIndex;
        }
    }

    public void playSection(int recordIndex)
    {
        keyPointSpawner.traceChecker.DeactivateAllKeyframePlayers();
        resetKeyPointSpawner();
        
        keyPointSpawner.recorder.currentRecord = records[recordIndex];

        if (recordToKeyFrameLists[records[recordIndex]] == null)
        {
            keyPointSpawner.recorder.PlayRecording();
        }
        else
        {
            populateKeyFrameList(recordIndex);
            keyPointSpawner.traceChecker.startTestTrace();
        }
    }

    public void playLevel()
    {
        playSection(currentRecordIndex);
        currentUpdate = sectionInProgress;
    }

    void sectionInProgress()
    {
        // Get the current Record from records list using currentRecordIndex
        Record currentRecord = records[currentRecordIndex];

        // Check if the current section has no associated keyframe lists
        if (recordToKeyFrameLists[records[currentRecordIndex]] == null)
        {
            // Check if the end of the current record's frames has been reached
            if (currentRecord.currentFrame >= currentRecord.frames.Count)
            {
                // Move to the next record index if there are more records available
                if (currentRecordIndex + 1 < records.Count)
                {
                    currentRecordIndex++;
                    playSection(currentRecordIndex); // Play the next section
                }
                else
                {
                    // Optionally, handle the scenario where no more records are available
                    Debug.Log("Finished all records in the level.");
                    currentUpdate = null; // Stop the update cycle
                }
            }
        }
        else
        {
            
            // There is an associated keyframe list, check if all keyframes have been processed
            if (keyPointSpawner.traceChecker.currentKeyFrame >= keyPointSpawner.keyFrameList.Count)
            {
                // Move to the next record index if there are more records available
                if (currentRecordIndex + 1 < records.Count)
                {
                    currentRecordIndex++;
                    playSection(currentRecordIndex); // Play the next section
                }
                else
                {
                    // Optionally, handle the scenario where no more records are available
                    Debug.Log("Finished all sections of the level.");
                    currentUpdate = null; // Stop the update cycle
                }
            }
        }
    }

}
