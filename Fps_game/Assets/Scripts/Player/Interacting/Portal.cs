
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;



public class Portal : MonoBehaviour
{



    [SerializeField] Door exit, entrance;
    [SerializeField] Transform spawnPos;
    [SerializeField] bool typeEntrance;

    [SerializeField] Transform player;
    [SerializeField] BoxCollider col;
    [SerializeField]CameraController cc;

    static int lastScene = 0;

    void Awake(){
        player = GameObject.Find("Player").transform;
        col = GetComponent<BoxCollider>();
        cc = Camera.main.GetComponent<CameraController>();
        if(player.GetComponent<PlayerMovement>().reachedEnd){
            typeEntrance = !typeEntrance;
        }
    }

    void Start()
    {
        
        if(lastScene < SceneManager.GetActiveScene().buildIndex && typeEntrance){
            exit.InteractWithDoor();
            player.position = spawnPos.position;
            player.GetComponent<Interactions>().spawnPos = spawnPos.position;
            cc.yOffset = transform.rotation.eulerAngles.y - 180;
        }
                
        else if(lastScene > SceneManager.GetActiveScene().buildIndex && !typeEntrance){
            exit.InteractWithDoor();
            player.position = spawnPos.position;
            player.GetComponent<Interactions>().spawnPos = spawnPos.position;
            cc.yOffset = transform.rotation.eulerAngles.y - 180;
        }
        

    }
    void OnTriggerEnter(Collider other)
    {

        if(other.name.Contains("Player")){
            lastScene = SceneManager.GetActiveScene().buildIndex;
            if(!typeEntrance)
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
            else if(typeEntrance)
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);

        }
    }

}
