
using UnityEngine;
using FirstGearGames.SmoothCameraShaker;
using System;

public class BulletController : MonoBehaviour
{
    [SerializeField] Rigidbody rb;
    [SerializeField] GameObject explosion;
    public bool hitPlayer;
    [SerializeField] LayerMask playerMask;

    [Range(0f,1f), SerializeField]float bounciness;
    [SerializeField] bool useGravity;
    Vector3 spawnPoint;
    public int explosionDmg;
    [SerializeField] float explosionRange, hitRange;
    [SerializeField] float explosionForce;

    [SerializeField] int maxCollisions;
    [SerializeField] float lifeSpan;
    [SerializeField] bool explodeOnTouch = true, exploding;

    public ShakeData shakeData;
    public float shakeRange;
    public GameObject muzzleFlash;
    public GameObject[] bulletHoleGraphic;

    int collisions;
    PhysicMaterial physM;


    void Awake(){
        Setup();
        if(muzzleFlash){
            var muzzle = Instantiate(muzzleFlash, transform.position, Quaternion.identity);
            muzzle.GetComponent<ParticleSystem>().Play();
        
            Destroy(muzzle, 2);
        }
      
    
    }
    //seting up bullet
    void Setup(){
        physM = new PhysicMaterial();
        physM.bounciness = bounciness;
        physM.frictionCombine = PhysicMaterialCombine.Minimum;
        physM.bounceCombine = PhysicMaterialCombine.Maximum;
        GetComponent<SphereCollider>().material = physM;
        rb = GetComponent<Rigidbody>();

        rb.useGravity = useGravity;
        spawnPoint = transform.position;
    }
    public void Fire(Vector3 forward, Vector3 forwardForce, Vector3 upForce, int damage, float range){
        transform.forward = forward;
        rb.AddForce(forwardForce, ForceMode.Impulse);
        rb.AddForce(upForce, ForceMode.Impulse);
        explosionDmg *= damage;
        hitRange = range;
    }
    //checking if should explode
    void Update(){
        if(maxCollisions <= collisions) Explode();

        lifeSpan -= Time.deltaTime;
        if(lifeSpan < 0) Explode();

        if(Vector3.Distance(spawnPoint, transform.position) > hitRange) Explode();
    }

    //exploding and damaging
    void Explode(){
        if(exploding) return;
        exploding = true;

        if(explosion != null) Destroy(Instantiate(explosion, transform.position, Quaternion.identity), 2);

        Collider[] hits = Physics.OverlapSphere(transform.position, explosionRange);
        for (int i = 0; i < hits.Length; i++)
        {
            
            if(hits[i].CompareTag("Chaser")){
                hits[i].GetComponent<ChaserController>().TakeDamage(explosionDmg);
            }
            else if(hits[i].CompareTag("Turret")){
                hits[i].GetComponent<TurretController>().TakeDamage(explosionDmg);
            }
            else if(hits[i].CompareTag("Player") && hitPlayer){
                hits[i].GetComponentInParent<Interactions>().TakeDamage(explosionDmg);
                if(shakeData != null && Physics.OverlapSphere(transform.position, shakeRange, playerMask).Length > 0) CameraShakerHandler.Shake(shakeData);
            }

            if(hits[i].GetComponent<Rigidbody>())
                hits[i].attachedRigidbody.AddExplosionForce(explosionForce,transform.position, explosionRange);

        }
        Destroy(gameObject, 0.05f);
        if(explosionRange > 1)
            FindObjectOfType<AudioManager>().Play("Explosion");
    }

    
    //checkings collision and if should explode
    void OnCollisionEnter(Collision collision)
    {
        collisions++;
        var layer = collision.gameObject.layer;
        if(layer == LayerMask.NameToLayer("Enemy")&& explodeOnTouch) Explode();
        if(hitPlayer && layer == LayerMask.NameToLayer("Player") && explodeOnTouch) Explode();

        

        if(bulletHoleGraphic.Length > 0)
            foreach (ContactPoint p in collision.contacts)
            {
                if(collision.gameObject.tag == "Chaser" || collision.gameObject.tag == "Player"){
                    var bh = Instantiate(bulletHoleGraphic[1], p.point, Quaternion.LookRotation(p.normal));
                    Destroy(bh, 3);
                }
                else{
                    var bh = Instantiate(bulletHoleGraphic[0], p.point, Quaternion.LookRotation(p.normal));
                    Destroy(bh, 2);
                    FindObjectOfType<AudioManager>().Play("BulletImpact");
                }
            }
            
    }

    void OnDrawGizmosSelected()
    {
        
    }
}
