using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundMusic : MonoBehaviour
{
    public AudioSource bgm; // link this with a bgm prefab, add tag "BGM", loop, don't play on awake

    void Awake()
    {
        GameObject currentBGM = GameObject.FindGameObjectWithTag("BGM1");
        if (currentBGM == null)
        {
            AudioSource spawned = Instantiate(bgm);
            spawned.Play();
            DontDestroyOnLoad(spawned);
        }
    }
}
