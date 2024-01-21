using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelMenu : MonoBehaviour
{
    public void OpenLevel(int levelId)
    {
        SceneManager.LoadScene(levelId);

        if (Time.timeScale == 0) // unpquse the gqme when chamging levels
        {
            Time.timeScale = 1;
        }
    }
}
