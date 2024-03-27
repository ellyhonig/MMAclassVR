using UnityEngine;

public class recorderButtonInteraction : MonoBehaviour
{
    private Recorder recorder; // Reference to the Recorder script
    private KeyPointSpawner keyPointSpawner; // Reference to the KeyPointSpawner script

    void Start()
    {
        // Find the GameObject named "script" in the scene
        GameObject scriptObject = GameObject.Find("script");
        if (scriptObject != null)
        {
            // Get the Recorder and KeyPointSpawner from the "script" GameObject
            recorder = scriptObject.GetComponent<Recorder>();
            keyPointSpawner = scriptObject.GetComponent<KeyPointSpawner>();
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
        }
    }

    void ResetButtonColors()
    {
        // Extend this method to include resetting the "spawn trace" buttons if needed
        GameObject[] spawnTraceButtons = GameObject.FindGameObjectsWithTag("spawn trace");
        ResetButtons(spawnTraceButtons);

        GameObject[] playButtons = GameObject.FindGameObjectsWithTag("PlayButton");
        GameObject[] pauseButtons = GameObject.FindGameObjectsWithTag("PauseButton");
        GameObject[] recordButtons = GameObject.FindGameObjectsWithTag("RecordButton");

        ResetButtons(playButtons);
        ResetButtons(pauseButtons);
        ResetButtons(recordButtons);
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
