using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PopQuizManager : MonoBehaviour
{
    [Header("Timer")]
    public float timeLeft = 15;

    [Header("Game Manager")]
    [SerializeField] private GameObject gameManager = null;

    public int numQuestions = 3;

    private bool startCountdown = false;
    private System.Random rnd = new System.Random();

    [Header("UI")]
    [SerializeField] private Text timeDisplay = null;
    [SerializeField] private Text itemDisplay = null;
    [SerializeField] private GameObject popQuizPanel = null;
    [SerializeField] private GameObject wrongAnswerPanel = null;
    [SerializeField] private GameObject rightAnswerPanel = null;
    [SerializeField] private GameObject[] powerupImages = null;

    [Header("Topics")]
    [SerializeField] private GameObject geographyPanel = null;
    [SerializeField] private GameObject historyPanel = null;
    [SerializeField] private GameObject gamingPanel = null;
    [SerializeField] private GameObject moviePanel = null;
    [SerializeField] private GameObject sportsPanel = null;
    [SerializeField] private GameObject animalsPanel = null;
    [SerializeField] private GameObject sciencePanel = null;

    [Header("Topic Questions")]
    [SerializeField] private GameObject[] geographyQuestions = null;
    [SerializeField] private GameObject[] historyQuestions = null;
    [SerializeField] private GameObject[] gamingQuestions = null;
    [SerializeField] private GameObject[] movieQuestions = null;
    [SerializeField] private GameObject[] sportsQuestions = null;
    [SerializeField] private GameObject[] animalsQuestions = null;
    [SerializeField] private GameObject[] scienceQuestions = null;

    private GameObject currentQuestion;
    private GameObject currentTopic;

    void Update()
    {
        if (timeLeft <= 0)
        {
            startCountdown = false;
            timeDisplay.text = "" + 0;
            currentQuestion.SetActive(false);
            currentTopic.SetActive(false);
            AnyOtherOption();
        }

        if (startCountdown)
        {
            timeLeft -= Time.deltaTime;
            Debug.Log("" + timeLeft);
            if (timeLeft <= 5) { timeDisplay.color = Color.red; }
            timeDisplay.text = "" + timeLeft;
        }
    }

    // activated by pressing any topic button
    public void StartToCount()
    {
        timeDisplay.gameObject.SetActive(true);
        startCountdown = true;
    }

    public void PickRandomGeogQuestion()
    {
        int rng = rnd.Next(numQuestions);
        currentTopic = geographyPanel;
        currentQuestion = geographyQuestions[rng];
        currentQuestion.SetActive(true);
    }
    public void PickRandomHistQuestion()
    {
        int rng = rnd.Next(numQuestions);
        currentTopic = historyPanel;
        currentQuestion = historyQuestions[rng];
        currentQuestion.SetActive(true);
    }
    public void PickRandomGameQuestion()
    {
        Debug.Log("Test");
        int rng = rnd.Next(numQuestions);
        currentTopic = gamingPanel;
        currentQuestion = gamingQuestions[rng];
        currentQuestion.SetActive(true);
    }
    public void PickRandomMovieQuestion()
    {
        int rng = rnd.Next(numQuestions);
        currentTopic = moviePanel;
        currentQuestion = movieQuestions[rng];
        currentQuestion.SetActive(true);
    }

    public void PickRandomSportsQuestions()
    {
        int rng = rnd.Next(numQuestions);
        currentTopic = sportsPanel;
        currentQuestion = sportsQuestions[rng];
        currentQuestion.SetActive(true);
    }

    public void PickRandomAnimalsQuestions()
    {
        int rng = rnd.Next(numQuestions);
        currentTopic = animalsPanel;
        currentQuestion = animalsQuestions[rng];
        currentQuestion.SetActive(true);
    }

    public void PickRandomScienceQuestions()
    {
        int rng = rnd.Next(numQuestions);
        currentTopic = sciencePanel;
        currentQuestion = scienceQuestions[rng];
        currentQuestion.SetActive(true);
    }
    public void CorrectOptionPressed()
    {
        startCountdown = false;
        rightAnswerPanel.SetActive(true);
        itemDisplay.text = "You have received:";

        // call some function to add powerups and resume game, or move on to next level
        int randomItem = rnd.Next(3);
        if (randomItem == 0)
        {
            GameManager.reverseAmount++;
            powerupImages[0].SetActive(true);
        }
        if (randomItem == 1)
        {
            GameManager.frozenAmount++;
            powerupImages[1].SetActive(true);
        }
        if (randomItem == 2)
        {
            GameManager.spedupAmount++;
            powerupImages[2].SetActive(true);
        }

    }

    public void AnyOtherOption()
    {
        startCountdown = false;
        wrongAnswerPanel.SetActive(true);

    }

    public void ContinueGame()
    {

        // reset timing and all the powerup images
        timeLeft = 15;
        timeDisplay.color = Color.white;
        timeDisplay.gameObject.SetActive(false);
        foreach (GameObject image in powerupImages)
        {
            image.SetActive(false);
        }

        // revert quiz to usual state
        if (wrongAnswerPanel.activeInHierarchy) { wrongAnswerPanel.SetActive(false); }
        if (rightAnswerPanel.activeInHierarchy) { rightAnswerPanel.SetActive(false); }
        popQuizPanel.SetActive(true);

        if (gameManager.GetComponent<GameManager>().pelletsConsumed ==
        gameManager.GetComponent<GameManager>().totalPellets)
        {
            Debug.Log("Moving on to next level now");
            gameManager.GetComponent<GameManager>().StartNextLevel();
        }
        else
        {
            Debug.Log("Game will restart now");
            gameManager.GetComponent<GameManager>().ProcessRestartWrapper();
        }
    }

}
