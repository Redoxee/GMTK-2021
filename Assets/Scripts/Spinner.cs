using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spinner : MonoBehaviour
{
    public float speed = 1f;

    void Update()
    {
        this.transform.Rotate(0, 0, this.speed * Time.deltaTime);    
    }
}
