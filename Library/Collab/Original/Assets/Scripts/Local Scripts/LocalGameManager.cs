using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LocalGameManager : MonoBehaviour
{
    private static int width = 28;
    private static int height = 31;

    // for background music
    public AudioClip normalMusic;
    public AudioClip powerMusic;
    public AudioClip deathMusic;
    public AudioClip consumedMusic;
    public AudioSource backgroundMusic;

    // for game effects
    private bool isDeathStarted; // for death animation
    private bool isGhostConsumed; // for ghost consuming animation
    private bool shouldBlink;
    public float blinkInterval = 0.1f;
    public float blinkTime = 0;

    // text/graphics for the board
    public Text playerText;
    public Text readyText;
    public Text highscoreText;
    public Text playerUp;
    public Text playerScoreText;
    public Image playerLives2;
    public Image playerLives1;
    public Text ghostpointsText;
    public Sprite whiteMaze;
    public Sprite blueMaze;

    // for score count and keeping track of the game
    public static int totalPellets = 234;
    public int pelletsConsumed;
    public int score = 0;
    public static int playerScore = 0;
    public static int lives = 3;
    public static int playerLevel = 1;
    public static int ghostConsumedScore; // 200 by default

    // reference to pacman and the ghosts (oop will be questioned...)
    private GameObject pacman;
    private GameObject[] ghosts;

    public GameObject[,] board = new GameObject[width, height];
    // Start is called before the first frame update
    void Start()
    {
        Object[] objects = GameObject.FindObjectsOfType<GameObject>(); // find all objects 
        foreach (GameObject obj in objects)
        {
            Vector2 currPos = obj.transform.position; // global world location (because their local position is set 
            // according to the empty game object folder storing it)

            if (obj.tag != "PacManWarrior" && obj.tag != "GhostDemon" && obj.tag != "Scattered" && obj.tag != "BoardGraphics")
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
            ghost.GetComponent<LocalGhost>().canMove = false;
        }
        // ??? why not found ???
        pacman.GetComponent<SpriteRenderer>().enabled = false;
        pacman.GetComponent<LocalPacMan>().canMove = false;

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
            ghost.GetComponent<LocalGhost>().canMove = true;
        }
        pacman.GetComponent<LocalPacMan>().canMove = true;
        readyText.GetComponent<Text>().enabled = false;

        // start the actual audio after intro music
        backgroundMusic.clip = normalMusic;
        backgroundMusic.Play();
    }

    // main driver function to start ghost consuming animation for pacman
    public void StartConsume(LocalGhost consumedGhost)
    {
        if (!isGhostConsumed)
        {
            isGhostConsumed = true;
            foreach (GameObject ghost in ghosts)
            {
                ghost.GetComponent<LocalGhost>().canMove = false; // stop all ghost movement
            }
            consumedGhost.GetComponent<SpriteRenderer>().enabled = false; // hide the consumed ghost
            pacman.GetComponent<LocalPacMan>().canMove = false; // stop pacman movement
            pacman.GetComponent<SpriteRenderer>().enabled = false; // hide pacman
            backgroundMusic.Stop(); // stop playing background music

            // to find position of text
            Vector2 consumedPos = consumedGhost.transform.position;
            // transform position from world space into viewport space
            Vector2 viewPortPoint = Camera.main.WorldToViewportPoint(consumedPos);
            // for getting the coordinates of the ghost points text
            ghostpointsText.GetComponent<RectTransform>().anchorMin = viewPortPoint; // 0-1 in x
            ghostpointsText.GetComponent<RectTransform>().anchorMax = viewPortPoint; // 0-1 in y

            ghostpointsText.text = ghostConsumedScore.ToString(); // set the score based on the ghost consumed score
            ghostpointsText.GetComponent<Text>().enabled = true;

            backgroundMusic.PlayOneShot(consumedMusic); // play consumed music
            // queue for next event to play
            StartCoroutine(ProcessConsumedAfter(0.75f, consumedGhost));
        }
    }

    IEnumerator ProcessConsumedAfter(float delay, LocalGhost consumedGhost)
    {
        yield return new WaitForSeconds(delay);

        ghostpointsText.GetComponent<Text>().enabled = false; // hide the score
        consumedGhost.GetComponent<SpriteRenderer>().enabled = true; // un-hide ghost
        pacman.GetComponent<SpriteRenderer>().enabled = true; // un-hide pacman
        pacman.GetComponent<LocalPacMan>().canMove = true; // unfreeze pacman
        foreach (GameObject ghost in ghosts)
        {
            ghost.GetComponent<LocalGhost>().canMove = true; // unfreeze the ghosts
        }
        backgroundMusic.Play(); // continue background music
        isGhostConsumed = false; // allow for more ghosts to be consumed
    }

    // main driver function to start to run the whole death animation for pacman
    public void StartDeath()
    {
        if (!isDeathStarted)
        {
            isDeathStarted = true;
            foreach (GameObject ghost in ghosts)
            {
                ghost.GetComponent<LocalGhost>().canMove = false; // freeze all the ghosts
            }

            pacman.GetComponent<LocalPacMan>().canMove = false; // freeze the pacman as well
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
        pacman.transform.GetComponent<Animator>().runtimeAnimatorController = pacman.GetComponent<LocalPacMan>().animateDeath;
        pacman.transform.GetComponent<Animator>().enabled = true; // need to set to true to activate the controller

        // play the audio clip for death
        backgroundMusic.clip = deathMusic;
        backgroundMusic.Play();

        // activate the delay for the music to play finish
        yield return new WaitForSeconds(delay);

        // starts next process with 2s delay
        StartCoroutine(ProcessRestart(2.0f));
    }

    IEnumerator ProcessRestart(float delay)
    {
        // before restarting pacman and ghosts, do a check on the number of lives left
        lives--; // minus one life from pacman
        if (lives == 0)
        {
            playerText.GetComponent<Text>().enabled = true;
            pacman.GetComponent<SpriteRenderer>().enabled = false; // now hide the pacman after the animation
            readyText.GetComponent<Text>().text = "GAME OVER"; // can change color also
            readyText.GetComponent<Text>().enabled = true;
            backgroundMusic.Stop(); // stop the music and end it
            StartCoroutine(ProcessGameover(1.5f));
        }
        else
        {
            pacman.GetComponent<SpriteRenderer>().enabled = false; // now hide the pacman after the animation
            backgroundMusic.Stop(); // stop the music again
            yield return new WaitForSeconds(delay);
            Restart(); // then finally restart the game after the delay
        }
    }

    IEnumerator ProcessGameover(float delay)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene("StartMenu2");
    }

    public void Restart()
    {
        pacman.GetComponent<LocalPacMan>().Restart(); // for pacman
        foreach (GameObject ghost in ghosts)
        {
            ghost.GetComponent<LocalGhost>().Restart(); // for ghosts
        }
        isDeathStarted = false; // so pacman can die again sadly
        // play the background music again after resetting
        backgroundMusic.clip = normalMusic;
        backgroundMusic.Play();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateGraphics();
        TrackPelletsConsumed();
        CheckBlink();
    }

    void UpdateGraphics()
    {
        playerScoreText.text = playerScore.ToString(); // to make it visible and updated
        if (lives == 3)
        {
            playerLives2.enabled = true;
            playerLives1.enabled = true;
        }
        else if (lives == 2)
        {
            playerLives2.enabled = false;
            playerLives1.enabled = true;
        }
        else if (lives == 1)
        {
            playerLives2.enabled = false;
            playerLives1.enabled = false;
        }
    }

    // for winning the game
    void TrackPelletsConsumed()
    {
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
        playerLevel++; // increment the game level
        StartCoroutine(ProcessVictory(2.0f));
    }

    IEnumerator ProcessVictory(float delay)
    {
        pacman.GetComponent<LocalPacMan>().canMove = false; // freeze pacman
        pacman.GetComponent<Animator>().enabled = false; // stop pacman animation
        foreach (GameObject ghost in ghosts)
        {
            ghost.GetComponent<LocalGhost>().canMove = false; // freeze ghosts
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

        shouldBlink = true;
        yield return new WaitForSeconds(delay); // to blink the board
        // restart the game with the next level
        shouldBlink = false;
        StartNextLevel();
    }

    void StartNextLevel()
    {
        SceneManager.LoadScene("Level1Local");
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
                GameObject maze = GameObject.FindGameObjectWithTag("BoardLayout");
                if (maze.GetComponent<SpriteRenderer>().sprite == blueMaze)
                {
                    maze.GetComponent<SpriteRenderer>().sprite = whiteMaze;
                }
                else
                {
                    maze.GetComponent<SpriteRenderer>().sprite = blueMaze;
                }
            }
        }
    }
}
