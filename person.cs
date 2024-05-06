using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class person : MonoBehaviour
{
    public GameObject hmd;
    public GameObject conR;
    public GameObject conL;
    public GameObject kneeConR;
    public GameObject kneeConL;
    public player player1;
    public MirroredPlayer mirroredPlayer;
    public float elbowRad;
    public float handRad;
    public float kneeRad;
    public float footRad;


   
    
    void Awake()
    {
        player1 = new player(hmd.transform, conR.transform, conL.transform, kneeConR.transform, kneeConL.transform);
        mirroredPlayer = new MirroredPlayer(hmd, conR, conL, kneeConR, kneeConL);
        
        
    }

    void Update()
    {
        player1.updater();
        //mirroredPlayer.UpdateMirroredPlayer();
        //mirroredPlayer.updateTransforms?.Invoke(hmd.transform, conR.transform, conL.transform, kneeConR.transform, kneeConL.transform);
       

    }
}

public class MirroredPlayer
{
    private GameObject dummyHmd;
    private GameObject dummyConR;
    private GameObject dummyConL;
    private GameObject dummyHipConR;
    private GameObject dummyHipConL;
    public player mirroredPlayer;
    public delegate void UpdateTransformsDelegate(Transform hmdTransform, Transform conRTransform, Transform conLTransform, Transform kneeConRTransform, Transform kneeConLTransform);
    public UpdateTransformsDelegate updateTransforms;
    
    public void SetUpdateTransformsMethod(UpdateTransformsDelegate newMethod)
    {
        updateTransforms = newMethod;
    }
    public MirroredPlayer(GameObject hmd, GameObject conR, GameObject conL, GameObject kneeConR, GameObject kneeConL)
    {
        // Initialize dummy transforms for mirrored player
        dummyHmd = new GameObject("DummyHmd");
        dummyConR = new GameObject("DummyConR");
        dummyConL = new GameObject("DummyConL");
        dummyHipConR = new GameObject("DummyHipConR");
        dummyHipConL = new GameObject("DummyHipConL");

        mirroredPlayer = new player(dummyHmd.transform, dummyConR.transform, dummyConL.transform, dummyHipConR.transform, dummyHipConL.transform);

        updateTransforms = UpdateDummyTransforms;
    }

    public void UpdateMirroredPlayer()
    {
        mirroredPlayer.updater();
    }

    public void UpdateDummyTransforms(Transform hmdTransform, Transform conRTransform, Transform conLTransform, Transform kneeConRTransform, Transform kneeConLTransform)
{
    var transformPairs = new List<(Transform dummy, Transform original)>
    {
        (dummyHmd.transform, hmdTransform),
        (dummyConR.transform, conRTransform),
        (dummyConL.transform, conLTransform),
        (dummyHipConR.transform, kneeConRTransform),
        (dummyHipConL.transform, kneeConLTransform)
    };

    foreach (var pair in transformPairs)
    {
        pair.dummy.localPosition = pair.original.localPosition;
        pair.dummy.localRotation = pair.original.localRotation;
    }
}
public void DisableMirroredPlayerParent()
{
    if (mirroredPlayer != null && mirroredPlayer.bodyPartsParent != null)
    {
        mirroredPlayer.bodyPartsParent.SetActive(false);
    }
    else
    {
        Debug.LogError("Mirrored player or its parent object is not properly initialized.");
    }
}
public void EnableMirroredPlayerParent()
{
    if (mirroredPlayer != null && mirroredPlayer.bodyPartsParent != null)
    {
        mirroredPlayer.bodyPartsParent.SetActive(true);
    }
    else
    {
        Debug.LogError("Mirrored player or its parent object is not properly initialized.");
    }
}


}
