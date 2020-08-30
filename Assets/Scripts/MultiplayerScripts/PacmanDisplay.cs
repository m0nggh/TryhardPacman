using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PacmanDisplay : MonoBehaviour
{
    [Header("Text")]
    public Text powerTimeLeft;
    public Text bulletsLeft;
    public Text shieldTimeLeft;
    public Text teleportToggle;
    public Text speedUpTimeLeft;

    void Start() {
        powerTimeLeft = GameObject.Find("PowerTimeValue").GetComponent<Text>();
        bulletsLeft = GameObject.Find("BulletValue").GetComponent<Text>();
        shieldTimeLeft = GameObject.Find("ShieldTimeValue").GetComponent<Text>();
        teleportToggle = GameObject.Find("TeleportValue").GetComponent<Text>();
        speedUpTimeLeft = GameObject.Find("SpeedUpTimeValue").GetComponent<Text>();
    }

    public void UpdatePowerTime(float time) {
        powerTimeLeft.text = "" + time;
    }

    public void UpdateBullets(int bullets) {
        bulletsLeft.text = "" + bullets;
    }

    public void UpdateShieldTime(float time) {
        shieldTimeLeft.text = "" + time;
    }

    public void UpdateTeleport(bool toggle) {
        if (toggle) {
            teleportToggle.text = "press q";
        } else {
            teleportToggle.text = "none";
        }
        
    }

    public void UpdateSpeedUpTime(float time) {
        speedUpTimeLeft.text = "" + time;
    }
}
