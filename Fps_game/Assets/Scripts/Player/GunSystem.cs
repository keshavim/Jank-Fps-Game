using FirstGearGames.SmoothCameraShaker;
using UnityEngine;
using TMPro;

public class GunSystem : MonoBehaviour
{
    [Header("Gun Stats")]
    public int damageMulti;
    public float timeBetweenShooting, timeBetweenShots, range, spread, reloadTime;
    public int magazineSize, magazines, bulletsPerTap;
    public bool allowHold;
    int bulletsLeft,maxBulletsLeft, bulletsShot;

    public string clipName;

    bool shooting, readyToShoot, reloading, allowinvoke;
    public float recoil;

    [SerializeField]GameObject bullet;

    [SerializeField] float shootForce, upwardForce;

    public Camera fpsCam;
    public Rigidbody rb, playerRB;
    public Transform attackPoint;
    public ShakeData shakeData;
    public GameObject muzzleFlash;
    public GameObject[] bulletHoleGraphic;

    public TextMeshProUGUI text;

    void Awake(){
        bulletsLeft = magazineSize;
        maxBulletsLeft = magazines * magazineSize;
        readyToShoot = true;
        playerRB = GameObject.Find("Player").GetComponent<Rigidbody>();
        rb = GetComponent<Rigidbody>();
        fpsCam = Camera.main;
        
        text = GameObject.FindWithTag("AmmoCounter").GetComponent<TextMeshProUGUI>();

        
    }
    void OnDisable()
    {
        text.SetText("");
    }
    void Update(){
        PlayerInputs();
        text.SetText($"{bulletsLeft/bulletsPerTap} / {maxBulletsLeft/bulletsPerTap}");
        
    }

    void PlayerInputs(){
        if(allowHold) shooting = Input.GetKey(KeyCode.Mouse0);
        else shooting = Input.GetKey(KeyCode.Mouse0);

        if(Input.GetKeyDown(KeyCode.R) && bulletsLeft < magazineSize && !reloading) Reload();
        if(shooting && !reloading && readyToShoot && bulletsLeft < 0) Reload();

        //shooting
        if(shooting && !reloading && readyToShoot && bulletsLeft > 0){
            bulletsShot = bulletsPerTap;
            Shoot();
            
        }
    }
    void Shoot(){
        readyToShoot = false;
        bulletsLeft--;
        bulletsShot--;

        //fireing a ray form center of screen
        Ray ray = fpsCam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit rayHit;
        //checking the point of it
        Vector3 targetpoint = Physics.Raycast(ray, out rayHit) ? rayHit.point : ray.GetPoint(100);

        //direction the bullet moves
        float x = Random.Range(-spread, spread);
        float y = Random.Range(-spread, spread);
        float z = Random.Range(-spread, spread);
        Vector3 direction = (targetpoint - attackPoint.position) + new Vector3(x, y, z);

        //cosmetic stuff
        var currentbullet = Instantiate(bullet,attackPoint.position, Quaternion.identity).GetComponent<BulletController>();
        currentbullet.Fire(direction.normalized, direction.normalized * shootForce, fpsCam.transform.up * upwardForce, damageMulti, range);
        
        FindObjectOfType<AudioManager>().Play(clipName);

        if(bulletsShot == 0)
            playerRB.AddForce(-direction.normalized * recoil, ForceMode.Impulse);
        if(shakeData != null )CameraShakerHandler.Shake(shakeData);
        
        if(bulletsShot == 0)
            Invoke(nameof(ResetShot), timeBetweenShooting);
        //burst shots
        if(bulletsShot > 0 && bulletsLeft > 0)
            Invoke(nameof(Shoot), timeBetweenShots);
    }
    void ResetShot() {readyToShoot = true;}

    
    void Reload(){
        if(maxBulletsLeft > 0){
            maxBulletsLeft -= magazineSize - bulletsLeft;
            reloading = true;
            transform.localRotation = Quaternion.Euler(45,0,0);
            Invoke(nameof(ReloadingFinished), reloadTime);
        }
        
    }
    void ReloadingFinished(){
        bulletsLeft = magazineSize;
        if(bulletsLeft < 0) bulletsLeft = 0;
        transform.localRotation = Quaternion.Euler(0,0,0);
        reloading = false;
    }
}
