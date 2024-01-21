using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerLogic : MonoBehaviour
{
    private int cherries = 0;
    private Array cherryList;

    [SerializeField] private Text cherriesText;

    [SerializeField] private AudioSource collectionSoundEffect;
    
    private void Start()
    {
        cherryList = GameObject.FindGameObjectsWithTag("Cherry");
        cherriesText.text = "0/" + cherryList.Length;
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Cherry"))
        {
            collectionSoundEffect.Play();
            Destroy(collision.gameObject);
            cherries++;
            cherriesText.text = cherries + "/" + cherryList.Length;
        }
    }
}
