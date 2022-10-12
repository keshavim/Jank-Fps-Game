using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Liquids : MonoBehaviour
{
    public float drag;
    public float damage;
    public float tickRate;
    bool damaging;
    void OnTriggerStay(Collider other)
    {
        if(other.tag == "Player"){
            other.attachedRigidbody.AddForce(-(Physics.gravity * 0.2f), ForceMode.Force);
            other.GetComponentInParent<PlayerMovement>().inWater = true;
            other.GetComponentInParent<PlayerMovement>().waterDrag = drag;
            if(damaging){
                //implement damage fuction later
            }
        }
        else {
            other.attachedRigidbody.drag =5;
            other.attachedRigidbody.AddForce(-(Physics.gravity * 1.1f), ForceMode.Force);
            other.attachedRigidbody.angularVelocity = Vector3.zero;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if(other.tag == "Player"){
            other.GetComponentInParent<PlayerMovement>().inWater = false;
            
        }
    }
}
