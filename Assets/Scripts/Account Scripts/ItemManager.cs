using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using Firebase;
using Firebase.Auth;
using Firebase.Unity.Editor;
using Firebase.Database;

public class ItemManager : MonoBehaviour
{
    [Header("Points")]
    public Text totalPoints;

    [Header("Buy Buttons")]
    public Button[] pacmanBuyButtons;
    public Button[] mapBuyButtons;
    public Button[] topicBuyButtons;

    [Header("Equip Buttons")]
    public Button[] pacmanEquipButtons;
    public Button[] mapEquipButtons;

    [Header("Toggle Purchase")]
    public bool[] pacmanBuy;
    public bool[] mapBuy;
    public bool[] topicBuy;

    [Header("Toggle Equip")]
    public bool[] pacmanEquip;
    public bool[] mapEquip;

    [Header("References")]
    public int points;
    private DatabaseReference databaseReference;

    private Firebase.Auth.FirebaseUser currentUser = null;
    
    // Start is called before the first frame update
    void Start()
    {
        FirebaseApp.DefaultInstance.SetEditorDatabaseUrl("https://orbital-pacman.firebaseio.com/");
        databaseReference = FirebaseDatabase.DefaultInstance.RootReference;

        if (FirebaseAuth.DefaultInstance.CurrentUser != null) {
            UpdateInventory();
        }
    }

    public void UpdateInventory() {

        Debug.Log("Called");

        if (FirebaseAuth.DefaultInstance.CurrentUser == null) return;

        Debug.Log("Passed");

        currentUser = FirebaseAuth.DefaultInstance.CurrentUser;
        string[] strList = currentUser.Email.Split('.');
        string storedEmail = strList[0] + strList[1];

        points = Int32.Parse(totalPoints.text);

        // add some firebase thing to toggle setactive for buy buttons, depending on if they are purchased
        // also toggle equip buttons
        for (int i = 0; i < pacmanBuy.Length; i++) {
            FirebaseDatabase.DefaultInstance.GetReference("/Items/" + storedEmail + "/pacmanBuy/" + i).GetValueAsync().
            ContinueWith(task => {
                if (task.IsFaulted) {
                    Debug.Log("OSHIETE YO");
                }
                if (task.IsCompleted) {
                    DataSnapshot snapshot = task.Result;
                    bool result = bool.Parse(snapshot.GetRawJsonValue());

                    pacmanBuy[i] = result;
                    Debug.Log("Success");
                }
            });

            FirebaseDatabase.DefaultInstance.GetReference("/Items/" + storedEmail + "/pacmanEquip/" + i).GetValueAsync().
            ContinueWith(task => {
                if (task.IsFaulted) {
                    Debug.Log("OSHIETE YO");
                }
                if (task.IsCompleted) {
                    DataSnapshot snapshot = task.Result;
                    bool result = bool.Parse(snapshot.GetRawJsonValue());

                    pacmanEquip[i] = result;
                    Debug.Log("Success");
                }
            });
        }

        for (int i = 0; i < mapBuy.Length; i++) {
            FirebaseDatabase.DefaultInstance.GetReference("/Items/" + storedEmail + "/mapBuy/" + i).GetValueAsync().
            ContinueWith(task => {
                if (task.IsFaulted) {
                    Debug.Log("OSHIETE YO");
                }
                if (task.IsCompleted) {
                    DataSnapshot snapshot = task.Result;
                    bool result = bool.Parse(snapshot.GetRawJsonValue());

                    mapBuy[i] = result;
                    Debug.Log("Success");
                }
            });

            FirebaseDatabase.DefaultInstance.GetReference("/Items/" + storedEmail + "/mapEquip/" + i).GetValueAsync().
            ContinueWith(task => {
                if (task.IsFaulted) {
                    Debug.Log("OSHIETE YO");
                }
                if (task.IsCompleted) {
                    DataSnapshot snapshot = task.Result;
                    bool result = bool.Parse(snapshot.GetRawJsonValue());

                    mapEquip[i] = result;
                    Debug.Log("Success");
                }
            });
        }

        for (int i = 0; i < topicBuy.Length; i++) {
            FirebaseDatabase.DefaultInstance.GetReference("/Items/" + storedEmail + "/topicBuy/" + i).GetValueAsync().
            ContinueWith(task => {
                if (task.IsFaulted) {
                    Debug.Log("OSHIETE YO");
                }
                if (task.IsCompleted) {
                    DataSnapshot snapshot = task.Result;
                    bool result = bool.Parse(snapshot.GetRawJsonValue());

                    topicBuy[i] = result;
                    Debug.Log("Success");
                }
            });
        }

        Debug.Log("Calling next");
        UpdateUI();
    }

