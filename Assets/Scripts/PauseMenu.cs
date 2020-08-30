using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    public GameObject pauseMenuPanel;
    public GameObject popQuizPanel;
    public GameObject gameManager;
    public static bool isPaused = false;

    // Start is called before the first frame update
    void Start()
    {
        // gameManager = GameObject.Find("GameManager");
    }
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && popQuizPanel.activeSelf == false)
        {
            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    public void ResumeGame()
    {
        pauseMenuPanel.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
        AudioListener.pause = false;
    }

    void PauseGame()
    {
        pauseMenuPanel.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;
        AudioListener.pause = true;
    }

    public void LoadMainMenu()
    {
        ResumeGame();
        GameManager.lives = 1;
        gameManager.GetComponent<GameManager>().StartDeath();
    }

    public void ExitGame()
    {
        Debug.Log("Hope you had fun!");
        Application.Quit();
    }
}
