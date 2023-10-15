using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Simple Component that adds a constant force every frame.
//   Initially created to simulate and dynamically tweak gravity forces on the arms.
public class ArtificialGravity : MonoBehaviour
{
    public bool addInLocalSpace = false;
    public Vector3 gravityVector;
    private Rigidbody rb;

    // Start is called before the first frame update
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        Vector3 gravityForce = gravityVector;
        if (addInLocalSpace) 
        {
            rb.AddRelativeForce(gravityForce, ForceMode.Acceleration);
        } 
        else
        {
            rb.AddForce(gravityForce, ForceMode.Acceleration);
        }
    }

    public Rigidbody getRigidbody()
    {
        return rb;
    }
}
