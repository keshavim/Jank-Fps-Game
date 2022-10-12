
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.AI;

//Base enemy controller class. containts basic enemy actions and variables
public class ChaserController : MonoBehaviour
{
    //variables
    [SerializeField]NavMeshAgent agent;
    [SerializeField]Transform player;
    [SerializeField]LayerMask playerMask;

    //patroling
    [SerializeField] Vector3 walkPoint;
    bool walkPointSet;
    [SerializeField] float walkPointRange;

    //Attacking
    [SerializeField] float attackCooldown;
    bool attacked;
    [SerializeField] Transform attackPoint;
    public float sightRange, attackRange, forwardForce, upForce;
    public GameObject bulletref;
        
    public bool playerInSR, playerInAR;

    public int health;
    public int damage;
    float agroed;
    [SerializeField]Vector3 deathRotation;



    // Start is called before the first frame update
    void Awake()
    {
        player = GameObject.Find("Player").transform;
        agent = GetComponent<NavMeshAgent>();

        playerMask = LayerMask.GetMask("Player");



    }
    void Update(){
        if(health > 0){
            SearchForPlayer();

            if(!playerInSR && !playerInAR) Patroling();
            if(playerInSR && !playerInAR) ChasePlayer();
            if(playerInAR && playerInSR) AttackPlayer();
        }
        else{
            transform.rotation = Quaternion.Euler(deathRotation);
            GetComponent<CapsuleCollider>().enabled = false;
        }
        
    }
//detects if the enemy can see the player and attak the player
    void SearchForPlayer(){
        //checks if player is in front and not behind a wall
        var playerDirection = player.position - transform.position;
        var angle = Vector3.Angle(transform.forward, playerDirection);
        
        RaycastHit firstHit;
        Physics.Raycast(transform.position, playerDirection, out firstHit);
        //checks if player is in sight or attackrange
        if(Mathf.Abs(angle) < 80 && firstHit.collider.gameObject.name == "PlayerBody"){
            playerInSR = Physics.CheckSphere(transform.position, sightRange, playerMask);
            playerInAR = Physics.CheckSphere(transform.position, attackRange, playerMask);

        }
        if(firstHit.collider.gameObject.name != "PlayerBody") playerInAR = false;
        

    }
    //enemy walks around
    void Patroling(){
        if(!walkPointSet) SetWalkPoint();
    
        if(walkPointSet) agent.SetDestination(walkPoint);

        Vector3 distToPoint = (transform.position - new Vector3(0, transform.position.y)) - (walkPoint - new Vector3(0, walkPoint.y));

        if(distToPoint.magnitude < 1f) walkPointSet = false;
    }
    //next point to walk to 
    void SetWalkPoint(){
        //calculate random range
        if(agroed > 0){
            float randomx = Random.Range(-5, 5);
            float randomz = Random.Range(5, 15);
            walkPoint = new Vector3(transform.position.x + randomx, transform.position.y, transform.position.z +randomz);
            agroed -= Time.deltaTime;
        }else{
            float randomx = Random.Range(-walkPointRange, walkPointRange);
            float randomz = Random.Range(-walkPointRange, walkPointRange);
            walkPoint = new Vector3(transform.position.x + randomx, transform.position.y, transform.position.z +randomz);
        }
        
        NavMeshPath path = new NavMeshPath();
        if(agent.CalculatePath(walkPoint, path) && path.status == NavMeshPathStatus.PathComplete){
            walkPointSet = true;
        }
    }


    void ChasePlayer(){
        agent.SetDestination(player.position);
    }
    void AttackPlayer(){
        agent.SetDestination(transform.position);
        transform.LookAt(player.position - new Vector3(0, 0.6f, 0), Vector3.up);

        if(!attacked){
            attacked = true;
            
            var curentb = Instantiate(bulletref, attackPoint.position, Quaternion.identity).GetComponent<BulletController>();
            curentb.Fire(transform.forward, transform.forward * forwardForce, transform.up* upForce, damage, sightRange);

            Invoke(nameof(ResetAttack), attackCooldown);
        }
    }
    void ResetAttack() => attacked = false;

    public void TakeDamage(int damage){
        health -= damage;
        if(health <= 0){
            
            Destroy(gameObject, 4);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, walkPointRange);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, sightRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }

    void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag == "Bullet" && !playerInAR || !playerInSR && health > 0){
            transform.LookAt(collision.transform, Vector3.up);
            agroed = Random.Range(10,20);
        }
    }


}
