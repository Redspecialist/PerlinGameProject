using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotationPerSec : MonoBehaviour {

    public GameObject self;
    public float speed;
    private float curr = 0;
    private void Awake()
    {
    }

    // Update is called once per frame
    void Update () {
        curr += speed * Time.deltaTime;
        self.transform.rotation = Quaternion.AngleAxis(curr,Vector3.up);
	}
}
