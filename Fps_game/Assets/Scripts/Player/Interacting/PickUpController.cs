using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickUpController : MonoBehaviour
{
    public GunSystem gunScript;
    public Rigidbody rb;
    public BoxCollider coll;
    public Transform player, gunContainer, fpsCam;

    public float pickUpRange, dropForceUp, dropForceForward;

    public bool equipped;
    public static bool slotFull;

    void Awake() {
        gunScript = GetComponent<GunSystem>();
        rb = GetComponent<Rigidbody>();
        coll = GetComponent<BoxCollider>();
        player = GameObject.Find("Player").transform;
        fpsCam = player.Find("PlayerHead").Find("MainCamera").transform;
        gunContainer = fpsCam.Find("GunContainer");


        
    }
    void Start()
    {
        if(!equipped){
            rb.isKinematic = false;
            coll.isTrigger = false;
            gunScript.enabled = false;
        }
        if(equipped){
            rb.isKinematic = true;
            coll.isTrigger = true;
            gunScript.enabled = true;
            slotFull = true;
        }
    }

    void Update()
    {
        Vector3 DistanceToPlayer = player.position - transform.position;
        if(Input.GetKeyDown(KeyCode.E) && DistanceToPlayer.magnitude <= pickUpRange)
            if(!equipped && !slotFull)
                PickUp();
            else if(equipped && slotFull)
                Drop();
            else if(!equipped && slotFull)
                Invoke(nameof(PickUp), 0.1f);
    }

    void PickUp(){
        equipped = true;
        slotFull = true;

        //set gunContainer as parent and 
        transform.SetParent(gunContainer);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.Euler(Vector3.zero);
        transform.localScale = Vector3.one;

        //enable script and make object collision non interactable
        rb.isKinematic = true;
        coll.isTrigger = true;
        gunScript.enabled = true;
        rb.interpolation =RigidbodyInterpolation.None;
        coll.enabled = false;
    }

    void Drop(){
        equipped = false;
        slotFull = false;

        //make gun no longer a part of the player
        transform.SetParent(null);
        rb.isKinematic = false;
        coll.isTrigger = false;
        coll.enabled = true;
        rb.interpolation =RigidbodyInterpolation.Interpolate;

        //throw it with force
        rb.velocity = player.GetComponent<Rigidbody>().velocity;

        rb.AddForce(fpsCam.forward * dropForceForward, ForceMode.Impulse);
        rb.AddForce(fpsCam.up * dropForceUp, ForceMode.Impulse);

        float random = Random.Range(1f,1f);
        rb.AddTorque(new Vector3(random, random, random) * 10, ForceMode.Impulse);

        //disable script
        
        gunScript.enabled = false;
    }

    void Swap(){
        //drops current gun
        gunContainer.GetChild(0).GetComponent<PickUpController>().Drop();
        PickUp();
    }
}
