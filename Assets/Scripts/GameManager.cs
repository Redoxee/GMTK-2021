using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public float EndAnimationDuration = 3;
    public AnimationCurve EndAnimationCurve;

    public LineRenderer LinkRenderer = null;
    public Transform[] KeysTransforms = new Transform[0];

    public Transform ConvergeTransform = null;

    private List<Vector3> recordedPositions = new List<Vector3>();

    private float timer;
    bool isAnimatingEnd = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void StartEndTransition()
    {
        this.recordedPositions.Clear();
        for (int index = 0; index < this.LinkRenderer.positionCount; ++index)
        {
            this.recordedPositions.Add(this.LinkRenderer.GetPosition(index));
        }

        for (int index = 0; index < this.KeysTransforms.Length; ++index)
        {
            this.recordedPositions.Add(this.KeysTransforms[index].position);
        }

        this.isAnimatingEnd = true;
        this.timer = 0;
    }
    
    void Update()
    {
        if (!this.isAnimatingEnd)
        {
            return;
        }

        this.timer = this.timer + Time.deltaTime;
        float factor = this.EndAnimationCurve.Evaluate(this.timer / this.EndAnimationDuration);
        int id = 0;
        Vector3 converge = this.ConvergeTransform.position * (1f - factor);

        for (int index = 0; index < this.LinkRenderer.positionCount; ++index)
        {
            this.LinkRenderer.SetPosition(index, this.recordedPositions[id++] * factor + converge);
        }

        for (int index = 0; index < this.KeysTransforms.Length; ++index)
        {
            this.KeysTransforms[index].position = this.recordedPositions[id++] * factor + converge;
        }

        if (this.timer > this.EndAnimationDuration)
        {
            MainScene.LoadNextScene();
        }
    }
}
