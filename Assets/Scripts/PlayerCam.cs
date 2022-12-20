using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCam : MonoBehaviour
{
    
    public float sensitivityX, sensitivityY;
    public Transform orientation;
    float rotationX, rotationY;


    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OnEnable()
    {
        rotationX = orientation.rotation.x;
        rotationY = orientation.rotation.y;

        Debug.Log("y orientation: " + rotationY + "     should be: " + orientation.rotation.y);
    }

    // Update is called once per frame
    void Update()
    {
        float mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * sensitivityX;
        float mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * sensitivityY;

        //Moving the mouse on it's X axis causes the camera to rotate about the world's Y axis, same for the opposite axiis
        rotationY += mouseX;
        rotationX -= mouseY;

        rotationX = Mathf.Clamp(rotationX, -90f, 90);

        transform.rotation = Quaternion.Euler(rotationX, rotationY, 0);
        orientation.rotation = Quaternion.Euler(orientation.rotation.x, rotationY, orientation.rotation.z);
    }
}
