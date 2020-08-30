using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameMenu : MonoBehaviour
{
    public static bool isOnePlayerSelected = true;

    public Text singlePlayer;
    public Text multiPlayer;
    public Text selectionArrow;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        // getkeydown -> happen once key is pressed
        // getkey -> continue to happen while holding the key
        // getkeyup -> happens once when key is released
        if (Input.GetKeyUp(KeyCode.UpArrow))
        {
            // set the flag accordingly
            if (!isOnePlayerSelected)
            {
                isOnePlayerSelected = true;
                selectionArrow.transform.localPosition = new Vector3(selectionArrow.transform.localPosition.x,
                            singlePlayer.transform.localPosition.y, selectionArrow.transform.localPosition.z);
            }
        }
        else if (Input.GetKeyUp(KeyCode.DownArrow))
        {
            if (isOnePlayerSelected)
            {
                isOnePlayerSelected = false;
                selectionArrow.transform.localPosition = new Vector3(selectionArrow.transform.localPosition.x,
                            multiPlayer.transform.localPosition.y, selectionArrow.transform.localPosition.z);
            }
        }
        else if (Input.GetKeyUp(KeyCode.Return))
        {
            // SceneManager.LoadScene("Level1");
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1); // to get next scene
        }
    }
}