    void UpdateUI() {

        Debug.Log("Next");
        // disable buttons if player has insufficient currency
        if (points < 400) {
            foreach (Button button in mapBuyButtons) {
                button.interactable = false;
            }
        }
        if (points < 300) {
            foreach (Button button in topicBuyButtons) {
                button.interactable = false;
            }
        }
        if (points < 200) {
            Debug.Log("Insufficient points");
            foreach (Button button in pacmanBuyButtons) {
                button.interactable = false;
            }
        }

        // toggle buy buttons
        for (int a = 0; a < pacmanBuy.Length; a++) {
            pacmanBuyButtons[a].interactable = pacmanBuy[a];
        }
        for (int b = 0; b < mapBuy.Length; b++) {
            mapBuyButtons[b].interactable = mapBuy[b];
        }
        for (int c = 0; c < topicBuy.Length; c++) {
            topicBuyButtons[c].interactable = topicBuy[c];
        }

        // toggle equip buttons
        for (int i = 0; i < pacmanEquip.Length; i++) {
            pacmanEquipButtons[i].interactable = pacmanEquip[i];
        }
        for (int j = 0; j < mapEquip.Length; j++) {
            mapEquipButtons[j].interactable = mapEquip[j];
        }

        Debug.Log("Ended");
    }

    // PACMAN BUY BUTTONS
    public void BuyRed() {
        pacmanBuy[0] = false;
        pacmanEquip[0] = true;
        points -= 200;
        totalPoints.text = "" + points;
        UpdateUI();
    }
    public void BuyBlue() {
        pacmanBuy[1] = false;
        pacmanEquip[1] = true;
        points -= 200;
        totalPoints.text = "" + points;
        UpdateUI();
    }
    public void BuyGreen() {
        pacmanBuy[2] = false;
        pacmanEquip[2] = true;
        points -= 200;
        totalPoints.text = "" + points;
        UpdateUI();
    }
    public void BuyPurple() {
        pacmanBuy[3] = false;
        pacmanEquip[3] = true;
        points -= 200;
        totalPoints.text = "" + points;
        UpdateUI();
    }

    // MAP BUY BUTTONS
    public void BuyRedMap() {
        mapBuy[0] = false;
        mapEquip[0] = true;
        points -= 400;
        totalPoints.text = "" + points;
        UpdateUI();
    }
    public void BuyGreenMap() {
        mapBuy[1] = false;
        mapEquip[1] = true;
        points -= 400;
        totalPoints.text = "" + points;
        UpdateUI();
    }

    // POPQUIZ BUY BUTTONS
    public void BuyTopicOne() {
        topicBuy[0] = false;
        points -= 300;
        totalPoints.text = "" + points;
        UpdateUI();
    }
    public void BuyTopicTwo() {
        topicBuy[1] = false;
        points -= 300;
        totalPoints.text = "" + points;
        UpdateUI();
    }
    public void BuyTopicThree() {
        topicBuy[2] = false;
        points -= 300;
        totalPoints.text = "" + points;
        UpdateUI();
    }

    public void SaveTransactions() {
        string[] strList = currentUser.Email.Split('.');
        string savedEmail = strList[0] + strList[1];
        Debug.Log("Creating new item storage...");

        ItemData data = new ItemData(pacmanBuy, mapBuy, topicBuy, pacmanEquip, mapEquip);
        string json = JsonUtility.ToJson(data);

        databaseReference.Child("Items").Child(savedEmail).SetRawJsonValueAsync(json);
    }


}
