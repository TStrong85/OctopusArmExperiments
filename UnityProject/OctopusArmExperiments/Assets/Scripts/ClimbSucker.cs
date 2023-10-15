using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// A class that defines the properties of a segment in the octopus arm
//   Controls various responses (contact, smell, passive animation)
public class ClimbSucker : MonoBehaviour
{
    public Rigidbody rb;

    public Vector3 avgContactPos;
    public Vector3 avgContactNorm;

    [Header("Recruitment")]
    // References effectively create a doubly linked list of arm segments
    public ClimbSucker prevSucker;
    public ClimbSucker nextSucker;
    public ClimbSucker currSucker;
    [Tooltip("How many neighbors to recruit in either direction?")] 
    public int recruitmentWidth = 2;
    [Tooltip("What multiplicative falloff to use when propogating response to recruited neighbor?")]
    public float recruitmentFalloffMult = 1f;

    [Space]
    public float contactResponseTorqueAmount = 1000;

    // An experimental system for dynamically reducing the responsiveness to smells after extended exposure to them
    [Header("Smell")]
    public float smellSensitivity = 1;
    //public float smellStrengthBufferDecay = 2f;
    // float smellStrengthBufferAccumilation = 2f;
    //private float smellStrengthBuffer = 0;
    //public float powerforSmellInfluenceOnTouchResponse = 0.5f;

    [Header("Crawling")]
    public float crawlInput = 0;
    public float crawlTimeOffset = 0;
    public float crawlTorque = 1000;
    public float crawlArmAngleRatio = 1;
    public float crawlContactOffsetShifting = 0f;
    public GameObject torqueAxisRef;

    public float sideFrontBlend = 0;
    public float crawlOffset = 0;

    public Vector3 openingVerticalPosition;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();

        ClimbSucker[] siblingSuckers = transform.parent.GetComponentsInChildren<ClimbSucker>();
        for (int i = 0; i < siblingSuckers.Length; i++)
        {
            ClimbSucker currSucker = siblingSuckers[i];

            if (currSucker == this)
            {
                if (i > 0) { prevSucker = siblingSuckers[i - 1]; }
                if (i < siblingSuckers.Length - 1) { nextSucker = siblingSuckers[i + 1]; }
            }
        }

