using UnityEngine;

public class Tile : MonoBehaviour
{
    public GameObject oppositeGateway;
    public bool isGateway;
    public bool isPellet;
    public bool isPowerPellet;
    public bool isConsumed;
    public bool isEntrance; // entrance to ghosthouse
    public bool isGhosthouse;
    // for powerups
    public bool isActivated;
    public bool isReversed;
    public bool isFreeze;
    public bool isPlayerSpeed;
    public bool isExtraLife;
    public bool isBonusItem;
    public int bonusPoints;
}
