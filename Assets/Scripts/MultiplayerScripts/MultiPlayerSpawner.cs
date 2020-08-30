using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;

public class MultiPlayerSpawner : MonoBehaviourPunCallbacks
{
    [Header("Prefabs")]
    [SerializeField] private GameObject[] pacManPlayerPrefabs = null;

    [Header("Positions")]
    [SerializeField] private Vector3[] pacManPosition = null;
    // Start is called before the first frame update
    void Start()
    {
        int posIndex = Int32.Parse(PhotonNetwork.LocalPlayer.NickName.Substring(0)) - 1;

        Debug.Log("This is player " + posIndex);

        var player = PhotonNetwork.Instantiate(pacManPlayerPrefabs[posIndex].name,
        pacManPosition[posIndex],
        Quaternion.identity);
    }
}
