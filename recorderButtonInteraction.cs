using UnityEngine;
using HomeDojo;

public class recorderButtonInteraction : MonoBehaviour
{
    private Recorder recorder; // Reference to the Recorder script
    private KeyPointSpawner keyPointSpawner; // Reference to the KeyPointSpawner script
    private LevelQueue levelQueue;
    void Start()
    {
        // Find the GameObject named "script" in the scene
        GameObject scriptObject = GameObject.Find("script");
        if (scriptObject != null)
        {
            // Get the Recorder and KeyPointSpawner from the "script" GameObject
            recorder = scriptObject.GetComponent<Recorder>();
            keyPointSpawner = scriptObject.GetComponent<KeyPointSpawner>();
            levelQueue = scriptObject.GetComponent<LevelQueue>();
        }
        else
        {
            Debug.LogError("The object named 'script' was not found. Please ensure it exists and has Recorder and KeyPointSpawner components.");
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        // Reset colors of all tagged objects
        ResetButtonColors();

        // Change the color of the current cube to green
        GetComponent<Renderer>().material.color = Color.green;

        // Call the corresponding method based on the cube's tag
        switch (gameObject.tag)
        {
            case "PlayButton":
                recorder.PlayRecording();
                break;
            case "PauseButton":
                recorder.PauseRecording();
                keyPointSpawner.Pause();
                break;
            case "RecordButton":
                recorder.StartRecording();
                break;
            case "spawn trace": // New case for handling "spawn trace" button interactions
                keyPointSpawner.addKeyFrame();
                recorder.PauseRecording();
                break;
            case "test trace":
                   keyPointSpawner.Pause();
                   keyPointSpawner.traceChecker.startTestTrace();
                   break;
            case "play level":
                    levelQueue.playLevel();  
                    break;  
            case "add tutorial section":
                    levelQueue.addRecord();  
                    break;      
            case "add test section":
                    levelQueue.addTest();  
                    break;      
        }
    }

    void ResetButtonColors()
{
    // Reset colors of all tagged objects

    GameObject[] spawnTraceButtons = GameObject.FindGameObjectsWithTag("spawn trace");
    ResetButtons(spawnTraceButtons);

    GameObject[] playButtons = GameObject.FindGameObjectsWithTag("PlayButton");
    GameObject[] pauseButtons = GameObject.FindGameObjectsWithTag("PauseButton");
    GameObject[] recordButtons = GameObject.FindGameObjectsWithTag("RecordButton");
    GameObject[] testTraceButtons = GameObject.FindGameObjectsWithTag("test trace"); // New line for test trace buttons
    GameObject[] playLevelButtons = GameObject.FindGameObjectsWithTag("play level"); // New line for play level buttons
    GameObject[] addTutorialButtons = GameObject.FindGameObjectsWithTag("add tutorial section"); // New line for add tutorial section buttons
    GameObject[] addTestButtons = GameObject.FindGameObjectsWithTag("add test section"); // New line for add test section buttons

    ResetButtons(playButtons);
    ResetButtons(pauseButtons);
    ResetButtons(recordButtons);
    ResetButtons(spawnTraceButtons); // Reset spawn trace buttons
    ResetButtons(testTraceButtons); // Reset test trace buttons
    ResetButtons(playLevelButtons); // Reset play level buttons
    ResetButtons(addTutorialButtons); // Reset add tutorial section buttons
    ResetButtons(addTestButtons); // Reset add test section buttons
}


    void ResetButtons(GameObject[] buttons)
    {
        foreach (GameObject button in buttons)
        {
            if (button != this.gameObject) // Exclude the current object
            {
                button.GetComponent<Renderer>().material.color = Color.white;
            }
        }
    }
}
