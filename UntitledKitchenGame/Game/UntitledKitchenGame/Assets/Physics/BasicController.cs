using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Searcher.SearcherWindow.Alignment;

public class BasicController : MonoBehaviour {
    public string horizontalAxis;
    public string verticalAxis;

    public float maxSpeed = 10;

    public float maxAccel = 10;

    [SerializeField, HideInInspector]
    Rigidbody _body;

    void OnValidate() {
        if (_body == null)
            TryGetComponent(out _body);
    }

    void FixedUpdate() {
         var input = new Vector3(
            Input.GetAxis(horizontalAxis),
            0,
            Input.GetAxis(verticalAxis)
        );
        input.x= Mathf.Abs(input.x) < 0.5f?0:input.x;
        input.z = Mathf.Abs(input.z) < 0.5f ? 0 : input.z;

        Vector3 cameraForward = Camera.main.transform.forward;
        Vector3 cameraRight = Camera.main.transform.right;
        cameraForward.y = 0; 
        cameraRight.y = 0;

        input=input.x*cameraRight.normalized+input.z* cameraForward.normalized;

        var desiredVel = maxSpeed * input;

        var accel = (desiredVel - _body.velocity) / Time.deltaTime;

        accel = Vector3.ClampMagnitude(accel, maxAccel);
        
        _body.AddForce(accel, ForceMode.Acceleration);
    }
}
