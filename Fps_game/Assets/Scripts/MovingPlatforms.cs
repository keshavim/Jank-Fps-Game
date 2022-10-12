using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatforms : MonoBehaviour
{
    public Transform[] points;
    Transform target;
    int currentPoint;
    [SerializeField]int moveDirection;

    public bool moving;
    public float moveSpeed;
    public bool rotating;
    public float rotateSpeed, rotateDirection;
    

    void Awake(){
        if(transform.parent.childCount>1){
            points = new Transform[transform.parent.childCount-1];
            for(int i = 0; i < transform.parent.childCount - 1; ++i){
                points[i] = transform.parent.GetChild(i+1);
            }
            target = points[0];
            moving = true;
        }
        
    }

    void Update(){
        if(moving && target != null){
            MoveToNextPoint();
        }
        if(rotating){
            Rotate();
        }
    }

    void MoveToNextPoint(){
        transform.position = Vector3.MoveTowards(transform.position, target.position, moveSpeed * Time.deltaTime);
        if(Vector3.Distance(transform.position, target.position) < 0.01){
            if(currentPoint + moveDirection >= points.Length || currentPoint + moveDirection < 0) moveDirection *= -1;
            currentPoint += moveDirection;
            target = points[currentPoint];
        }
    }
    void Rotate(){
        transform.Rotate(0, rotateDirection * rotateSpeed,0);
    }

    void OnCollisionEnter(Collision other)
    {
        foreach (ContactPoint p in other.contacts)
        {
            if(p.normal.y < -0.5){
                Transform parent = other.transform;
                while(parent.parent != null) parent = parent.parent;
                parent.SetParent(transform);
            }
        }
    }

    void OnCollisionExit(Collision other)
    {
    
        Transform parent = other.transform;
        while(parent.parent != null){
            if(parent.parent == transform){
                parent.SetParent(null);
            }
            else parent = parent.parent;
        } 
    }
}
