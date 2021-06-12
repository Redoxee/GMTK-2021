using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutoScript : MonoBehaviour
{
    public Character character = null;

    public SpriteRenderer tutoImage = null;

    public Sprite fullTutoSprite = null;

    public Transform[] transformsToEnable = null;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        this.transform.GetComponent<Collider2D>().enabled = false;
        this.transform.GetComponent<SpriteRenderer>().enabled = false;

        this.character.InhibShot = false;
        this.tutoImage.sprite = this.fullTutoSprite;

        foreach (var transform in this.transformsToEnable)
        {
            transform.gameObject.SetActive(true);
        }
    }
}
