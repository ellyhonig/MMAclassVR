using UnityEngine;

public class HandProximityActionTrigger : MonoBehaviour
{
    public int interval = 1 ; // Default interval for adjusting keyframes
    public float distance =  0.1f;  // Default interval for adjusting keyframes

    private KeyPointSpawner keyPointSpawner; // Reference to the KeyPointSpawner script
    private Recorder recorder; // Reference to the Recorder
    public GameObject visualIndicator; // Assign in the inspector, a visual indicator for proximity

    void Start()
    {
        // Find the GameObject named "script" in the scene
        GameObject scriptObject = GameObject.Find("script");
        if (scriptObject != null)
        {
            keyPointSpawner = scriptObject.GetComponent<KeyPointSpawner>();
            recorder = scriptObject.GetComponent<Recorder>();
        }
        else
        {
            Debug.LogError("The object named 'script' was not found. Please ensure it exists and has KeyPointSpawner and Recorder components.");
        }
    }

    void Update()
    {
        // Only proceed if the Recorder and KeyPointSpawner references are set
        if (recorder != null && keyPointSpawner != null)
        {
            // Check proximity between the recorded player's hands
            if (CheckHandProximity(recorder.PlayerToRecord.Chest.shoulderR.hand.bp, recorder.PlayerToRecord.Chest.shoulderL.hand.bp) || CheckHandProximity(recorder.PlayerToRecord.Hip.shoulderR.hand.bp, recorder.PlayerToRecord.Hip.shoulderL.hand.bp))
            {
                // Hands are close enough; indicate activation
                if (visualIndicator != null)
                {
                    visualIndicator.GetComponent<Renderer>().material.color = Color.green;
                }
                // Example action based on proximity (Adjust based on actual use case)
                 switch (this.tag)
             {
            case "forward":
                FastForwardKeyframe();
                break;
            case "back":
            RewindKeyframe();
                break;
                } // This is just an example; adjust according to your game logic
            }
            else
            {
                // Hands are not close; indicate deactivation
                if (visualIndicator != null)
                {
                    visualIndicator.GetComponent<Renderer>().material.color = Color.white;
                }
            }
        }
    }

    private bool CheckHandProximity(GameObject handR, GameObject handL)
    {
        return Vector3.Distance(handR.transform.position, this.transform.position) <= distance || Vector3.Distance(handL.transform.position, this.transform.position) <= distance;
    }

   private void FastForwardKeyframe()
    {
        if (keyPointSpawner.publicIndex + interval < keyPointSpawner.recorder.currentRecord.frames.Count)
        {
            keyPointSpawner.currentUpdate = keyPointSpawner.moveKeyFrame;

            keyPointSpawner.publicIndex += interval;
           // Debug.Log($"Fast forwarded to keyframe index: {keyPointSpawner.publicIndex}");
        }
        else
        {
            Debug.LogWarning("Cannot fast forward, index out of bounds.");
        }
    }

    private void RewindKeyframe()
    {
        if (keyPointSpawner.publicIndex - interval >= 0)
        {
            keyPointSpawner.currentUpdate = keyPointSpawner.moveKeyFrame;

            keyPointSpawner.publicIndex -= interval;
            //Debug.Log($"Rewound to keyframe index: {keyPointSpawner.publicIndex}");
        }
        else
        {
            Debug.LogWarning("Cannot rewind, index out of bounds.");
        }
    }
}
