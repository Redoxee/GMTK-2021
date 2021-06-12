using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField]
    private float travelDuration = 2f;

    [SerializeField]
    private AnimationCurve speedCurve;

    [SerializeField]
    CameraController camera;

    public bool isShooting = false;
    private float timer;
    private List<Character.PathNode> path;
    private int currentStep;

    private float shootDistance;

    void Start()
    {
    }

    internal void Shoot(List<Character.PathNode> path, float totalDistance)
    {
        this.path = path;
        Character.PathNode[] arrayPath = path.ToArray();
        this.timer = 0;
        this.currentStep = 0;
        this.shootDistance = totalDistance;
        this.isShooting = true;
        this.transform.position = path[0].Position;
        this.gameObject.SetActive(true);
    }

    void Update()
    {
        if (!this.isShooting)
        {
            return;
        }

        this.timer += Time.deltaTime;
        float newDist = this.speedCurve.Evaluate(this.timer / this.travelDuration) * this.shootDistance;

        if (newDist < this.shootDistance)
        {

            Character.PathNode step = this.path[this.currentStep];
            if (newDist > step.EndDistance)
            {
                while (newDist > step.EndDistance && this.currentStep < this.path.Count - 1)
                {
                    // TODO : Bump;
                    this.currentStep++;
                    step = this.path[this.currentStep];
                    this.camera.Impulse(step.Normal, step.TurnRate);
                }
            }

            float remainingDist = newDist - step.StartDistance;
            Vector2 position = step.Position + remainingDist * step.Direction;
            this.transform.position = position;
        }

        else
        {
            Character.PathNode lastStep = this.path[this.path.Count - 1];
            Vector2 endPos = lastStep.Position + lastStep.Direction * lastStep.Length;
            this.transform.position = endPos;
            this.isShooting = false;
        }
        
    }
}
