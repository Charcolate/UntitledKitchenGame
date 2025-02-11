using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Rope : MonoBehaviour {
    [SerializeField]
    Rigidbody[] _endPoints;

    [SerializeField] float _radius = 0.25f;
    [SerializeField] float _totalLength = 10;

    [SerializeField] int _segmentCount = 8;
    [SerializeField] float _segmentMass = 0.2f;

    Rigidbody[] _segments;
    Vector3[] _linePoints;

    LineRenderer _line;

    // Start is called before the first frame update
    void Start() {
        if (_endPoints == null || _endPoints.Length < 2 || _endPoints[0] == null || _endPoints[1] == null) {
            Debug.LogError("Please assign endpoints to the rope.");
            return;
        }

        if (TryGetComponent(out _line)) {
            _linePoints = new Vector3[_segmentCount * 5];
            _line.positionCount = _linePoints.Length;
        }

        _segments = new Rigidbody[_segmentCount];
        float halfSegment = _totalLength / (2*_segmentCount);

        var span = _endPoints[1].transform.position - _endPoints[0].transform.position;
        var stride = span / (_segmentCount - 1);
        var orientation = Quaternion.LookRotation(span);
    
        var (body, joint) = MakeSegment(halfSegment);
        _segments[0] = body;

        var position = _endPoints[0].transform.position + stride/2f;
        body.transform.SetPositionAndRotation(position, orientation);
        joint.connectedBody = _endPoints[0];
        joint.connectedAnchor = Vector3.zero;

        for (int i = 1; i < _segmentCount; i++) {
            position += stride;
            var next = Instantiate(joint, position, orientation, transform);
            next.connectedBody = _segments[i-1];
            next.connectedAnchor = new Vector3(0, 0, halfSegment);
            body = next.GetComponent<Rigidbody>();
            _segments[i] = body;
        }

        joint = MakeJoint(body);
        joint.anchor = new Vector3(0, 0, halfSegment);
        joint.connectedBody = _endPoints[1];
        joint.connectedAnchor = Vector3.zero;
    }

    SpringJoint MakeJoint(Rigidbody body) {
        var joint = body.AddComponent<SpringJoint>();
        joint.autoConfigureConnectedAnchor = false;
        joint.spring = 50;
        joint.damper = 0.2f;
        return joint;
    }

    (Rigidbody, SpringJoint) MakeSegment(float halfSegment) {
        var go = new GameObject("Segment");
        go.transform.SetParent(transform);
        
        var shape = go.AddComponent<CapsuleCollider>();

        shape.radius = _radius;
        shape.height = 2 * (halfSegment + _radius);
        shape.direction = 2; // Z axis.

        var body = go.AddComponent<Rigidbody>();
        body.mass = _segmentMass;
        body.drag = 0.6f;
        body.angularDrag = 0.9f;

        body.constraints = RigidbodyConstraints.FreezePositionY
                         | RigidbodyConstraints.FreezeRotationX
                         | RigidbodyConstraints.FreezeRotationZ;

        var joint = MakeJoint(body);
        joint.anchor = new Vector3(0, 0, -halfSegment);

        return (body, joint);
    }

    void Update() {
        if (_line == null) return;

        float scale = _totalLength/(3 * _segmentCount);

        int p = 0;
        _linePoints[p++] = _endPoints[0].transform.position;        

        var step = _segments[0].transform.forward * scale;

        var prevAnchor = _segments[0].position;
        var prevControl = prevAnchor + step;

        _linePoints[p++] = 0.5f * (_linePoints[0] + prevAnchor);

        for (int segment = 1; segment < _segmentCount; segment++) {
            var nextAnchor = _segments[segment].position;
            step = _segments[segment].transform.forward * scale;
            var nextControl = nextAnchor - step;

            _linePoints[p++] = prevAnchor;
            for (int i = 0; i < 4; i ++) {
                float t = i * 0.25f;
                float inv = 1f-t;
                _linePoints[p++] = inv*inv*inv * prevAnchor
                                 + 3 * t * inv * inv * prevControl
                                 + 3 * t * t * inv * nextControl
                                 + t * t * t * nextAnchor; 
            }
            prevAnchor = nextAnchor;
            prevControl = nextAnchor + step;
        }

        _linePoints[p++] = prevAnchor;
        _linePoints[p++] = 0.5f * (prevAnchor + _endPoints[1].position);
        _linePoints[p++] = _endPoints[1].position;

        _line.SetPositions(_linePoints);
    }

    /*
    void OnDrawGizmos() {
        if (_segments == null || _segments.Length == 0) return;

        Vector3 previous = _endPoints[0].position;
        foreach (var segment in _segments) {
            Gizmos.DrawLine(previous, segment.position);
            previous = segment.position;
        }

        Gizmos.DrawLine(previous, _endPoints[1].position);
    }
    */
}
