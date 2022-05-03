using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class ConnectUIElementsWithLine : MonoBehaviour {
    public RectTransform origin;
    public RectTransform target;

    public float z_pos = 0;

    private LineRenderer _renderer;

    public int segments = 4;
    
    // Update is called once per frame
    void Update() {
        _renderer = GetComponent<LineRenderer>();
        if (origin == null) {
            _renderer.enabled = false;
        }else if (target == null) {
            _renderer.enabled = true;

            var firstPos = origin.position;
            var secondPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            firstPos = new Vector3(firstPos.x, firstPos.y, z_pos);
            secondPos = new Vector3(secondPos.x, secondPos.y, z_pos);
            
            _renderer.SetPositions(CreateSegments(firstPos, secondPos, segments));
        } else {
            _renderer.enabled = true;
            
            var firstPos = origin.position;
            var secondPos = target.position;
            
            
            firstPos = new Vector3(firstPos.x, firstPos.y, z_pos);
            secondPos = new Vector3(secondPos.x, secondPos.y, z_pos);
            
            _renderer.SetPositions(CreateSegments(firstPos, secondPos, segments));
        }
    }


    Vector3[] CreateSegments(Vector3 firstPos, Vector3 secondPos, int segments) {
        var segs = new Vector3[segments];

        for (int i = 0; i < segments-1; i++) {
            segs[i] = Vector3.Lerp(firstPos,secondPos, (float)i/segments);
        }

        segs[segments - 1] = secondPos;

        return segs;
    }
}
