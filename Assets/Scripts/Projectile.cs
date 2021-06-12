using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField]
    private float travelDuration = 2f;

    private float endDelayHide = 1f;

    [SerializeField]
    private AnimationCurve speedCurve;

    [SerializeField]
    CameraController camera;

    [SerializeField]
    LineRenderer LinkRenderer = null;

    [SerializeField]
    GameManager GameManager = null;

    public bool isShooting = false;
    private float timer;
    private List<Character.PathNode> path;
    private int currentStep;
    private List<Character.TriggerHit> triggerHits;
    int currentTrigger = 0;

    private float shootDistance;

    void Start()
    {
    }

    internal void Shoot(List<Character.PathNode> path, float totalDistance, List<Character.TriggerHit> triggerHits)
    {
        this.path = path;
        Character.PathNode[] arrayPath = path.ToArray();
        this.timer = 0;
        this.currentStep = 0;
        this.shootDistance = totalDistance;
        this.isShooting = true;
        this.transform.position = path[0].Position;
        this.gameObject.SetActive(true);
        this.triggerHits = triggerHits;
        this.currentTrigger = 0;
        this.LinkRenderer.positionCount = 1;
        this.LinkRenderer.SetPosition(0, this.transform.position);
    }

    void Update()
    {
        if (!this.isShooting)
        {
            if (this.timer > 0)
            {
                this.timer -= Time.deltaTime;
                if (timer <= 0)
                {
                    this.gameObject.SetActive(false);
                    if (this.currentTrigger < 2)
                    {
                        this.LinkRenderer.positionCount = 0;
                    }
                }
            }

            return;
        }

        this.timer += Time.deltaTime;
        float newDist = this.speedCurve.Evaluate(this.timer / this.travelDuration) * this.shootDistance;

        if (this.triggerHits.Count > 0 && this.currentTrigger < this.triggerHits.Count)
        {
            Character.TriggerHit triggerHit = this.triggerHits[this.currentTrigger];
            if (triggerHit.DistanceHitted <= newDist)
            {
                
                this.currentTrigger++;
                this.LinkRenderer.SetPosition(this.LinkRenderer.positionCount - 1, triggerHit.Trigger.transform.position);
                this.LinkRenderer.positionCount++;
                this.LinkRenderer.SetPosition(this.LinkRenderer.positionCount - 1, triggerHit.Trigger.transform.position);
            }
        }

        if (newDist < this.shootDistance)
        {

            Character.PathNode step = this.path[this.currentStep];
            if (newDist > step.EndDistance)
            {
                while (newDist > step.EndDistance && this.currentStep < this.path.Count - 1)
                {
                    this.currentStep++;
                    step = this.path[this.currentStep];
                    this.camera.Impulse(step.Normal, step.TurnRate);

                    if (this.LinkRenderer.positionCount > 1 && this.currentTrigger < 2)
                    {
                        this.LinkRenderer.SetPosition(this.LinkRenderer.positionCount - 1, step.Position);
                        this.LinkRenderer.positionCount++;
                        this.LinkRenderer.SetPosition(this.LinkRenderer.positionCount - 1, step.Position);
                    }
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
            this.timer = this.endDelayHide;
            if (this.currentTrigger == 2)
            {
                this.GameManager.StartEndTransition();
            }
        }

        if (this.currentTrigger < 2)
        {
            this.LinkRenderer.SetPosition(this.LinkRenderer.positionCount - 1, this.transform.position);
        }
    }
}
