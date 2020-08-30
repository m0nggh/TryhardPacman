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
    [SerializeField] private GameObject[] pacManPlayerPrefabs = null;
    private int pacmanId = 0;

    // use round robin or something to spawn players
    [Header("Positions")]
    [SerializeField] private Vector3[] pacManPosition = null;

    void Start()
    {
        int currentLevel = Int32.Parse(SceneManager.GetActiveScene().name.Substring(5));

        if (PlayerPrefs.GetString("PacmanSelected").Equals("redman"))
        {
            pacmanId = 1;
        }
        else if (PlayerPrefs.GetString("PacmanSelected").Equals("blueman"))
        {
            pacmanId = 2;
        }
        else if (PlayerPrefs.GetString("PacmanSelected").Equals("greenman"))
        {
            pacmanId = 3;
        }
        else if (PlayerPrefs.GetString("PacmanSelected").Equals("purpleman"))
        {
            pacmanId = 4;
        }

        var player = PhotonNetwork.Instantiate(pacManPlayerPrefabs[pacmanId].name,
        pacManPosition[currentLevel - 1],
        Quaternion.identity);

    }

}
