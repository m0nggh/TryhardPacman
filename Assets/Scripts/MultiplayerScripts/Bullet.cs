using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Bullet : MonoBehaviourPun
{
    
    public GameObject gameManager;

    [Header("Direction")]
    public float vertical = 0;
    public float horizontal = 0;
    
    // Start is called before the first frame update
    void Start()
    {
        gameManager = GameObject.Find("GameManager");
        SetDirection();
        GetComponent<Rigidbody2D>().velocity = new Vector2(horizontal * 8, vertical * 8);
    }

    // Update is called once per frame
    void Update()
    {
        if (GetTilePos(transform.position) == null) {
            PhotonNetwork.Destroy(this.gameObject.GetPhotonView());
        }
    }

    // this bullet goes to the right
    public virtual void SetDirection() {
        horizontal = 1;
    }

    GameObject GetTilePos(Vector2 pos)
    {
        GameObject tile = gameManager.GetComponent<MultiGameManager>().board[(int)pos.x, (int)pos.y];
        if (tile != null)
        {
            return tile;
        }
        return null;
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if (other.gameObject.tag == "PacManWarrior") {
            PhotonNetwork.Destroy(this.photonView);
        }
    }

}
