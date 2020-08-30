using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    [Header("Ghost Prefabs")]
    [SerializeField] private GameObject[] ghosts = null;

    [Header("Positions")]
    [SerializeField] private Vector3[] blinkyPositions = null;
    [SerializeField] private Vector3[] pinkyPositions = null;
    [SerializeField] private Vector3[] inkyPositions = null;
    [SerializeField] private Vector3[] clydePositions = null;
    
    private const int totalPlayers = 5;

    // Start is called before the first frame update
    void Start()
    {
        int currentLevel = Int32.Parse(SceneManager.GetActiveScene().name.Substring(5));
        Debug.Log("We have entered level " + currentLevel);
        
        int numPlayersRequired = totalPlayers - PhotonNetwork.CurrentRoom.PlayerCount;
        Vector3[] currentGhost = null;

        for (int i = 0; i < numPlayersRequired; i++) {
            string Name = ghosts[i].name;

            if (Name == "ghost") { currentGhost = blinkyPositions; }
            if (Name == "pinky") { currentGhost = pinkyPositions; }
            if (Name == "inky") { currentGhost = inkyPositions; }
            if (Name == "clyde") { currentGhost = clydePositions; }

            PhotonNetwork.InstantiateSceneObject(Name, currentGhost[currentLevel - 1], Quaternion.identity); 
        }
    }
    
}
