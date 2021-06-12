using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float dampFactor = 1f;
    public float shakeFactor = 10f;
    public float shakeVelocityFactor = 10f;

    private Vector3 targetPosition;
    private Vector3 currentVelocity;

    void Start()
    {
        this.targetPosition = this.transform.position;
        this.currentVelocity = new Vector3();
    }

    void Update()
    {
        this.transform.position = Vector3.SmoothDamp(this.transform.position, this.targetPosition, ref this.currentVelocity, this.dampFactor);
    }

    public void Impulse(Vector2 direction, float force)
    {
        Debug.Log($"Impulse {direction}");
        Vector3 dir = new Vector3(direction.x, direction.y, 0) * force;
        this.transform.position += dir * this.shakeFactor;
        this.currentVelocity += dir * this.shakeVelocityFactor;
    }
}
