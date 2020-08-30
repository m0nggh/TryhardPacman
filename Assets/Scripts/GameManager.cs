using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;

public class GameManager : MonoBehaviourPunCallbacks
{
    public int width = 28;
    public int height = 31;

    // for background music
    [Header("Audio")]
    public AudioClip normalMusic;
    public AudioClip powerMusic;
    public AudioClip deathMusic;
    public AudioClip consumedMusic;
    public AudioClip popQuizMusic;
    public AudioSource backgroundMusic;
    public static bool isInitialLevel = true; // by default true for first level

    // for game effects
    private bool isDeathStarted; // for death animation
    private bool isGhostConsumed; // for ghost consuming animation
    private bool shouldBlink;
    public float blinkInterval = 0.1f;
    public float blinkTime = 0;

    // text/graphics for the board
    [Header("Text/Graphics")]
    public Text playerText;
    public Text readyText;
    public Text highscoreText;
    public Text playerUp;
    public Text playerScoreText;
    public Text livesLeft;
    public Text ghostpointsText;
    public Sprite whiteMaze;
    public Sprite blueMaze;
    public Sprite redMaze;
    public Sprite greenMaze;
    public Sprite defaultMaze;
    public GameObject[] tilemaps;
    public bool isTilemap;
    public Image currentReverse;
    public Image currentFrozen;
    public Image currentSpedup;
    public Text reverseLeft;
    public Text frozenLeft;
    public Text spedupLeft;

    // for score count and keeping track of the game
    [Header("Game updates")]
    public int totalPellets = 234; // default pacman level
    public int pelletsConsumed;
    public int score = 0;
    public static int playerScore = 0;
    public static int lives = 3;
    public bool didIncreaseLevel;
    public static int playerLevel = 1;
    public int difficultyLevel = 3; // 1 - easy, 2 - medium, 3 - difficult
    public static int ghostConsumedScore;

    // pop quiz stuff
    [Header("Pop Quiz")]
    public GameObject popQuizPanel;
    public GameObject everythingElse;

    // for bonus item (flame)
    [Header("Bonus Item")]
    public float randomBonusSpawnTime;
    public float currentBonusTime;
    public bool bonusNotSpawned = true; // by default
    public int bonusPelletCount;
    public Tile bonusItemChosen;
    public Tile[] bonusItems;

    // for powerup
    [Header("Powerups")]
    public float randomPowerupSpawnTime;
    public float currentPowerupTime;
    public bool powerupNotSpawned = true; // by default
    public int powerupPelletCount;
    public Tile powerupChosen;
    public Tile[] powerups;
    // by default, two powerups each at the start
    public static int reverseAmount = 2;
    public static int frozenAmount = 2;
    public static int spedupAmount = 2;

    // reference to pacman and the ghosts (oop will be questioned...)
    private GameObject pacman;
    private GameObject[] ghosts;

