using UnityEngine;

public class recorderButtonInteraction : MonoBehaviour
{
    public Recorder recorder; // Reference to the Recorder script

    void Start()
    {
        // Find the Recorder in the scene
        recorder = FindObjectOfType<Recorder>();
        Debug.Log("started");
    }

    void OnCollisionEnter(Collision collision)
    {
        // Reset colors of all tagged objects
        ResetButtonColors();

        // Change the color of the current cube to green
        GetComponent<Renderer>().material.color = Color.green;
        Debug.Log("collided");

        // Call the corresponding method in the Recorder based on the cube's tag
        if (gameObject.tag == "PlayButton")
        {
            recorder.PlayRecording();
        }
        else if (gameObject.tag == "PauseButton")
        {
            recorder.PauseRecording();
        }
        else if (gameObject.tag == "RecordButton")
        {
            recorder.StartRecording();
        }
    }

    void ResetButtonColors()
    {
        // Find all objects with the tags and reset their colors to white
        GameObject[] playButtons = GameObject.FindGameObjectsWithTag("PlayButton");
        GameObject[] pauseButtons = GameObject.FindGameObjectsWithTag("PauseButton");
        GameObject[] recordButtons = GameObject.FindGameObjectsWithTag("RecordButton");

        foreach (GameObject button in playButtons)
        {
            if (button != this.gameObject) // Exclude the current object
            {
                button.GetComponent<Renderer>().material.color = Color.white;
            }
        }

        foreach (GameObject button in pauseButtons)
        {
            if (button != this.gameObject)
            {
                button.GetComponent<Renderer>().material.color = Color.white;
            }
        }

        foreach (GameObject button in recordButtons)
        {
            if (button != this.gameObject)
            {
                button.GetComponent<Renderer>().material.color = Color.white;
            }
        }
    }
}
