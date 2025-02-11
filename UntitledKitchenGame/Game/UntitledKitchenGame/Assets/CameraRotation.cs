using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraRotation : MonoBehaviour
{
    public Camera _camera;
    private void Start()
    {
        //hoepfully it grabs any camera in the scene rn we go public
    }
    void Update()
    {
        transform.LookAt(_camera.transform);
    }
}
