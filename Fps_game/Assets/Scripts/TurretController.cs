using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretController : MonoBehaviour
{
    [SerializeField]Transform player;
    [SerializeField]LayerMask playerMask;
    //searching
    [SerializeField]Transform gun;

    [Header("Searching")]
    public Vector3 minAngle, maxAngle, rotateDirection;
    [Range(1,5)]public float rotationSpeed;
    Quaternion rotationPoint;
    bool rotationPointSet;

    [Header("Attacking")]
    public float attackRange;
    public bool playerInAR;
    Vector3 lastSightPoint;
    public int health;

    public int damage;
    public float timeBetweenShooting, timeBetweenShots, spread, reloadTime;
    public int magazineSize, bulletsPerTap;
    public bool readyToShoot;
    int bulletsLeft, bulletsShot;
    public GameObject bulletRef;
    public float forwardForce, upForce;
    public Transform attackPoint;
    RaycastHit rayHit;


    // Start is called before the first frame update
    void Awake()
    {
        player = GameObject.Find("Player").transform;
        playerMask = LayerMask.GetMask("Player");
        gun = transform.Find("Gun").transform;
        bulletsLeft = magazineSize;
        readyToShoot = true;

    }
    void Update(){
        if(health > 0){
            SearchForPlayer();

            if(!playerInAR){
                Searching();
            } 
            if(playerInAR){
                AttackPlayer();
            } 
        } else gun.rotation = Quaternion.Euler(45, gun.rotation.y, gun.rotation.z);
        
        
    }
//detects if the enemy can see the player and attak the player
    void SearchForPlayer(){
        //checks if player is in front and not behind a wall
        var playerDirection = player.position - gun.position;
        var angle = Vector3.Angle(gun.forward, playerDirection);
        Debug.DrawRay(gun.position, gun.forward * 10, Color.blue);
        RaycastHit firstHit;
        Physics.Raycast(gun.position, playerDirection, out firstHit);

        //checks if player is in sight or attackrange
        if(Mathf.Abs(angle) < 90 && firstHit.collider.gameObject.name == "PlayerBody"){
            playerInAR = Physics.CheckSphere(gun.position, attackRange, playerMask);
        }
        else if(firstHit.collider.gameObject.name != "PlayerBody"){
            playerInAR = false;
        }
    }
    void Searching(){
        //gets new rotaion angle
        if(!rotationPointSet){
            rotationPoint = Quaternion.Euler(
                Random.Range(minAngle.x, maxAngle.x) * rotateDirection.x,
                Random.Range(minAngle.y, maxAngle.y) * rotateDirection.y,
                Random.Range(minAngle.z, maxAngle.z) * rotateDirection.z
            );
            rotationPointSet = true;
            
        }
        //rotates with slerp
        if(rotationPointSet){
            if((gun.localEulerAngles - rotationPoint.eulerAngles).magnitude > 2){
                gun.localRotation = Quaternion.Slerp(Quaternion.LookRotation(gun.forward), rotationPoint,  rotationSpeed * Time.deltaTime);
            } else rotationPointSet = false;
        }
        
    }
    void AttackPlayer(){
    
        var playerDirection = (player.position - new Vector3(0,0.5f * transform.localScale.y,0)) - gun.position;
        var angle = Quaternion.LookRotation(playerDirection);
        gun.localRotation = Quaternion.Slerp(Quaternion.LookRotation(gun.forward), angle,  rotationSpeed* 10f * Time.deltaTime);

        if(bulletsLeft > 0 && readyToShoot && playerInAR){
            bulletsShot = bulletsPerTap;
            Shoot();
        }
        else if(bulletsLeft <= 0)Invoke(nameof(Reload), reloadTime);
    }
//shooting fuction
    void Shoot(){
        readyToShoot = false;
        bulletsLeft--;
        bulletsShot--;

        //Spread
        float x = Random.Range(-spread, spread);
        float y = Random.Range(-spread, spread);
        float z = Random.Range(-spread, spread);
        Vector3 direction = gun.forward + new Vector3(x, y, z);
        var curentb = Instantiate(bulletRef, attackPoint.position, Quaternion.identity).GetComponent<BulletController>();
        curentb.hitPlayer = true;
        curentb.Fire(direction, direction * forwardForce, gun.up* upForce, damage, attackRange);

        if(bulletsShot == 0)
            Invoke(nameof(ResetShot), timeBetweenShooting);

        if(bulletsShot > 0 && bulletsLeft > 0){
            Invoke(nameof(Shoot), timeBetweenShots);
        }
            
            
    }
    void ResetShot() {
        readyToShoot = true;
    }
    void Reload(){
        bulletsLeft = magazineSize;
    }

    public void TakeDamage(int damage){
        health -= damage;
        if(health <= 0){
            
            Destroy(gameObject, 4);
        }
    }
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }

    
}
