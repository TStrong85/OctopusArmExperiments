using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CollisionInfo : MonoBehaviour
{
    // Add the option to filter based on Layers
    public LayerMask collisionMask;

    // Track information about collisions that can be read elsewhere
    public int numberOfCollisions;
    public Vector3 collisionPosition;
    public Vector3 collisionNormal;

    // Keep events that can be triggered
    public UnityEvent CollisionEnterEvent;
    public UnityEvent CollisionExitEvent;
    public UnityEvent TriggerEnterEvent;
    public UnityEvent TriggerExitEvent;

    private void OnCollisionEnter(Collision collision)
    {
        if ((collisionMask.value & (1 << collision.gameObject.layer)) != 0)
        {
            CollisionEnterEvent?.Invoke();

            // Update tracked info
            if (numberOfCollisions == 0)
            {
                collisionPosition = collision.contacts[0].point;
                collisionNormal = collision.contacts[0].normal;
            }
            numberOfCollisions += 1;
        }

        
    }

    private void OnCollisionExit(Collision collision)
    {
        if ((collisionMask.value & (1 << collision.gameObject.layer)) != 0) { 
            CollisionExitEvent?.Invoke();

            // Update tracked info
            numberOfCollisions -= 1;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if ((collisionMask.value & (1 << other.gameObject.layer)) != 0)
            TriggerEnterEvent?.Invoke();
    }

    private void OnTriggerExit(Collider other)
    {
        if ((collisionMask.value & (1 << other.gameObject.layer)) != 0)
            TriggerExitEvent?.Invoke();
    }
}
