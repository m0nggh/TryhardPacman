using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;

public class MultiGameManager : MonoBehaviourPunCallbacks
{
    public int width = 41;
    public int height = 39;

    [Header("Audio")]
    public AudioClip normalMusic;
    public AudioClip powerMusic;
    public AudioClip deathMusic;
    public AudioClip consumedMusic;
    public AudioSource backgroundMusic;

    [Header("Game Effects")]
    // for game effects
    private bool isDeathStarted; // for death animation
    private bool isGhostConsumed; // for ghost consuming animation
    private bool shouldBlink;
    public float blinkInterval = 0.1f;
    public float blinkTime = 0;

    [Header("Text/Graphics/UI")]
    public Text playerText;
    public Text readyText;
    public GameObject gameOverText;
    public GameObject leaveButton;
    public GameObject endGameCanvas;

    [Header("References")]
    public static int totalPlayers;
    public bool hasLost = false;
    public GameObject everythingElse;
    public GameObject pacman;
    public Vector3[] positions; 

    private float currentTime = 0;
    private float savedTime = 0;
    private float currentGunTime = 0;
    private float savedGunTime = 0;
    private float currentShieldTime = 0;
    private float savedShieldTime = 0;
    private float currentTeleportTime = 0;
    private float savedTeleportTime = 0;
    private float currentSpeedTime = 0;
    private float savedSpeedTime = 0;
    private float currentLifeTime = 0;
    private float savedLifeTime = 0;
    private GameObject currentPower = null;
    private GameObject currentGun = null;
    private GameObject currentShield = null;
    private GameObject currentTeleport = null;
    private GameObject currentSpeed = null;
    private GameObject currentLife = null;
    private GameObject[] powerList = new GameObject[6];
    public GameObject[,] board;
    // Start is called before the first frame update
    void Start()
    {
        PhotonNetwork.AutomaticallySyncScene = true;

        board = new GameObject[width, height];
        UnityEngine.Object[] objects = GameObject.FindObjectsOfType<GameObject>(); // find all objects 
        foreach (GameObject obj in objects)
        {
            Vector2 currPos = obj.transform.position; // global world location (because their local position is set 
            // according to the empty game object folder storing it)

            if (obj.tag == "Pellet" || obj.tag == "Vertex" || obj.tag == "Powerup")
            {
                board[(int)currPos.x, (int)currPos.y] = obj;
            }
        }

        pacman = GameObject.FindGameObjectWithTag("PacManWarrior");
        totalPlayers = PlayerPrefs.GetInt("TotalPlayers");
        Debug.Log("There are now " + totalPlayers + " players");

        backgroundMusic.Play();
        
        StartGame();
    }

    // main driver function to set the animation for the "player ready" text on the background map
    public void StartGame()
    {
        // StartCoroutine(StartBlinking(playerUp)); // blink the score
        pacman.GetComponent<SpriteRenderer>().enabled = false;
        pacman.GetComponent<MultiPacman>().canMove = false;
        StartCoroutine(ShowGameObjects(2.25f));
    }

    // IEnumerator StartBlinking(Text blinkText)
    // {
    //     yield return new WaitForSeconds(0.25f);
    //     blinkText.GetComponent<Text>().enabled = !blinkText.GetComponent<Text>().enabled;
    //     StartCoroutine(StartBlinking(blinkText));
    // }

    IEnumerator ShowGameObjects(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        pacman.GetComponent<SpriteRenderer>().enabled = true;
        // hide the player text now
        playerText.GetComponent<Text>().enabled = false;

        StartCoroutine(ReallyStartGame(1.81f));
    }

