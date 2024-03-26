using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

// Define a base class for body parts
public class bodypart 
{
    // Transform components for head-mounted display and controllers
    public Transform hmd;
    public Transform conR;
    public Transform conL;
    public GameObject bp; // GameObject representing the body part

    // Constructor for the bodypart class
    public bodypart(string Name,Transform Hmd,Transform ConR,Transform ConL)
    {
        conR = ConR;
        conL = ConL;
        hmd = Hmd;
        bp = GameObject.CreatePrimitive(PrimitiveType.Sphere); // Create a sphere to represent the body part
        bp.transform.localScale = new Vector3(.1f, .1f, .1f); // Set the scale of the sphere
        bp.name = Name; // Set the name of the GameObject
    }

    // Default constructor
    public bodypart(){}

    // Virtual method for updating the body part, to be overridden in derived classes
    public virtual void updater()
    {
    }
    
    // Calculate the closest point on the surface of a sphere
    public Vector3 closestPointOnSphere(Vector3 center, Vector3 target, float radius)
    {
        Vector3 direction = (target - center).normalized;
        return center + direction * radius;
    }

    // Set the position and orientation of a GameObject (cylinder) between two joints
    public void SetBetweenJoints(Vector3 joint1,Vector3 joint2, GameObject cyl)
    {
        cyl.transform.position = (joint1+joint2)/2;
        Vector3 direction = (joint1 - joint2).normalized;
        cyl.transform.up = direction;
    }
    public  static Func<Vector3, Vector3, Vector3, Vector3> avgof3vec = (leg_conL, leg_conR, hmd) =>
    {
        return (leg_conL + leg_conR + hmd) / 3f;
    };

    public static Func<Vector3, Vector3, int, Vector3> biasToTarget = (currentAvg, target, counter) =>
    {
        var newPosition = (counter == 0) ? currentAvg : biasToTarget((currentAvg + target) / 2f, target, counter - 1);
        return newPosition; 
    };
}

// Derived class for the chest
public class chest : bodypart
{
    public shoulder shoulderR;
    public shoulder shoulderL;
    public int biasCounter = 3;
    

    // Constructor for chest
    public chest(string name, Transform hmd, Transform conR, Transform conL) 
        : base(name, hmd, conR, conL) // Call the base class constructor
    {
       shoulderR = new shoulder("shoulR",this); 
       shoulderL = new shoulder("shoulL",this);
    }

    // Override the updater method for the chest
    public override void updater()
    {
        this.bp.transform.localPosition = biasToTarget(avgof3vec(this.conL.localPosition,this.conR.localPosition,this.hmd.localPosition),hmd.localPosition, biasCounter);
        shoulderL.updater();
        shoulderR.updater();
    }
}
public class shoulder : bodypart
{
    Transform con;
    Transform Chest;
    public elbow elbow; 
    public hand hand;
    public float radius = .15f;

    // Constructor for shoulder
    public shoulder(string name, chest _Chest)
    {
        bp = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        bp.transform.localScale = new Vector3(.1f, .1f, .1f);
        bp.name = name;
        Chest = _Chest.bp.transform;
        // Initialize the corresponding elbow and hand based on the shoulder side
        if (name == "shoulR")
        {
            con = _Chest.conR.transform;
            elbow = new elbow("elbowR", this.bp.transform, con);
            hand = new hand("handR", elbow.bp.transform);
        }
        else
        {
            con = _Chest.conL.transform;
            elbow = new elbow("elbowL", this.bp.transform, con);
            hand = new hand("handL", elbow.bp.transform);
        }
    }

    // Override the updater method for shoulder
    public override void updater()
    {
        this.bp.transform.localPosition = closestPointOnSphere(center: this.Chest.localPosition, target: this.con.localPosition, radius: radius);
        elbow.updater();
        hand.updater();
    }
}

public class elbow : bodypart
{
    public Transform shoul;
    public Transform con;
    public Transform hand;
    public float elbowOffset = .15f;
    public float radius = .2f;
    public GameObject tricep;

    // Constructor for elbow
    public elbow(string name, Transform Shoul, Transform Con)
    {
        bp = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        bp.transform.localScale = new Vector3(.1f, .1f, .1f);
        bp.name = name;
        tricep = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        tricep.transform.localScale = new Vector3(.13f, .35f*radius, .13f);
        shoul = Shoul;
        con = Con;
    }

