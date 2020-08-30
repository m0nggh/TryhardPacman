using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuAnimation : MonoBehaviour
{
    public float speed = 400;
    public Vector3 startPos;
    public GameObject[] points;
    public float timer;
    private int currPoint;
    private Vector3 currPos;

    // Start is called before the first frame update
    void Start()
    {
        CheckPoints();
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime * speed;
        if (gameObject.transform.position != currPos)
        {
            // if point is not reached
            gameObject.transform.position = Vector3.Lerp(startPos, currPos, timer);
        }
        else
        {
            if (currPoint == points.Length - 1)
            {
                // if at end of loop, move back to first
                currPoint = 0;
            }
            else
            {
                currPoint++;
            }
            CheckPoints();
        }
    }

    void CheckPoints()
    {
        // updates next point to move to
        timer = 0;
        currPos = points[currPoint].transform.position;
        startPos = gameObject.transform.position;
        // currPoint is where the game object is moving towards
        if (currPoint == 0)
        {
            transform.localRotation = Quaternion.Euler(0, 0, 270);
        }
        else if (currPoint == 1)
        {
            transform.localRotation = Quaternion.Euler(0, 0, 0);
        }
        else if (currPoint == 2)
        {
            transform.localRotation = Quaternion.Euler(0, 0, 90);
        }
        else if (currPoint == 3)
        {
            transform.localRotation = Quaternion.Euler(0, 0, 180);
        }
    }
}