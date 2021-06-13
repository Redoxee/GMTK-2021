using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [SerializeField]
    AudioSource[] Bumps = null;

    [SerializeField]
    AudioSource[] Victories = null;

    [SerializeField]
    AudioSource[] Collectibles = null;

    [SerializeField]
    AudioSource[] Jumps = null;

    void Start()
    {
        AudioManager.Instance = this;
    }

    public void Bump()
    {
        int randomIndex = Random.Range(0, this.Bumps.Length);
        this.Bumps[randomIndex].Play();
    }

    public void Victory()
    {
        int randomIndex = Random.Range(0, this.Victories.Length);
        this.Victories[randomIndex].Play();
    }

    public void Collectible()
    {
        int randomIndex = Random.Range(0, this.Collectibles.Length);
        this.Collectibles[randomIndex].Play();
    }

    public void Jump()
    {
        int randomIndex = Random.Range(0, this.Jumps.Length);
        this.Jumps[randomIndex].Play();
    }
}
