using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RedGizmos : MonoBehaviour
{

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, Vector3.up);
    }
}