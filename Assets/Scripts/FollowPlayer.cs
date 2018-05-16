using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPlayer : MonoBehaviour {

    public Transform target;
	// Update is called once per frame
	void Update () {
        Vector3 newPos = new Vector3(target.position.x,transform.position.y,target.position.z);
        transform.position = newPos;
	}
}