    // Override the updater method for elbow
    public override void updater()
    {
        if (hand == null)
            this.bp.transform.localPosition = closestPointOnSphere(center: this.shoul.localPosition, target: this.con.localPosition, radius: radius);
        else
           // this.bp.transform.localPosition = (closestPointOnSphere(center: this.shoul.localPosition, target: this.con.localPosition, radius: radius) - hand.localPosition).normalized * elbowOffset - closestPointOnSphere(center: this.shoul.localPosition, target: this.con.localPosition, radius: radius);
        this.bp.transform.localPosition = closestPointOnSphere(center: this.shoul.localPosition, target: this.con.localPosition, radius: radius);
        this.bp.transform.localRotation = con.transform.localRotation;
        SetBetweenJoints(joint1: shoul.position, joint2: this.bp.transform.position, cyl: tricep);
    }
}

public class hand : bodypart
{
    Transform elbow;
    public GameObject forearm;
    public float radius = .3f;
    public delegate void UpdateDelegate();
    private UpdateDelegate currentUpdate;

    // Constructor for hand
    public hand(string name, Transform Elbow)
    {
        bp = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        bp.transform.localScale = new Vector3(.1f, .1f, .1f);
        bp.name = name;
        forearm = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        forearm.transform.localScale = new Vector3(.1f, .09f, .1f);
        elbow = Elbow;
        currentUpdate = PreCalibrationUpdate;
    }

    // Method to calibrate the hand position
    public void Calibrate()
    {
        this.bp.transform.localPosition = elbow.localPosition + Vector3.down * radius;
        this.bp.transform.SetParent(elbow);
        currentUpdate = PostCalibrationUpdate;
    }

    // Update methods for before and after calibration
    private void PreCalibrationUpdate()
    {
        this.bp.transform.localPosition = elbow.localPosition + Vector3.down * radius;
        SetBetweenJoints(joint1: elbow.position, joint2: this.bp.transform.position, cyl: forearm);
    }

    private void PostCalibrationUpdate()
    {
        SetBetweenJoints(joint1: elbow.position, joint2: this.bp.transform.position, cyl: forearm);
    }

    // Override the updater method for hand
    public override void updater()
    {
        currentUpdate();
    }
}

public class Torso : bodypart
{
    private Transform chestTransform;
    private Transform hipsTransform;
    private Transform hmd;
    public GameObject upperTorso;
    public GameObject lowerTorso;

    // Constructor for the Torso class
    public Torso(string name, Transform hmd, Transform chest, Transform hips, GameObject parentObject)
    {
        // Directly set the transforms without calling the base constructor
        this.hmd = hmd;
        this.chestTransform = chest;
        this.hipsTransform = hips;

        // Create cylinders to represent the upper and lower torso
        upperTorso = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        upperTorso.name = name + "_UpperTorso";
        lowerTorso = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        lowerTorso.name = name + "_LowerTorso";
        resize();
        // Make the cylinders children of the parent object
        upperTorso.transform.SetParent(parentObject.transform, false);
        lowerTorso.transform.SetParent(parentObject.transform, false);
    }
    public void resize()
    {
        upperTorso.transform.localScale = new Vector3(.15f, (hmd.position-chestTransform.position).magnitude, .15f);
        lowerTorso.transform.localScale = new Vector3(.15f, .35f*(chestTransform.position-hipsTransform.position).magnitude, .15f);
    }
    // Override the updater method for the Torso
    public override void updater()
    {
        SetBetweenJoints(hipsTransform.position, chestTransform.position, lowerTorso);
        SetBetweenJoints((hipsTransform.position + chestTransform.position)/2f, chestTransform.position, upperTorso);
    }
}





public class player
{
    public Transform hmd;
    public Transform conR;
    public Transform conL;
    public Transform kneeConR; // New controller for right hip
    public Transform kneeConL; // New controller for left hip
    public chest Chest;
    public chest Hip;
    public Torso playerTorso;
    public Dictionary<string, GameObject> bodyPartsDictionary;

    public delegate void UpdateDelegate();
    public UpdateDelegate currentUpdate;
    public GameObject bodyPartsParent;
    
    public player(Transform Hmd, Transform ConR, Transform ConL, Transform KneeConR, Transform KneeConL)
    {
        hmd = Hmd;
        conR = ConR;
        conL = ConL;
        kneeConR = KneeConR;
        kneeConL = KneeConL;
        Chest = new chest("chest", hmd, conR, conL);
        Hip = new chest("hip", hmd, kneeConR, kneeConL);
        Hip.biasCounter = 1;
        Hip.shoulderR.elbow.radius = .35f;
        Hip.shoulderL.elbow.radius = .35f;
        Hip.shoulderR.radius = .1f; 
        Hip.shoulderL.radius = .1f;
        Hip.shoulderR.hand.radius = .35f; 
        Hip.shoulderL.hand.radius = .35f;
        currentUpdate = PreCalibrationUpdate; 
        bodyPartsParent = new GameObject("BodyPartsParent");
        playerTorso = new Torso("playerTorso", Hmd, Chest.bp.transform, Hip.bp.transform, bodyPartsParent);
        bodyPartsDictionary = new Dictionary<string, GameObject>();
        PopulateBodyPartsDictionary();
        InitializeBodyPartsParent();

    }
    private void InitializeBodyPartsParent()
    {
        

        // Set parent for HMD and controllers directly
        hmd.SetParent(bodyPartsParent.transform, false);
        conR.SetParent(bodyPartsParent.transform, false);
        conL.SetParent(bodyPartsParent.transform, false);
        kneeConR.SetParent(bodyPartsParent.transform, false);
        kneeConL.SetParent(bodyPartsParent.transform, false);

        // Now set the parent for all body parts stored in the dictionary
        foreach (var bodyPart in bodyPartsDictionary.Values)
        {
            bodyPart.transform.SetParent(bodyPartsParent.transform, false);
        }
    }

