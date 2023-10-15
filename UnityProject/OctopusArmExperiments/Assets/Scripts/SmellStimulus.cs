using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmellStimulus : MonoBehaviour
{
    public float smellStrength = 1;
    public float smellFalloffPower = 2; //inverse square by default
    [Header("Optional")]
    public Rigidbody smellRigidBody;

    protected virtual void OnTriggerStay(Collider other)
    {
        ClimbSucker climbSuckerComp = other.GetComponent<ClimbSucker>();
        if (climbSuckerComp)
        {
            climbSuckerComp.AddTorqueTowardsSmell(transform.position, smellStrength, smellFalloffPower);

            if (smellRigidBody != null)
            {
                //climbSuckerComp.AddForceToFollowTarget(smellRigidBody);
            }
        } 
    }
}