    IEnumerator ReallyStartGame(float delay)
    {
        yield return new WaitForSeconds(delay);

        pacman.GetComponent<MultiPacman>().canMove = true;
        readyText.GetComponent<Text>().enabled = false;

        // start the actual audio after intro music
        backgroundMusic.clip = normalMusic;
        backgroundMusic.Play();
    }

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
        SceneManager.LoadScene("StartMenu2");
    }

    // Update is called once per frame
    void Update()
    {
        if (totalPlayers == 1) {
            Debug.Log("Enter the endgame");
            EndGame();
            return;
        }
        
        if (!PhotonNetwork.IsMasterClient) {
            return;
        }

        currentTime += Time.deltaTime;
        if (currentTime >= savedTime + 10f) {
            
            savedTime = currentTime;

            if (currentPower != null) {
                PhotonNetwork.Destroy(currentPower.GetPhotonView());
            }

            GameObject newPower = PhotonNetwork.Instantiate("PowerPellet", 
            positions[UnityEngine.Random.Range(0, positions.Length)], 
            Quaternion.identity);

            currentPower = newPower;
            powerList[0] = currentPower;
        }

        currentSpeedTime += Time.deltaTime;
        if (currentSpeedTime >= savedSpeedTime + 10f) {
            
            savedSpeedTime = currentSpeedTime;

            if (currentSpeed != null) {
                PhotonNetwork.Destroy(currentSpeed.GetPhotonView());
            }

            GameObject newSpeed = PhotonNetwork.Instantiate("SpeedUp", 
            positions[UnityEngine.Random.Range(0, positions.Length)], 
            Quaternion.identity);

            currentSpeed = newSpeed;
            powerList[1] = currentSpeed;
        }

        currentGunTime += Time.deltaTime;
        if (currentGunTime >= savedGunTime + 15f) {
            
            savedGunTime = currentGunTime;

            if (currentGun != null) {
                PhotonNetwork.Destroy(currentGun.GetPhotonView());
            }

            GameObject newGun = PhotonNetwork.Instantiate("Gun", 
            positions[UnityEngine.Random.Range(0, positions.Length)], 
            Quaternion.identity);

            currentGun = newGun;
            powerList[2] = currentGun;
        }

        currentShieldTime += Time.deltaTime;
        if (currentShieldTime >= savedShieldTime + 12f) {
            
            savedShieldTime = currentShieldTime;

            if (currentShield != null) {
                PhotonNetwork.Destroy(currentShield.GetPhotonView());
            }

            GameObject newShield = PhotonNetwork.Instantiate("Shield", 
            positions[UnityEngine.Random.Range(0, positions.Length)], 
            Quaternion.identity);

            currentShield = newShield;
            powerList[3] = currentShield;
        }

        currentTeleportTime += Time.deltaTime;
        if (currentTeleportTime >= savedTeleportTime + 20f) {
            
            savedTeleportTime = currentTeleportTime;

            if (currentTeleport != null) {
                PhotonNetwork.Destroy(currentTeleport.GetPhotonView());
            }

            GameObject newTeleport = PhotonNetwork.Instantiate("Teleport", 
            positions[UnityEngine.Random.Range(0, positions.Length)], 
            Quaternion.identity);

            currentTeleport = newTeleport;
            powerList[4] = currentTeleport;
        }

        currentLifeTime += Time.deltaTime;
        if (currentLifeTime >= savedLifeTime + 30f) {
            
            savedLifeTime = currentLifeTime;

            if (currentLife != null) {
                PhotonNetwork.Destroy(currentLife.GetPhotonView());
            }

            GameObject newLife = PhotonNetwork.Instantiate("ExtraLife", 
            positions[UnityEngine.Random.Range(0, positions.Length)], 
            Quaternion.identity);

            currentLife = newLife;
            powerList[5] = currentLife;
        }
    }

    // main driver function to start to run the whole death animation for pacman
    public void StartDeath()
    {
        if (!isDeathStarted)
        {
            isDeathStarted = true;

            pacman.GetComponent<MultiPacman>().canMove = false; // freeze the pacman as well
            pacman.GetComponent<Animator>().enabled = false; // freeze the animation
            backgroundMusic.Stop();

            // starts this process with a 2 seconds delay
            StartCoroutine(ProcessDeathAnimation(2.0f));
        }
    }

    IEnumerator ProcessDeathAnimation(float delay)
    {
        // set the correct orientation for pacman
        pacman.transform.localScale = new Vector3(1.5f, 1.5f, 1);
        pacman.transform.localRotation = Quaternion.Euler(0, 0, 0);

        // set pacman animation controller to death animation
        pacman.transform.GetComponent<Animator>().runtimeAnimatorController = pacman.GetComponent<MultiPacman>().animateDeath;
        pacman.transform.GetComponent<Animator>().enabled = true; // need to set to true to activate the controller

        // play the audio clip for death
        backgroundMusic.clip = deathMusic;
        backgroundMusic.Play();

        // activate the delay for the music to play finish
        yield return new WaitForSeconds(delay);

        pacman.GetComponent<SpriteRenderer>().enabled = false; // now hide the pacman after the animation
        backgroundMusic.Stop(); // stop the music and end it

    }

    public IEnumerator ProcessRestart(float delay)
    {
        /* if (lives == 0)
        {
            playerText.GetComponent<Text>().enabled = true;
            readyText.GetComponent<Text>().text = "GAME OVER"; // can change color also
            readyText.GetComponent<Text>().enabled = true;
            Debug.Log("Ending game...");
            StartCoroutine(ProcessGameover(1.5f));
        } */

        yield return new WaitForSeconds(delay);
        Restart(); // then finally restart the game after the delay
    }

    /* IEnumerator ProcessGameover(float delay)
    {
        yield return new WaitForSeconds(delay);
        PlayerPrefs.SetInt("HighScore", Int32.Parse(highscoreText.text)); // updates the local highscore accordingly
        Debug.Log("Initiating Game over...");
        PhotonNetwork.LeaveRoom();
        // reset the game attributes
        lives = 3;
        playerScore = 0;
        reverseAmount = 2;
        frozenAmount = 2;
        spedupAmount = 2;
        isInitialLevel = true;
        playerLevel = 1; // easy by default

        SceneManager.LoadScene("StartMenu2");
    } */

    public void Restart()
    {
        Debug.Log("Test");
        pacman.GetComponent<MultiPacman>().Restart(); // for pacman
        isDeathStarted = false; // so pacman can die again sadly
        // play the background music again after resetting
        backgroundMusic.clip = normalMusic;
        backgroundMusic.Play();
    }

    void EndGame() {
        Debug.Log("We are in the endgame now");
        if (hasLost) return;

        for (int i = 0; i < powerList.Length; i++) {
            if (powerList[i] == null) continue;
            PhotonNetwork.Destroy(powerList[i].GetPhotonView());
        }

        everythingElse.SetActive(false);
        endGameCanvas.SetActive(true);
    }
}
