using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class PlayerNameTag : MonoBehaviourPun
{
    [SerializeField] private Text nameTag = null;

    // Start is called before the first frame update
    void Start()
    {
        if (!photonView.IsMine) { return; }
        
        nameTag = GameObject.Find("Name").GetComponent<Text>();
        SetName();
    }

    public void SetName() {
        nameTag.text = photonView.Owner.NickName;
    }

}
