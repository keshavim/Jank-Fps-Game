
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Range(100, 1000)]public float senX = 400, senY = 400;

    public Transform orientation, cameraPos;

    float xRotation, yRotation;
    public float yOffset;
    void Awake(){
        orientation = GameObject.Find("Player").transform;
        cameraPos = orientation.Find("PlayerHead");
    }
    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Update is called once per frame
    void Update()
    {
        //gets input and calculates rotation
        float mouseX = Input.GetAxisRaw("Mouse X") * senX * Time.deltaTime;
        float mouseY = Input.GetAxisRaw("Mouse Y") * senY * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        yRotation += mouseX;

        //sets rotation and position
        transform.rotation = Quaternion.Euler(xRotation, yRotation + yOffset, 0f);
        orientation.rotation = Quaternion.Euler(0f, yRotation + yOffset, 0f);


    }

}
