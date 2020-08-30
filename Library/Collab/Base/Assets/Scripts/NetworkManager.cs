using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private GameObject[] ghosts = null;
    
    private const int totalPlayers = 5;

    // Start is called before the first frame update
    void Start()
    {
        int numPlayersRequired = totalPlayers - PhotonNetwork.CurrentRoom.PlayerCount;
        for (int i = 0; i < numPlayersRequired; i++) {
            PhotonNetwork.Instantiate(ghosts[i].name, ghosts[i].transform.position, Quaternion.identity);
        }
    }
    
}
