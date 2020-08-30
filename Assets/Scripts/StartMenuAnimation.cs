using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartMenuAnimation : MonoBehaviour
{
    public float speed = 0.3f;
    public Vector3 startPos;
    public Vector3 endPos;
    public GameObject startPoint;
    public GameObject endPoint;
    public float timer;
    // Start is called before the first frame update
    void Start()
    {
        startPos = startPoint.transform.position;
        endPos = endPoint.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime * speed;
        if (gameObject.transform.position != endPos)
        {
            // if point is not reached
            gameObject.transform.position = Vector3.Lerp(startPos, endPos, timer);
        }
        else
        {
            timer = 0;
            gameObject.transform.position = startPos;
        }
    }
}

