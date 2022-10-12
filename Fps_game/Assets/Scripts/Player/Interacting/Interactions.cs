using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;


public class Interactions : MonoBehaviour
{
    public Camera fpscam;
    public float reach;

    RaycastHit hitInfo;
    public LayerMask interactableMask;

    GameObject obj;
    public int maxHealth, health;
    public Vector3 spawnPos;
    public TextMeshProUGUI healthtext, GOtext;

    

    void Awake(){
        fpscam = Camera.main;
        health = maxHealth;
        GOtext.SetText("");
    }
    
    void Update()
    {
        if(Input.GetMouseButtonDown(1)){
            if(Physics.Raycast(transform.position, fpscam.transform.forward, out hitInfo, reach, interactableMask)){
                obj = hitInfo.transform.gameObject;
                if(obj.name.Contains("Door"))
                    obj.GetComponent<Door>().InteractWithDoor();
                
                
            }
        }
        if(Input.GetKeyDown(KeyCode.R) && health <= 0){
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            transform.position = spawnPos;
        }    
        healthtext.SetText($"{health}/{maxHealth}");
        if(health == 0){
            GOtext.SetText("GameOver\nPress R");
            transform.Rotate(new Vector3(0, 0, 90));
            GetComponent<Rigidbody>().isKinematic = true;
        }
        

    }
    public void TakeDamage(int damage){
        health -= damage;
        health = Math.Clamp(health, 0, maxHealth);
    }

    
}
