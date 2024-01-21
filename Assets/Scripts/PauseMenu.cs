using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private GameObject pauseScreen;    

    private void Update()
    {
        if (Input.GetButtonDown("Cancel"))
        {
            PauseGame();
        }
    }
    
    public void PauseGame()
    {
        if (!pauseScreen.activeInHierarchy)
        {
            pauseScreen.SetActive(true);
            Time.timeScale = 0;
        }
        else
        {
            pauseScreen.SetActive(false);
            Time.timeScale = 1;            
        }        
    }

    public void ReturnToStart()
    {
        SceneManager.LoadScene(0);
    }
}
