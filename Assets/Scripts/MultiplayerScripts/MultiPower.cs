using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class MultiPower : MonoBehaviourPun
{
    private void OnTriggerEnter2D(Collider2D other) {
        if (other.gameObject.tag == "PacManWarrior") {
            PhotonNetwork.Destroy(this.photonView);
        }
    }

}
