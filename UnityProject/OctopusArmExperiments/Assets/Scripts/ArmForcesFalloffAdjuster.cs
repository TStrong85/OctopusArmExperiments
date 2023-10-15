using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Script for initializing various values related to arm movement and reactivity based on the
//   position of a given segment in the arm (the base is normalized as 0, and the tip as 1).
//   Initially created for experimenting with arm-box search tasks and tunings.
public class ArmForcesFalloffAdjuster : MonoBehaviour
{
    public bool applyOnStart = true;
    [Header("References")]
    public ConfigurableJoint[] armJoints;
    public ClimbSucker[] armSuckers;
    public ArtificialGravity[] gravityComponents;

    [Header("Curves")]
    [Tooltip("How strong is random motion given the segment's normalized distance from the base")]
    public AnimationCurve poseMatchingForceByNormalizedDistance = AnimationCurve.Constant(0, 1, 1);
    public float poseMatchingForceMultiplier = 1;
    [Tooltip("How strong is the contact response given the segment's normalized distance from the base")]
    public AnimationCurve contactResponseTorqueByNormalizedDistance = AnimationCurve.Constant(0, 1, 1);
    public float contactResponseTorqueMultiplier = 1;
    [Tooltip("How strong is the smell response given the segment's normalized distance from the base")]
    public AnimationCurve smellResponseSensitivityByNormalizedDistance = AnimationCurve.Constant(0, 1, 1);
    public float smellResponseSensitivityMultiplier = 1;
    [Tooltip("How far to recruit given the segment's normalized distance from the base")]
    public AnimationCurve recruitmentWidthByNormalizedDistance = AnimationCurve.Constant(0, 1, 1);
    public float recruitmentWidthMultiplier = 1;
    [Tooltip("How much to decay the recruitment response given the segment's normalized distance from the base")]
    public AnimationCurve recruitmentfalloffMultByNormalizedDistance = AnimationCurve.Constant(0, 1, 1);
    public float recruitmentfalloffMultMultiplier = 1;

    [Header("Constants")]
    public Vector3 newGravityVector;
    public float newDrag = 2;
    public float newAngularDrag = 0.05f;

    
    private float baseAngularDriveXPositionSpringValue;
    private float baseAngularDriveYZPositionSpringValue;
    private float baseContactResponseTorqueValue;
    private float baseSmellResponseSensitivityValue;
    private float baseRecruitmentWidthValue;
    private float baseRecruitmentfalloffMultValue;
    //private Vector3 baseGravityValue;


    [Header("Debug")]
    public bool drawGui = false;

    // Start is called before the first frame update
    void Start()
    {
        // Collect and record initial values for variables we want to adjust
        baseAngularDriveXPositionSpringValue = armJoints[0].angularXDrive.positionSpring;
        baseAngularDriveYZPositionSpringValue = armJoints[0].angularYZDrive.positionSpring;

        baseContactResponseTorqueValue = armSuckers[0].contactResponseTorqueAmount;
        baseSmellResponseSensitivityValue = armSuckers[0].smellSensitivity;
        baseRecruitmentWidthValue = armSuckers[0].recruitmentWidth;
        baseRecruitmentfalloffMultValue = armSuckers[0].recruitmentFalloffMult;

        //baseGravityValue = gravityComponents[0].gravityVector;


        if (applyOnStart)
        {
            // Apply Adjustments
            ApplyForceAdjustments();
        }
    }

    public void ApplyForceAdjustments()
    {
        // Apply random motion adjustments to joints
        for (int i = 0; i < armJoints.Length; i++)
        {
            float normalizedDistance = i / (float)armJoints.Length;

            // For X axis
            JointDrive jd = armJoints[i].angularXDrive;
            jd.positionSpring = poseMatchingForceByNormalizedDistance.Evaluate(normalizedDistance) 
                                    * baseAngularDriveXPositionSpringValue 
                                    * poseMatchingForceMultiplier;
            armJoints[i].angularXDrive = jd;

            // For YZ axis
            jd = armJoints[i].angularYZDrive;
            jd.positionSpring = poseMatchingForceByNormalizedDistance.Evaluate(normalizedDistance)
                                    * baseAngularDriveYZPositionSpringValue
                                    * poseMatchingForceMultiplier;
            armJoints[i].angularYZDrive = jd;
        }

        // Apply stimulus response and recruitment to segments
        for (int i = 0; i < armSuckers.Length; i++)
        {
            float normalizedDistance = i / (float)armSuckers.Length;

            armSuckers[i].contactResponseTorqueAmount = contactResponseTorqueByNormalizedDistance.Evaluate(normalizedDistance)
                                    * baseContactResponseTorqueValue
                                    * contactResponseTorqueMultiplier;

            armSuckers[i].smellSensitivity = smellResponseSensitivityByNormalizedDistance.Evaluate(normalizedDistance)
                                    * baseSmellResponseSensitivityValue
                                    * smellResponseSensitivityMultiplier;

            armSuckers[i].recruitmentWidth = (int)(recruitmentWidthByNormalizedDistance.Evaluate(normalizedDistance)
                                    * baseRecruitmentWidthValue
                                    * recruitmentWidthMultiplier);

            armSuckers[i].recruitmentFalloffMult = (int)(recruitmentfalloffMultByNormalizedDistance.Evaluate(normalizedDistance)
                                    * baseRecruitmentfalloffMultValue
                                    * recruitmentfalloffMultMultiplier);
        }

        // Apply artificial gravity and constant values to segments
        for (int i = 0; i < gravityComponents.Length; i++)
        {
            float normalizedDistance = i / (float)gravityComponents.Length;

            gravityComponents[i].gravityVector = newGravityVector;
            gravityComponents[i].getRigidbody().drag = newDrag;
            gravityComponents[i].getRigidbody().angularDrag = newAngularDrag;
        }
    }

    private void OnGUI()
    {
        if (!drawGui) { return; }

        GUILayout.BeginArea(new Rect(Screen.width * 0.7f, 0, Screen.width * 0.3f, Screen.height));
        if (GUILayout.Button("Update Adjustments"))
        {
            ApplyForceAdjustments();
        }
        GUILayout.EndArea();
    }
}
