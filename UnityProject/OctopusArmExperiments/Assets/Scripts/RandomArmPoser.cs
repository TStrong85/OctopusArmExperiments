using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomArmPoser : MonoBehaviour
{
    public Transform[] jointChain;
    public ConfigurableJoint[] physicsJointChain;

    [Space]
    public bool useFixedSeed = false;
    public int seed;
    System.Random myRand;

    [Space]
    // Range to pick from to determine random rotation
    public Vector3 randomRotationMin;
    public Vector3 randomRotationMax;
    public float rotationMagnitudeScaling = 1;
    public AnimationCurve rotationMagnitudeScalingByNormalizedDistance = AnimationCurve.Constant(0, 1, 1);
    // Range to pick from to determine random 
    public float randomTimeMin;
    public float randomTimeMax;

    [Space]
    public bool limitRotationSpeed;
    public float rotationSpeedInDegrees = 180f;
    public bool remapT;
    public AnimationCurve tRemapping = AnimationCurve.Linear(0, 0, 1, 1);

    private ArmMovement[] armGoals;

    // Start is called before the first frame update
    void Start()
    {
        if (myRand == null)
        {
            if (useFixedSeed)
            {
                ReinitializeRandomNumberGenerator(seed);
            }
            else
            {
                ReinitializeRandomNumberGenerator();
            }
        }

        armGoals = new ArmMovement[Mathf.Max(jointChain.Length, physicsJointChain.Length)];
        for (int i = 0; i < armGoals.Length; i++)
        {
            armGoals[i] = new ArmMovement();

            armGoals[i].animationStartTime = -2;
            armGoals[i].animationEndTime = -1;

            if(i < jointChain.Length)
            {
                armGoals[i].targetJoint = jointChain[i];
                armGoals[i].startRotation = armGoals[i].targetJoint.localRotation.eulerAngles;
            } 
            else if (i < physicsJointChain.Length)
            {
                armGoals[i].startRotation = physicsJointChain[i].transform.localRotation.eulerAngles;
            }
        }
    }

    public void ReinitializeRandomNumberGenerator(int seed)
    {
        useFixedSeed = true;
        myRand = new System.Random(seed);
        Debug.Log("reiniti with " + seed);
    }

    public void ReinitializeRandomNumberGenerator()
    {
        useFixedSeed = false;
        ReinitializeRandomNumberGenerator(Random.Range(int.MinValue, int.MaxValue));
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < armGoals.Length; i++)
        {
            float normalizedDistance = i / (float)armGoals.Length;

            var armGoal = armGoals[i];
            if (armGoal.GetT() >= 1)
            {
                //Debug.Log("Updating Goal");
                // Calculate the next goal
                float newDuration = GetRandomInRange(myRand, randomTimeMin, randomTimeMax);
                Vector3 newRotation = new Vector3(GetRandomInRange(myRand, randomRotationMin.x, randomRotationMax.x),
                                                GetRandomInRange(myRand, randomRotationMin.y, randomRotationMax.y),
                                                GetRandomInRange(myRand, randomRotationMin.z, randomRotationMax.z));
                                                     
                System.Random rand = new System.Random();
                if (rand.Next(0, 2) == 0)
                {
                    newRotation *= -1;
                }


                newRotation *= rotationMagnitudeScaling;
                newRotation *= rotationMagnitudeScalingByNormalizedDistance.Evaluate(normalizedDistance);
                //Debug.Log("newDuration = " + newDuration + "\t\tnewRotation" + newRotation);

                //Debug.Log(newRotation);

                // Update the struct information
                armGoal.animationStartTime = Time.time;
                armGoal.animationEndTime = Time.time + newDuration;
                //armGoal.startRotation = armGoal.targetJoint.localRotation.eulerAngles;
                armGoal.endRotation = newRotation;

                // Make sure to not go too fast
                if (limitRotationSpeed)
                {
                    Vector3 sweepAngle = armGoal.endRotation - armGoal.startRotation;
                    sweepAngle = new Vector3(Mathf.Abs(sweepAngle.x), Mathf.Abs(sweepAngle.y), Mathf.Abs(sweepAngle.z));
                    float maxSweep = Mathf.Max(sweepAngle.x, sweepAngle.y, sweepAngle.z);
                    // Increase the animation time if needed to avoid sweeping too fast
                    armGoal.animationEndTime = Mathf.Max(armGoal.animationEndTime, Time.time + maxSweep / rotationSpeedInDegrees);
                }
                
                armGoals[i] = armGoal;
            } else
            {
                //Debug.Log("Moving Joint");
                // Update the rotation of the joint
                float t = armGoal.GetT();
                if (remapT)
                {
                    t = tRemapping.Evaluate(t);
                }

                Vector3 currentRotation = new Vector3(Mathf.LerpAngle(armGoal.startRotation.x, armGoal.endRotation.x, t),
                                                    Mathf.LerpAngle(armGoal.startRotation.y, armGoal.endRotation.y, t),
                                                    Mathf.LerpAngle(armGoal.startRotation.z, armGoal.endRotation.z, t));
                armGoal.startRotation = currentRotation;
                //Debug.Log(" " + currentRotation);

                if (armGoal.targetJoint != null)
                {
                    armGoal.targetJoint.localRotation = Quaternion.Euler(currentRotation);
                }
                armGoals[i] = armGoal;
            }


            // Also apply to physics joint target rotation
            if (i < physicsJointChain.Length)
            {
                var joint = physicsJointChain[i];
                joint.targetRotation = Quaternion.Euler(-armGoal.endRotation);
            }
            
        }
    }

    private float GetRandomInRange(System.Random rand, float min, float max)
    {
        float interpolant = (float)rand.Next(int.MinValue, int.MaxValue) / (float)int.MaxValue;
        return Mathf.Lerp(min, max, interpolant);
    }

    [System.Serializable]
    public struct ArmMovement
    {
        public Transform targetJoint;
        public float animationStartTime;
        public float animationEndTime;
        public Vector3 startRotation;
        public Vector3 endRotation;

        public float GetT()
        {
            float duration = animationEndTime - animationStartTime;
            return (Time.time - animationStartTime) / duration;
        }

        
    }
}
