using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField]
    private float speed = 20f;
    

    private float timer;
    private Vector2[] path;
    private float[] pathSteps;
    private int currentStep;
    void Start()
    {
        
    }

    internal void Shoot(Vector2[] path)
    {
        this.path = path;
        this.timer = 0;
        float total = 0;
        if (this.pathSteps.Length != path.Length)
        {
            this.pathSteps = new float[path.Length];
        }

        for (int index = 1; index < path.Length; ++index)
        {
            total += (path[index] - path[index - 1]).magnitude;
            this.pathSteps[index - 1] = total;
        }
    }

    void Update()
    {
           
    }
}
