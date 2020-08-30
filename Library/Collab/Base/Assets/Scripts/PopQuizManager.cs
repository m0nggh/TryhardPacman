using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PopQuizManager : MonoBehaviour
{
    [Header("Timer")]
    [SerializeField] private int timeLeft = 15;

    [Header("Game Manager")]
    [SerializeField] private GameObject gameManager = null;

    public int numQuestions = 3;

    private bool startCountdown = false;
    private System.Random rnd = new System.Random();

    [Header("UI")]
    [SerializeField] private Text timeDisplay = null;
    [SerializeField] private Text itemDisplay = null;
    [SerializeField] private GameObject wrongAnswerPanel = null;
    [SerializeField] private GameObject rightAnswerPanel = null;
    [SerializeField] private GameObject[] powerupImages = null;

    [Header("Topic Questions")]
    [SerializeField] private GameObject[] geographyQuestions = null;
    [SerializeField] private GameObject[] historyQuestions = null;
    [SerializeField] private GameObject[] gamingQuestions = null;
    [SerializeField] private GameObject[] movieQuestions = null;

    void Update() {
        if (timeLeft <= 0) {
            startCountdown = false;
            timeDisplay.text = "" + 0;
            // call some function
        }

        if (startCountdown) {
            timeLeft = (int) (timeLeft - Time.deltaTime);
            timeDisplay.text = "" + timeLeft;
        }
    }

    // activated by pressing any topic button
    public void StartToCount() { 
        startCountdown = true;
        timeDisplay.gameObject.SetActive(true);
    }    

    public void PickRandomGeogQuestion() {
        geographyQuestions[rnd.Next(numQuestions)].SetActive(true);
    }
    public void PickRandomHistQuestion() {
        historyQuestions[rnd.Next(numQuestions)].SetActive(true);
    }
    public void PickRandomGameQuestion() {
        gamingQuestions[rnd.Next(numQuestions)].SetActive(true);
    }
    public void PickRandomMovieQuestion() {
        movieQuestions[rnd.Next(numQuestions)].SetActive(true);
    }
    
    public void CorrectOptionPressed() {
        startCountdown = false;
        rightAnswerPanel.SetActive(true);
        itemDisplay.text = "You have received:";

        // call some function to add powerups and resume game, or move on to next level
        int randomItem = rnd.Next(3);
        if (randomItem == 0) {
            GameManager.reverseAmount++;
            powerupImages[0].SetActive(true);
        }
        if (randomItem == 1) {
            GameManager.frozenAmount++;
            powerupImages[1].SetActive(true);
        }
        if (randomItem == 2) {
            GameManager.spedupAmount++;
            powerupImages[2].SetActive(true);
        }
        
    }

    public void AnyOtherOption() {
        wrongAnswerPanel.SetActive(true);
    
    }

    public void ContinueGame() {
        if (gameManager.GetComponent<GameManager>().pelletsConsumed == 
        gameManager.GetComponent<GameManager>().totalPellets) {
            Debug.Log("Moving on to next level now");
            gameManager.GetComponent<GameManager>().StartNextLevel();
        } else {
            Debug.Log("Game will restart now");
            StartCoroutine(gameManager.GetComponent<GameManager>().ProcessRestart(0));
        }
    }

}
