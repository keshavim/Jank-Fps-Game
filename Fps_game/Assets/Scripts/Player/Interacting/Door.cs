using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    public float direction;
    public float speed;

    public bool locked;
    public bool opening;

    Quaternion newRo;
    void Start(){
        newRo = transform.localRotation;
    }
    void Update(){
        if(opening){
            if(transform.localRotation != newRo)
                transform.localRotation = Quaternion.Slerp(transform.localRotation, newRo, speed * Time.deltaTime);
            else{
                opening = false;
                direction *= -1;
            }
            
        }
    }

    public void InteractWithDoor(){
        if(!locked){
            if(opening) direction *= -1;
            newRo = Quaternion.Euler(newRo.eulerAngles + new Vector3(0, 90 * direction));
            opening = true;
        }
    }
}
