using System.Collections.Generic;
using UnityEngine;
using System.IO;
using HomeDojo;


[System.Serializable]
public class Level
{
    public Record record;
    public List<List<int>> keyframeIndicesSections;

    public Level()
    {
        keyframeIndicesSections = new List<List<int>>();
    }
}

public class LevelEditor
{
    private KeyPointSpawner spawner;
    private string levelFilePath;
    private int _currentSection;

     public LevelEditor(KeyPointSpawner spawner)
    {
        this.spawner = spawner;
        this.levelFilePath = Path.Combine(Application.persistentDataPath, "levels.json");
        this._currentSection = 0;
        //SaveLevel();
        //Debug.Log("made the file");
        //Debug.Log(Application.persistentDataPath);
    }

    public int currentSection
    {
        get => _currentSection;
        set
        {
            _currentSection = value;
            LoadCurrentSection();
        }
    }

    public void switchToNextSection()
    {
        if (currentSection + 1 < spawner.currentLevel.keyframeIndicesSections.Count)
        {
            currentSection++;
        }
        else
        {
            Debug.Log("Already at the last section.");
        }
    }

    private void LoadCurrentSection()
    {
        if (currentSection < 0 || currentSection >= spawner.currentLevel.keyframeIndicesSections.Count)
        {
            Debug.LogError("Current section is out of bounds.");
            return;
        }

        var currentSectionIndices = spawner.currentLevel.keyframeIndicesSections[currentSection];
        spawner.keyFrameList.Clear(); // Clear existing keyframes

        foreach (var frameIndex in currentSectionIndices)
        {
            spawner.publicIndex = frameIndex;
            spawner.addKeyFrame(); // Adds a new keyframe and sets it up
            spawner.currentlySelectedKeyFrame = spawner.keyFrameList.Count - 1; // Set the last added keyframe as currently selected
            spawner.keyFrameList[spawner.currentlySelectedKeyFrame].assignedFrame = frameIndex; // Assign frame
        }

        Debug.Log($"Loaded section {currentSection}.");
    }

    public void AddSection()
    {
        // Save the current section's keyframe indices
        List<int> currentSection = new List<int>();
        foreach (var keyFrame in spawner.keyFrameList)
        {
            currentSection.Add(keyFrame.assignedFrame);
        }
        spawner.currentLevel.keyframeIndicesSections.Add(currentSection);

        // Save the current level state to JSON
        SaveLevel();

        // Delete all player GameObjects spawned by the keyframes from the previous section
        foreach (var keyFrame in spawner.keyFrameList)
        {
            if (keyFrame.keyFramePlayer.bodyPartsParent != null)
            {
                GameObject.Destroy(keyFrame.keyFramePlayer.bodyPartsParent.gameObject);
            }
        }

        // Optionally clear keyFrameList here if starting fresh for the next section
        // Note: Clearing this might depend on whether you want to retain the keyframe visuals for reference or not
        // spawner.keyFrameList.Clear();

        Debug.Log("Section added and current players cleared.");
    }

    private void SaveLevel()
    {
    spawner.currentLevel.record = spawner.recorder.currentRecord;        
    string json = JsonUtility.ToJson(spawner.currentLevel, true);
    Debug.Log($"Saving level with {spawner.currentLevel.keyframeIndicesSections.Count} sections.");
    File.WriteAllText(this.levelFilePath, json);
    Debug.Log("Level saved.");
    }

    public void LoadLevel()
    {
        if (File.Exists(this.levelFilePath))
        {
            string json = File.ReadAllText(this.levelFilePath);
            Level loadedLevel = JsonUtility.FromJson<Level>(json);
            spawner.currentLevel = loadedLevel;

            // Clear existing keyframes
            spawner.keyFrameList.Clear();
            
            // Reconstruct keyframes from loaded data
            foreach (var section in loadedLevel.keyframeIndicesSections)
            {
                foreach (var frameIndex in section)
                {
                    spawner.publicIndex = frameIndex;
                    spawner.addKeyFrame(); // Adds a new keyframe and sets it up
                    spawner.currentlySelectedKeyFrame = spawner.keyFrameList.Count - 1;
                    spawner.keyFrameList[spawner.currentlySelectedKeyFrame].assignedFrame = frameIndex;
                }
                // Add logic here if you need to differentiate sections after loading
            }

            Debug.Log("Level loaded.");
        }
        else
        {
            Debug.LogError("Level file not found.");
        }
    }
    public void PlayLevel()
{
    // Step 1: Load the level from the saved JSON file
    LoadLevel();

    // Assuming that LoadLevel sets up the currentLevel object appropriately
    // and that you want to start testing from the first section
    currentSection = 0; // This will also trigger LoadCurrentSection due to the property's setter

    // Step 3: Call testTrace() from TraceChecker
    // Ensure TraceChecker is properly initialized and associated with a KeyPointSpawner instance
    spawner.traceChecker.startTestTrace();
}


}
