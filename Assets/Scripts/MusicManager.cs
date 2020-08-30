using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicManager : MonoBehaviour
{
    private static MusicManager instance = null;
    private bool isPaused = false;
    
    // Start is called before the first frame update
    void Start()
    {
        if (instance != null) {
            Destroy(this);
            return;
        }

        instance = this;
        DontDestroyOnLoad(this);
        GetComponent<AudioSource>().Play();
    }

    // Update is called once per frame
    void Update()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        if (currentScene.Equals("GameLobby") || currentScene.Equals("StartMenu2")) {
            if (isPaused) {
                GetComponent<AudioSource>().UnPause();
                isPaused = false;
            }
            return;
        }

        if (isPaused) return;
        GetComponent<AudioSource>().Pause();
        isPaused = true;
    }
}
