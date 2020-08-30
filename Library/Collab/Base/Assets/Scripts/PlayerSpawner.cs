using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;

public class PlayerSpawner : MonoBehaviourPunCallbacks
{
    [Header("Prefabs")]
    [SerializeField] private GameObject pacManPlayerPrefab = null;
    [SerializeField] private GameObject[] ghostPlayerPrefabs = null;

    [Header("Positions")]
    [SerializeField] private Vector3[] pacManPosition = null;
    [SerializeField] private Vector3[] blinkyPositions = null;
    [SerializeField] private Vector3[] pinkyPositions = null;
    [SerializeField] private Vector3[] inkyPositions = null;
    [SerializeField] private Vector3[] clydePositions = null;

    void Start() {
        int currentLevel = Int32.Parse(SceneManager.GetActiveScene().name.Substring(5));

        string role = PlayerPrefs.GetString("PlayerRole");
        Debug.Log("Player is currently: " + role);
        Debug.Log(PlayerPrefs.GetString("PlayerName"));

        if (role == "PacMan") {
            var player = PhotonNetwork.Instantiate(pacManPlayerPrefab.name, 
            pacManPosition[currentLevel - 1], 
            Quaternion.identity);
        }
        if (role == "Blinky") {
            ghostPlayerPrefabs[0].GetComponent<Ghost>().isHuman = true;

            var player = PhotonNetwork.Instantiate(ghostPlayerPrefabs[0].name, 
            blinkyPositions[currentLevel - 1], 
            Quaternion.identity);
        }
        if (role == "Pinky") {
            ghostPlayerPrefabs[1].GetComponent<Ghost>().isHuman = true;

            var player = PhotonNetwork.Instantiate(ghostPlayerPrefabs[1].name, 
            pinkyPositions[currentLevel - 1], 
            Quaternion.identity);
        }
        if (role == "Inky") {
            ghostPlayerPrefabs[2].GetComponent<Ghost>().isHuman = true;

            var player = PhotonNetwork.Instantiate(ghostPlayerPrefabs[2].name, 
            inkyPositions[currentLevel - 1], 
            Quaternion.identity);
        }
        if (role == "Clyde") {
            ghostPlayerPrefabs[3].GetComponent<Ghost>().isHuman = true;

            var player = PhotonNetwork.Instantiate(ghostPlayerPrefabs[3].name, 
            clydePositions[currentLevel - 1], 
            Quaternion.identity);
        }
        
    }

}
