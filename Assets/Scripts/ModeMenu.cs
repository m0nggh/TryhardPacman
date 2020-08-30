using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Firebase;
using Firebase.Auth;

public class ModeMenu : MonoBehaviour
{
    [Header("PacMan Colours")]
    [SerializeField] private Sprite[] sprites = null;

    [Header("References")]
    [SerializeField] private Image currentPacMan = null;
    public void EnterSelection() {
        SceneManager.LoadScene("GameLobby");
    }

    public void EquipRed() {
        currentPacMan.sprite = sprites[0];
    }
    public void EquipBlue() {
        currentPacMan.sprite = sprites[1];
    }
    public void EquipGreen() {
        currentPacMan.sprite = sprites[2];
    }
    public void EquipPurple() {
        currentPacMan.sprite = sprites[3];
    }
    public void Default() {
        currentPacMan.sprite = sprites[4];
    }

    public void QuitGame() {
        Application.Quit();
    }
}