        crawlOffset = Random.Range(0, 4);
    }

    // Update is called once per frame
    void Update()
    {
        CrawlRotation();
    }

    private void FixedUpdate()
    {
        // Maintain SmellStengthBuffer
        //smellStrengthBuffer -= smellStrengthBuffer * Time.fixedDeltaTime * smellStrengthBufferDecay;
    }

    private void OnCollisionEnter(Collision collision)
    {
        OnCollisionStay(collision);
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.isStatic)
        {
            avgContactNorm = Vector3.zero;
            avgContactPos = Vector3.zero;
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.isStatic)
        {
            crawlTimeOffset += crawlContactOffsetShifting * Time.fixedDeltaTime;


            // Record Contacts
            avgContactNorm = Vector3.zero;
            avgContactPos = Vector3.zero;
            foreach (var contact in collision.contacts)
            {
                avgContactPos += contact.point;
                avgContactNorm += contact.normal;
            }
            avgContactPos /= collision.contactCount;
            avgContactNorm /= collision.contactCount;
            avgContactNorm.Normalize();

            // Respond with torque
            Vector3 contactResposnseTorqueAxis = Vector3.Cross(avgContactNorm.normalized, transform.forward);
            
            contactResposnseTorqueAxis *= contactResponseTorqueAmount;
            

            // Potentially increase according to smellStrength
            //float smellMult = Mathf.Max(1, Mathf.Pow(smellStrengthBuffer, powerforSmellInfluenceOnTouchResponse));
                

            rb.AddTorque(contactResposnseTorqueAxis, ForceMode.Acceleration);

            // Recruit nearby joints to apply the response torque to them, applying falloff multiplicatively if set in inspector
            ClimbSucker currNext = nextSucker;
            ClimbSucker currPrev = prevSucker;
            Vector3 currTorque = contactResposnseTorqueAxis;
            for (int i = 0; i < recruitmentWidth; i++)
            {
                currTorque *= recruitmentFalloffMult;
                if (currNext != null) 
                {
                    currNext.rb.AddTorque(currTorque, ForceMode.Acceleration);
                    currNext = currNext.nextSucker;
                }
                if (currPrev != null)
                {
                    currPrev.rb.AddTorque(currTorque, ForceMode.Acceleration);
                    currPrev = currPrev.prevSucker;
                }
                // Hit the ends of the chain
                if (currNext == null && currPrev == null) {
                    break; 
                }
            }
        }
    }

    public void AddTorqueTowardsSmell(Vector3 smellSource, float smellStrength, float smellFalloff)
    {
        // Determine base response strength
        float torqueAmount = 1000;
        torqueAmount *= smellStrength;

        // Apply falloff (inverse square by default)
        Vector3 smellDir = smellSource - transform.position;
        float smellDistance = Vector3.Distance(transform.position, smellSource) + Mathf.Epsilon;
        if (smellFalloff > 0)
        {
            torqueAmount /= Mathf.Pow(smellDistance, smellFalloff);
        }

        // Update smellStrengthBuffer
        //float localSmellStrength = smellStrength / Mathf.Pow(smellDistance, smellFalloff);
        //smellStrengthBuffer += Mathf.Max(0, localSmellStrength - smellStrengthBuffer) * Time.fixedDeltaTime * smellStrengthBufferAccumilation;

        
        // Reduce torque when near target direction, full torque between opposite to orthogonal direction.
        float misalignment = -Vector3.Dot(smellDir.normalized, transform.forward);
        misalignment = Mathf.Clamp01(misalignment+1);
        torqueAmount *= misalignment;

        Vector3 torqueAxis = Vector3.Cross(transform.forward, smellDir.normalized);
        rb.AddTorque(torqueAxis.normalized * torqueAmount, ForceMode.Acceleration);
        Debug.DrawLine(transform.position, transform.position + torqueAxis.normalized * 2, Color.blue);
    }

    public void CrawlRotation()
    {
        float time = Time.time + crawlTimeOffset;
        time += crawlOffset;
        time *= 1.5f;    

        // Sideways crawl cycle
        Vector3 sideCycleTorqueDirection = new Vector3(Mathf.Cos(time), Mathf.Sin(time), 0);
        float sideCycleAlignment = 1 - sideFrontBlend;
        sideCycleTorqueDirection = Vector3.Lerp(Vector3.forward, sideCycleTorqueDirection, crawlArmAngleRatio);
        if (torqueAxisRef != null)
        {
            sideCycleTorqueDirection = torqueAxisRef.transform.rotation * sideCycleTorqueDirection;
            sideCycleAlignment = Vector3.Dot(
                    Vector3.ProjectOnPlane(torqueAxisRef.transform.right, Vector3.up),
                    Vector3.ProjectOnPlane(transform.forward, Vector3.up)
                    );
        }
        

        // Front crawl cycle
        Vector3 forwardTorqueDirection = new Vector3(Mathf.Sin(time), 0, 0);
        float frontCycleAlignment = sideFrontBlend;
        forwardTorqueDirection = Vector3.Lerp(Vector3.forward, forwardTorqueDirection, crawlArmAngleRatio);
        if (torqueAxisRef != null)
        {
            forwardTorqueDirection = torqueAxisRef.transform.rotation * forwardTorqueDirection;
            frontCycleAlignment = Vector3.Dot(
                    Vector3.ProjectOnPlane(torqueAxisRef.transform.forward, Vector3.up),
                    Vector3.ProjectOnPlane(transform.forward, Vector3.up)
                    );
        }

        // Blend and apply
        //Vector3 torqueDir = Vector3.Slerp(sideCycleTorqueDirection, forwardTorqueDirection, sideFrontBlend);
        Vector3 torqueDir = sideCycleTorqueDirection * sideCycleAlignment * 3 + forwardTorqueDirection * frontCycleAlignment;
        rb.AddTorque(torqueDir * crawlTorque * crawlInput, ForceMode.Acceleration);

        
    }

    private void OnDrawGizmos(){

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(openingVerticalPosition, 0.1f);

    }
}
