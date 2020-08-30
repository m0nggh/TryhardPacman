using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vertex : MonoBehaviour
{
    public Vertex[] vertices;
    public Vector2[] validDir;
    // Start is called before the first frame update
    void Start()
    {
        validDir = new Vector2[vertices.Length];
        for (int i = 0; i < vertices.Length; i++)
        {
            Vertex next = vertices[i];
            Vector2 dist = next.transform.localPosition - transform.localPosition;
            validDir[i] = dist.normalized;
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
