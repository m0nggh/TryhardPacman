using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Inventory : MonoBehaviour
{

    public void EquipRedMan()
    {
        PlayerPrefs.SetString("PacmanSelected", "redman");
    }

    public void EquipBlueMan()
    {
        PlayerPrefs.SetString("PacmanSelected", "blueman");
    }

    public void EquipGreenMan()
    {
        PlayerPrefs.SetString("PacmanSelected", "greenman");
    }

    public void EquipPurpleMan()
    {
        PlayerPrefs.SetString("PacmanSelected", "purpleman");
    }

    public void ResetDefault()
    {
        PlayerPrefs.DeleteKey("PacmanSelected");
        PlayerPrefs.DeleteKey("MazeSelected");
    }

    public void EquipRedMaze()
    {
        PlayerPrefs.SetString("MazeSelected", "redmaze");
    }

    public void EquipGreenMaze()
    {
        PlayerPrefs.SetString("MazeSelected", "greenmaze");
    }


    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
