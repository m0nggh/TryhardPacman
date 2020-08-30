using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemData
{
    public bool[] pacmanBuy;
    public bool[] mapBuy;
    public bool[] topicBuy;
    public bool[] pacmanEquip;
    public bool[] mapEquip;

    public ItemData(bool[] pacmanBuy, bool[] mapBuy, bool[] topicBuy, bool[] pacmanEquip, bool[] mapEquip) {
        this.pacmanBuy = pacmanBuy;
        this.mapBuy = mapBuy;
        this.topicBuy = topicBuy;
        this.pacmanEquip = pacmanEquip;
        this.mapEquip = mapEquip;
    }
    
}
