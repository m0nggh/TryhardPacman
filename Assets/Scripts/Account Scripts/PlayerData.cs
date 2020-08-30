using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PlayerData
{
    public string username;
    public string email;
    public string password;
    public int points;

    public PlayerData() { }

    public PlayerData(string username, string email, string password) {
        this.username = username;
        this.email = email;
        this.password = password;
    }

    public PlayerData(string username, string email, string password, int points) {
        this.username = username;
        this.email = email;
        this.password = password;
        this.points = points;
    }
}