    // public GameObject[,] board = new GameObject[width, height];
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
            else
            {
                Debug.Log("Pacman is fighting at: " + currPos);
            }
        }
        pacman = GameObject.FindGameObjectWithTag("PacManWarrior");
        ghosts = GameObject.FindGameObjectsWithTag("GhostDemon");
        bonusPelletCount = UnityEngine.Random.Range(50, totalPellets); // set the bonus pellet count for spawning
        powerupPelletCount = UnityEngine.Random.Range(0, totalPellets); // set the powerup pellet count for spawning

        // set difficulty level
        difficultyLevel = PlayerPrefs.GetInt("DifficultyLevel");

        // only play background music the first time game is started
        if (isInitialLevel)
        {
            backgroundMusic.Play();
            isInitialLevel = false;
        }

        // initialise difficulty level from the start
        if (difficultyLevel == 1)
        {
            playerLevel = 1;
        }
        else if (difficultyLevel == 2)
        {
            playerLevel = 3;
        }
        else if (difficultyLevel == 3)
        {
            playerLevel = 5;
        }

        // initialise the high score at the start
        highscoreText.text = PlayerPrefs.GetInt("HighScore", 50000).ToString(); // default - 50k

        // initialise the background map
        String map = PlayerPrefs.GetString("MazeSelected");
        if (map.Equals("redmaze"))
        {
            defaultMaze = redMaze;
            if (isTilemap)
            {
                tilemaps[1].GetComponent<Renderer>().enabled = true; // set red only
                tilemaps[0].GetComponent<Renderer>().enabled = false;
                tilemaps[2].GetComponent<Renderer>().enabled = false;
            }
            else
            {
                GameObject maze = GameObject.FindGameObjectWithTag("BoardLayout");
                maze.GetComponent<SpriteRenderer>().sprite = defaultMaze;
            }
        }
        else if (map.Equals("greenmaze"))
        {
            defaultMaze = greenMaze;
            if (isTilemap)
            {
                tilemaps[2].GetComponent<Renderer>().enabled = true; // set green only
                tilemaps[0].GetComponent<Renderer>().enabled = false;
                tilemaps[1].GetComponent<Renderer>().enabled = false;
            }
            else
            {
                GameObject maze = GameObject.FindGameObjectWithTag("BoardLayout");
                maze.GetComponent<SpriteRenderer>().sprite = defaultMaze;
            }
        }
        else
        {
            defaultMaze = blueMaze;
        }

        StartGame();
    }

    // main driver function to set the animation for the "player ready" text on the background map
    public void StartGame()
    {
        StartCoroutine(StartBlinking(playerUp)); // blink the score
        // hide all ghosts and pacman, not allowing them to move first
        foreach (GameObject ghost in ghosts)
        {
            ghost.GetComponent<SpriteRenderer>().enabled = false;
            ghost.GetComponent<Ghost>().canMove = false;
        }
        pacman.GetComponent<SpriteRenderer>().enabled = false;
        pacman.GetComponent<PacMan>().canMove = false;

        StartCoroutine(ShowGameObjects(2.25f));
    }

    IEnumerator StartBlinking(Text blinkText)
    {
        yield return new WaitForSeconds(0.25f);
        blinkText.GetComponent<Text>().enabled = !blinkText.GetComponent<Text>().enabled;
        StartCoroutine(StartBlinking(blinkText));
    }

    IEnumerator ShowGameObjects(float delay)
    {
        yield return new WaitForSeconds(delay);
        // reveal the pacman and the ghosts first but don't allow them to move yet
        foreach (GameObject ghost in ghosts)
        {
            ghost.GetComponent<SpriteRenderer>().enabled = true;
        }
        pacman.GetComponent<SpriteRenderer>().enabled = true;
        // hide the player text now
        playerText.GetComponent<Text>().enabled = false;

        StartCoroutine(ReallyStartGame(1.81f));
    }

    IEnumerator ReallyStartGame(float delay)
    {
        yield return new WaitForSeconds(delay);
        // allow pacman and the ghosts to now move after delays
        foreach (GameObject ghost in ghosts)
        {
            ghost.GetComponent<Ghost>().canMove = true;
        }
        pacman.GetComponent<PacMan>().canMove = true;
        readyText.GetComponent<Text>().enabled = false;

        // start the actual audio after intro music
        backgroundMusic.clip = normalMusic;
        backgroundMusic.Play();
    }

    // main driver function to start ghost consuming animation for pacman
    public void StartConsume(Ghost consumedGhost)
    {
        if (!isGhostConsumed)
        {
            isGhostConsumed = true;
            foreach (GameObject ghost in ghosts)
            {
                ghost.GetComponent<Ghost>().canMove = false; // stop all ghost movement
            }
            consumedGhost.GetComponent<SpriteRenderer>().enabled = false; // hide the consumed ghost
            pacman.GetComponent<PacMan>().canMove = false; // stop pacman movement
            pacman.GetComponent<SpriteRenderer>().enabled = false; // hide pacman
            backgroundMusic.Stop(); // stop playing background music

            // to find position of text
            Vector2 consumedPos = consumedGhost.transform.position;
            // transform position from world space into viewport space
            Vector2 viewPortPoint = Camera.main.WorldToViewportPoint(consumedPos);
            // for getting the coordinates of the ghost points text
            ghostpointsText.GetComponent<RectTransform>().anchorMin = viewPortPoint; // 0-1 in x
            ghostpointsText.GetComponent<RectTransform>().anchorMax = viewPortPoint; // 0-1 in y

            ghostpointsText.text = ghostConsumedScore.ToString(); // update ghost consumed score accordingly
            ghostpointsText.GetComponent<Text>().enabled = true;

            backgroundMusic.PlayOneShot(consumedMusic); // play consumed music
            // queue for next event to play
            StartCoroutine(ProcessConsumedAfter(0.75f, consumedGhost));
        }
    }

    IEnumerator ProcessConsumedAfter(float delay, Ghost consumedGhost)
    {
        yield return new WaitForSeconds(delay);

        ghostpointsText.GetComponent<Text>().enabled = false; // hide the score
        consumedGhost.GetComponent<SpriteRenderer>().enabled = true; // un-hide ghost
        pacman.GetComponent<SpriteRenderer>().enabled = true; // un-hide pacman
        pacman.GetComponent<PacMan>().canMove = true; // unfreeze pacman
        foreach (GameObject ghost in ghosts)
        {
            ghost.GetComponent<Ghost>().canMove = true; // unfreeze the ghosts
        }
        backgroundMusic.Play(); // continue background music
        isGhostConsumed = false; // allow for more ghosts to be consumed
    }

    // main driver function to start to run the bonus point consumption animation
    public void StartConsumeBonusItem()
    {
        Vector2 consumedPos = bonusItemChosen.transform.position;
        Vector2 viewPortPoint = Camera.main.WorldToViewportPoint(consumedPos);

        // reusing the ghostpoints text
        ghostpointsText.GetComponent<RectTransform>().anchorMin = viewPortPoint; // 0-1 in x
        ghostpointsText.GetComponent<RectTransform>().anchorMax = viewPortPoint; // 0-1 in y

        ghostpointsText.text = bonusItemChosen.bonusPoints.ToString(); // update bonus item score accordingly
        ghostpointsText.GetComponent<Text>().enabled = true;
        bonusItemChosen.GetComponent<SpriteRenderer>().enabled = false;

        StartCoroutine(ProcessBonusItem(0.5f));
    }

    IEnumerator ProcessBonusItem(float delay)
    {
        yield return new WaitForSeconds(delay);
        ghostpointsText.GetComponent<Text>().enabled = false;
    }

    // main driver function to freeze all the ghosts on the spot
    public void StartFreezing()
    {
        foreach (GameObject ghost in ghosts)
        {
            ghost.GetComponent<Ghost>().canMove = false; // freeze the ghosts
        }
        StartCoroutine(ProcessFrozenTime(5.0f));
    }

    IEnumerator ProcessFrozenTime(float delay)
    {
        yield return new WaitForSeconds(delay);
        foreach (GameObject ghost in ghosts)
        {
            ghost.GetComponent<Ghost>().canMove = true; // unfreeze the ghosts
        }
    }

    // main driver function to start to run the whole death animation for pacman
    public void StartDeath()
    {
        if (!isDeathStarted)
        {
            isDeathStarted = true;
            foreach (GameObject ghost in ghosts)
            {
                ghost.GetComponent<Ghost>().canMove = false; // freeze all the ghosts
            }

            pacman.GetComponent<PacMan>().canMove = false; // freeze the pacman as well
            pacman.GetComponent<Animator>().enabled = false; // freeze the animation
            backgroundMusic.Stop();

            // starts this process with a 2 seconds delay
            StartCoroutine(ProcessUponDeath(2.0f));
        }
    }

    // create parallel processes to help to entire the death process is smooth
    IEnumerator ProcessUponDeath(float delay)
    {
        // this activates the delay before anything else is executed
        yield return new WaitForSeconds(delay);
        foreach (GameObject ghost in ghosts)
        {
            ghost.GetComponent<SpriteRenderer>().enabled = false; // make the ghosts disappear out of screen
        }

        // starts next process with a 2.4 seconds delay since that is the duration of death music
        StartCoroutine(ProcessDeathAnimation(2.4f));
    }

    IEnumerator ProcessDeathAnimation(float delay)
    {
        // set the correct orientation for pacman
        pacman.transform.localScale = new Vector3(1.5f, 1.5f, 1);
        pacman.transform.localRotation = Quaternion.Euler(0, 0, 0);

        // set pacman animation controller to death animation
        pacman.transform.GetComponent<Animator>().runtimeAnimatorController = pacman.GetComponent<PacMan>().animateDeath;
        pacman.transform.GetComponent<Animator>().enabled = true; // need to set to true to activate the controller

        // play the audio clip for death
        backgroundMusic.clip = deathMusic;
        backgroundMusic.Play();

        // activate the delay for the music to play finish
        yield return new WaitForSeconds(delay);

        pacman.GetComponent<SpriteRenderer>().enabled = false; // now hide the pacman after the animation
        backgroundMusic.Stop(); // stop the music and end it

        if (lives - 1 == 0)
        {
            StartCoroutine(ProcessRestart(2.0f));
            yield break;
        }

        // start the pop quiz section
        popQuizPanel.SetActive(true);
        everythingElse.SetActive(false);

        // play the music
        backgroundMusic.clip = popQuizMusic;
        backgroundMusic.Play();
    }

    public void ProcessRestartWrapper() => StartCoroutine(ProcessRestart(0));

    public IEnumerator ProcessRestart(float delay)
    {
        // disable pop quiz section
        popQuizPanel.SetActive(false);
        everythingElse.SetActive(true);
        backgroundMusic.Stop();

        // before restarting pacman and ghosts, do a check on the number of lives left
        lives--; // minus one life from pacman
        if (lives == 0)
        {
            playerText.GetComponent<Text>().enabled = true;
            readyText.GetComponent<Text>().text = "GAME OVER"; // can change color also
            readyText.GetComponent<Text>().enabled = true;
            Debug.Log("Ending game...");
            StartCoroutine(ProcessGameover(1.5f));
        }
        else
        {
            yield return new WaitForSeconds(delay);
            Restart(); // then finally restart the game after the delay
        }
    }

    IEnumerator ProcessGameover(float delay)
    {
        yield return new WaitForSeconds(delay);
        PlayerPrefs.SetInt("HighScore", Int32.Parse(highscoreText.text)); // updates the local highscore accordingly
        PlayerPrefs.SetInt("PlayerScore", Int32.Parse(playerScoreText.text)); // stores current score for point conversion later
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

        Debug.Log("Time played: " + Time.time + " seconds");
        PlayerPrefs.DeleteKey("PacmanSelected");
        PlayerPrefs.DeleteKey("MazeSelected");

        SceneManager.LoadScene("StartMenu2");
    }

    public void Restart()
    {
        Debug.Log("Test");
        pacman.GetComponent<PacMan>().Restart(); // for pacman
        foreach (GameObject ghost in ghosts)
        {
            ghost.GetComponent<Ghost>().Restart(); // for ghosts
        }
        isDeathStarted = false; // so pacman can die again sadly
        // play the background music again after resetting
        backgroundMusic.clip = normalMusic;
        backgroundMusic.Play();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateGraphics(); // updating score, lives and powerups
        TrackPelletsConsumed(); // tracking the pellet count
        CheckBlink(); // checks when to blink upon victory
        SpawnBonusItem(); // checks when to spawn bonus item
        CheckBonusItem(); // checks when to disable bonus item
        SpawnPowerup(); // checks when to spawn powerup
        CheckPowerup(); // checks when to disable powerup
        UpdateHighScore(); // checks and updates highscore
    }

    void UpdateGraphics()
    {
        playerScoreText.text = playerScore.ToString(); // to make it visible and updated
        livesLeft.text = "x" + lives.ToString();

        // update the powerups
        reverseLeft.text = "x" + reverseAmount.ToString();
        frozenLeft.text = "x" + frozenAmount.ToString();
        spedupLeft.text = "x" + spedupAmount.ToString();
    }

    // for winning the game
    void TrackPelletsConsumed()
    {
        // Debug.Log("Pellets consumed: " + pelletsConsumed);
        if (totalPellets == pelletsConsumed)
        {
            Debug.Log("Player has won!");
            PlayerVictory();
        }
        // Debug.Log("Pellet count: " + pelletsConsumed);
    }

    // main driver method to call for game victory
    void PlayerVictory()
    {
        if (!didIncreaseLevel)
        {
            playerLevel++; // ensure the only one level is incremented
            didIncreaseLevel = true;
        }
        StartCoroutine(ProcessVictory(2.0f));
    }

    IEnumerator ProcessVictory(float delay)
    {
        pacman.GetComponent<PacMan>().canMove = false; // freeze pacman
        pacman.GetComponent<Animator>().enabled = false; // stop pacman animation
        foreach (GameObject ghost in ghosts)
        {
            ghost.GetComponent<Ghost>().canMove = false; // freeze ghosts
            ghost.GetComponent<Animator>().enabled = false; // stop ghosts animation
        }
        backgroundMusic.Stop(); // stop the music
        yield return new WaitForSeconds(delay);
        StartCoroutine(BlinkGameboard(2.0f));
    }

    IEnumerator BlinkGameboard(float delay)
    {
        pacman.GetComponent<SpriteRenderer>().enabled = false; // hide pacman
        foreach (GameObject ghost in ghosts)
        {
            ghost.GetComponent<SpriteRenderer>().enabled = false; // hide ghosts
        }
        // hide powerups and bonus points if not taken before the end
        powerupChosen.GetComponent<SpriteRenderer>().enabled = false;
        bonusItemChosen.GetComponent<SpriteRenderer>().enabled = false;

        shouldBlink = true;
        yield return new WaitForSeconds(delay); // to blink the board
        // restart the game with the next level
        shouldBlink = false;

        // begin the pop quiz
        popQuizPanel.SetActive(true);
        everythingElse.SetActive(false);

        // begin the music
        backgroundMusic.clip = popQuizMusic;
        backgroundMusic.Play();
    }

    public void StartNextLevel()
    {
        // disable pop quiz section
        popQuizPanel.SetActive(false);
        everythingElse.SetActive(true);
        backgroundMusic.Stop();

        int nextLevel = Int32.Parse(SceneManager.GetActiveScene().name.Substring(5)) + 1;
        if (nextLevel == 6)
        {
            PhotonNetwork.LoadLevel("Level1");
        }
        else
        {
            PhotonNetwork.LoadLevel("Level" + nextLevel);
        }
    }

    void CheckBlink()
    {
        if (shouldBlink)
        {
            if (blinkTime < blinkInterval)
            {
                blinkTime += Time.deltaTime; // continue incrementing time
            }
            else
            {
                blinkTime = 0f; // reset blink time

                if (isTilemap)
                {
                    GameObject whiteGrid = GameObject.FindGameObjectWithTag("WhiteGridboard");
                    if (whiteGrid.GetComponent<Renderer>().enabled == false)
                    {
                        whiteGrid.GetComponent<Renderer>().enabled = true;
                    }
                    else
                    {
                        whiteGrid.GetComponent<Renderer>().enabled = false;
                    }
                }
                else
                {
                    GameObject maze = GameObject.FindGameObjectWithTag("BoardLayout");
                    if (maze.GetComponent<SpriteRenderer>().sprite == defaultMaze)
                    {
                        maze.GetComponent<SpriteRenderer>().sprite = whiteMaze;
                    }
                    else
                    {
                        maze.GetComponent<SpriteRenderer>().sprite = defaultMaze;
                    }
                }
            }
        }
    }

    // checks when to spawn the powerup
    void SpawnPowerup()
    {
        if (pelletsConsumed >= powerupPelletCount && powerupNotSpawned)
        {
            powerupNotSpawned = false;
            randomPowerupSpawnTime = UnityEngine.Random.Range(8, 12);
            powerupChosen = powerups[UnityEngine.Random.Range(0, powerups.Length)];
            powerupChosen.GetComponent<SpriteRenderer>().enabled = true;
            powerupChosen.isActivated = true;
        }
    }

    // records the duration of the powerup and disable it when it runs out
    void CheckPowerup()
    {
        if (powerupChosen.isActivated && currentPowerupTime < randomPowerupSpawnTime)
        {
            currentPowerupTime += Time.deltaTime;
        }
        else
        {
            powerupChosen.GetComponent<SpriteRenderer>().enabled = false;
            powerupChosen.isActivated = false;
        }
    }

    // checks when to spawn the bonus item
    void SpawnBonusItem()
    {
        if (pelletsConsumed >= bonusPelletCount && bonusNotSpawned)
        {
            bonusNotSpawned = false;
            randomBonusSpawnTime = UnityEngine.Random.Range(5, 10);
            bonusItemChosen = bonusItems[UnityEngine.Random.Range(0, bonusItems.Length)];
            bonusItemChosen.GetComponent<SpriteRenderer>().enabled = true;
            bonusItemChosen.isActivated = true;
        }
    }

    // records the duration of the bonus item and disable it when it runs out
    void CheckBonusItem()
    {
        if (bonusItemChosen.isActivated && currentBonusTime < randomBonusSpawnTime)
        {
            currentBonusTime += Time.deltaTime;
        }
        else
        {
            bonusItemChosen.GetComponent<SpriteRenderer>().enabled = false;
            bonusItemChosen.isActivated = false;
        }
    }

    // checks and updates the highscore
    void UpdateHighScore()
    {
        if (playerScore > Int32.Parse(highscoreText.text))
        {
            highscoreText.text = playerScore.ToString(); // constantly update the highscore
        }
    }

    void OnApplicationQuit()
    {
        Debug.Log("Time played: " + Time.time + " seconds");
        PlayerPrefs.DeleteKey("PacmanSelected");
        PlayerPrefs.DeleteKey("MazeSelected");
    }
}