    private void PopulateBodyPartsDictionary()
    {
        // Main body parts
        bodyPartsDictionary.Add("HMD", hmd.gameObject);
        bodyPartsDictionary.Add("Chest", Chest.bp);
        bodyPartsDictionary.Add("Hip", Hip.bp);
    bodyPartsDictionary.Add("UpperTorso", playerTorso.upperTorso); // Added upper torso
    bodyPartsDictionary.Add("LowerTorso", playerTorso.lowerTorso); 
        // Chest body parts
        AddBodyPartToDictionary(Chest, "Chest");

        // Hip body parts
        AddBodyPartToDictionary(Hip, "Hip");
    }

    // Helper method to add body parts to the dictionary with a prefix, now mapping to GameObjects
    private void AddBodyPartToDictionary(chest chestPart, string prefix)
    {
        bodyPartsDictionary.Add(prefix + "ShoulderR", chestPart.shoulderR.bp);
        bodyPartsDictionary.Add(prefix + "ShoulderL", chestPart.shoulderL.bp);
        bodyPartsDictionary.Add(prefix + "ElbowR", chestPart.shoulderR.elbow.bp);
        bodyPartsDictionary.Add(prefix + "ElbowL", chestPart.shoulderL.elbow.bp);
        bodyPartsDictionary.Add(prefix + "HandR", chestPart.shoulderR.hand.bp);
        bodyPartsDictionary.Add(prefix + "HandL", chestPart.shoulderL.hand.bp);
        bodyPartsDictionary.Add(prefix + "TricepR", chestPart.shoulderR.elbow.tricep);
        bodyPartsDictionary.Add(prefix + "TricepL", chestPart.shoulderL.elbow.tricep);
        bodyPartsDictionary.Add(prefix + "ForearmR", chestPart.shoulderR.hand.forearm);
        bodyPartsDictionary.Add(prefix + "ForearmL", chestPart.shoulderL.hand.forearm);
    }

    public Transform GetTransformByName(string name)
    {
        // Attempt to get the GameObject from the dictionary using the provided name
        if (bodyPartsDictionary.TryGetValue(name, out GameObject bodyPart))
        {
            // If found, return the transform of the GameObject
            return bodyPart.transform;
        }

        // If the body part name was not found in the dictionary, throw an exception
        throw new ArgumentException($"Transform name not recognized: {name}");
    }




    // Method to calibrate the player's body parts
    public void Calibrate()
    {
    // Calibration for upper body
        Chest.shoulderR.hand.Calibrate();
        Chest.shoulderL.hand.Calibrate();
        Chest.shoulderR.elbow.hand = Chest.shoulderR.hand.bp.transform;
        Chest.shoulderL.elbow.hand = Chest.shoulderL.hand.bp.transform;

        // Calibration for lower body
        Hip.shoulderR.hand.Calibrate();
        Hip.shoulderL.hand.Calibrate();
        Hip.shoulderR.elbow.hand = Chest.shoulderR.hand.bp.transform;
        Hip.shoulderL.elbow.hand = Chest.shoulderL.hand.bp.transform;
        currentUpdate = PostCalibrationUpdate;
    }


    // Update methods for before and after calibration
    private void PreCalibrationUpdate()
    {
        Hip.shoulderR.hand.radius = Hip.shoulderR.elbow.bp.transform.position.y;
        Hip.shoulderL.hand.radius = Hip.shoulderL.elbow.bp.transform.position.y;
        playerTorso.resize();
        if (isLookingUp()) Calibrate();
    }

    private void PostCalibrationUpdate()
    {
        // Placeholder for post-calibration updates
    }

    // Update the player's body parts
    public void updater()
    {
        Chest.updater();
        Hip.updater();
        playerTorso.updater();
        currentUpdate();
    }
    public void legCorrecter()
    {
        
    }
    // Check if the player is looking up
    public bool isLookingUp()
    {
        return Vector3.Dot(hmd.forward, Vector3.up) > .8f;
    }
}
