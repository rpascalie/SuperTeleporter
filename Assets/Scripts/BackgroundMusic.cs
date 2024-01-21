using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BackgroundMusic : MonoBehaviour
{
    public AudioSource bgm; // link this with a bgm prefab, add tag "BGM", loop, don't play on awake
    private AudioSource spawned;
    

    void Awake()
    {
        GameObject currentBGM = GameObject.FindGameObjectWithTag("BGM1");
        if (currentBGM == null)
        {
            spawned = Instantiate(bgm);
            spawned.Play();
            DontDestroyOnLoad(spawned);
        }    
    }
}
