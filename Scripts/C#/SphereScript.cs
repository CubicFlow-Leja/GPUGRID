using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SphereScript : MonoBehaviour
{
    private float Offset;
    void Start()
    {
        Offset = Random.Range(-100f, 100f);
    }

    
    void FixedUpdate()
    {
        this.GetComponent<Rigidbody>().velocity+=(9*(Vector3.forward * Mathf.Sin(Offset + Time.realtimeSinceStartup/5) + Vector3.right * Mathf.Cos(Offset + Time.realtimeSinceStartup/5))-Vector3.up*10- this.GetComponent<Rigidbody>().velocity)*Time.fixedDeltaTime*1.5f;
    }
}
