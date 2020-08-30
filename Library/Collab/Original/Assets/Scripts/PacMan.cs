using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PacMan : MonoBehaviour
{
    public float speed = 4.0f;
    public bool canMove = true;
    // adding in orientation for ghost chase
    public Vector2 orientation;

    // starting vertex
    private Vertex start;
    public Sprite frozenSprite;
    private Vector2 dir = Vector2.zero;
    private Vector2 nextDir;
    private Vertex currVertex;
    private Vertex prevVertex;
    private Vertex endVertex;
    public GameObject gameManager;

    // for the audio
    public AudioClip chomp;
    private AudioSource theAudio; // to not get mixed with the audio in unity

    // intialise all the runtimeanimatorcontrollers for pacman
    public RuntimeAnimatorController animateChomp;
    public RuntimeAnimatorController animateDeath;

    // Start is called before the first frame update
    void Start()
    {
        theAudio = GetComponent<AudioSource>();

        Vertex vertex = GetVertexPos(transform.localPosition);
        // if pacman is currently at a vertex (not any pellet):
        if (vertex != null)
        {
            start = vertex; // initialise the starting vertex as well
            currVertex = vertex;
            Debug.Log(currVertex);
        }

        // set Pacman to move left right from the start by default
        dir = Vector2.left;
        ChangePosition(dir);
    }

    // Restarts the game
    public void Restart()
    {
        canMove = true; // allow the pacman to move again
        transform.GetComponent<Animator>().runtimeAnimatorController = animateChomp; // reset the animation to normal chomp

        transform.GetComponent<SpriteRenderer>().enabled = true; // make pacman appear again since it was gone after death animation
        transform.position = start.transform.position; // resets the position

        currVertex = start;
        dir = Vector2.left;
        nextDir = Vector2.left; // to prevent user from pressing buttons in between
        orientation = Vector2.left;
        ChangePosition(dir);
    }

    // Update is called once per frame
    void Update()
    {
        // only if can move then check for these updates
        if (canMove)
        {
            CheckInput(); // at every frame, check the input
            MovePacMan(); // to make it move continuously instead
            RotatePacMan(); // to constantly rotate pacman as he switches direction
            UpdateSpriteState(); // stop pacman from animating at walls
            DestroyPellet(); // constatly consume the pellets on the go
        }
    }

    void CheckInput()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            ChangePosition(Vector2.left);
            // dir = Vector2.left;
            // MoveToNextVertex(dir);
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            ChangePosition(Vector2.right);
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            ChangePosition(Vector2.down);

        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            ChangePosition(Vector2.up);
        }
        // calling it outside would cause the pacman to keep trying to move towards the direction 
        // pressed until it hits a wall (null)
        // MoveToNextVertex(dir);
    }

    // this is activated when dir is changed from zero to something valid! + it updates every single direction click
    void ChangePosition(Vector2 queuedDir)
    {
        // constantly updates the next dir for the MovePacMan() function
        if (queuedDir != dir)
        {
            nextDir = queuedDir;
            // Debug.Log(queuedDir); --> can spam any button
        }

        // for the initial movement of the pacman right from the start
        if (currVertex != null)
        {
            Vertex nextVertex = CanMove(queuedDir);
            // Debug.Log("Current vertex is not null!");
            if (nextVertex != null)
            {
                // Debug.Log("Next vertex is not null!");
                dir = queuedDir;
                endVertex = nextVertex;
                prevVertex = currVertex;
                currVertex = null; // since the pacman is on the go between the prev and end vertices
            }
        }
    }

    void MovePacMan()
    {
        // no null pointer nonsense
        if (endVertex != null && endVertex != currVertex)
        {
            // for reversing direction on the move
            if (nextDir == dir * -1)
            {
                dir = -dir;
                // swap the prev and end vertices
                Vertex temp = prevVertex;
                prevVertex = endVertex;
                endVertex = temp;
            }


            if (ExceedEndpoint())
            {
                // since currVertex is null, set is as endVertex first and set pacman to stop momentarily at currVertex
                currVertex = endVertex;
                transform.localPosition = currVertex.transform.position; // global position

                // checks for gateway at endpoints
                CheckForGateway();

                // important since CanMove uses the currVertex to check
                Vertex nextVertex = CanMove(nextDir);

                // check if nextVertex is reachable, move to dir accordingly
                if (nextVertex != null)
                {
                    dir = nextDir;
                }
                // if nextDir is invalid, try curr dir instead
                if (nextVertex == null)
                {
                    nextVertex = CanMove(dir);
                }

                // now check again if the vertex is valid
                if (nextVertex != null)
                {
                    endVertex = nextVertex;
                    prevVertex = currVertex;
                    currVertex = null;
                }
                else
                {
                    dir = Vector2.zero; // if invalid, time to knock against the wall
                }
            }
            else
            {
                // if distance is not exceeded yet, continue moving
                transform.localPosition += (Vector3)(dir * speed) * Time.deltaTime; // local position is a Vector3
            }
        }
    }

    float CalculateDistanceFromVertex(Vector2 to)
    {
        Vector2 vector = to - (Vector2)prevVertex.transform.position; // global position of "from" vertex
        return vector.sqrMagnitude; // faster to compute rather than magnitude (no need for it)
    }

    bool ExceedEndpoint()
    {
        float toCurrPoint = CalculateDistanceFromVertex((Vector2)transform.localPosition);
        float toEndPoint = CalculateDistanceFromVertex((Vector2)endVertex.transform.position); // global position
        return toCurrPoint > toEndPoint; // returns true if dist of curr overshoots end point
    }

    // Remember that local pacman position is fixed to be the global position of next vertex/pellet AND not
    // the local position of the next vertex! (super important)
    // This is used for moving the pacman from one point to the other like magic
    /*
    void MoveToNextVertex(Vector2 dir)
    {
        Vertex next = CanMove(dir);
        if (next != null)
        {
            transform.localPosition = next.transform.position;
            // Debug.Log(transform.localPosition);
            currVertex = next; // update the curr vertex to be the next
        }
    }
    */

    // rotates pacman based on input direction and set the orientation properly based on its direction pointed to
    void RotatePacMan()
    {
        if (dir == Vector2.left)
        {
            orientation = Vector2.left;
            transform.localScale = new Vector3(1.5f, 1.5f, 1); // adjusted according to size of pacman
            transform.localRotation = Quaternion.Euler(0, 0, 0); // adjust back to correct orientation
        }
        else if (dir == Vector2.right)
        {
            orientation = Vector2.right;
            transform.localScale = new Vector3(-1.5f, 1.5f, 1);
            transform.localRotation = Quaternion.Euler(0, 0, 0); // adjust back to correct orientation
        }
        else if (dir == Vector2.down)
        {
            orientation = Vector2.down;
            transform.localScale = new Vector3(1.5f, 1.5f, 1); // have to reset orientation
            transform.localRotation = Quaternion.Euler(0, 0, 90); // tilt 90 degrees clockwise from curr pos
        }
        else if (dir == Vector2.up)
        {
            orientation = Vector2.up;
            transform.localScale = new Vector3(1.5f, 1.5f, 1); // have to reset orientation
            transform.localRotation = Quaternion.Euler(0, 0, 270);
        }
    }

    void UpdateSpriteState()
    {
        if (dir == Vector2.zero)
        {
            GetComponent<Animator>().enabled = false;
            GetComponent<SpriteRenderer>().sprite = frozenSprite;
        }
        else
        {
            GetComponent<Animator>().enabled = true;
        }
    }

    // get the current vertex position for pacman if possible
    Vertex GetVertexPos(Vector2 pos)
    {
        // find the game manager -> grab the game board script -> take the board coordinate position
        // GameObject tile = GameObject.Find("GameManager").GetComponent<GameManager>().board[(int)pos.x, (int)pos.y];
        GameObject tile = gameManager.GetComponent<GameManager>().board[(int)pos.x, (int)pos.y];
        // if it is not a pellet, tile will be null

        if (tile != null)
        {
            // if it is not a vertex, null will be returned as well
            // Debug.Log("This is" + tile);
            // Debug.Log(tile.GetComponent<Vertex>());
            return tile.GetComponent<Vertex>();
        }
        return null;
    }

    // iterate through all neighbours of current vertex and take next vertex if possible
    Vertex CanMove(Vector2 dir)
    {
        Vertex nextVertex = null;
        for (int i = 0; i < currVertex.vertices.Length; i++)
        {
            if (currVertex.validDir[i] == dir)
            {
                nextVertex = currVertex.vertices[i];
                break;
            }
        }
        return nextVertex;
    }

    // obtain any possible gateway
    GameObject ObtainGateway(Vector2 pos)
    {
        GameObject tile = gameManager.GetComponent<GameManager>().board[(int)pos.x, (int)pos.y];
        GameObject otherGateway = null;
        if (tile != null)
        {
            if (tile.GetComponent<Tile>() != null && tile.GetComponent<Tile>().isGateway)
            {
                otherGateway = tile.GetComponent<Tile>().oppositeGateway;
            }
        }
        return otherGateway;
    }

    // check whether the curr vertex is a gateway to teleport and teleport over if possible
    void CheckForGateway()
    {
        GameObject otherGateway = ObtainGateway(currVertex.transform.position); // global position
        if (otherGateway != null)
        {
            transform.localPosition = otherGateway.transform.position; // global position
            currVertex = otherGateway.GetComponent<Vertex>(); // queueDir is already towards the only connected vertexA
        }
    }

    // get the current tile position if possible
    GameObject GetTilePos(Vector2 pos)
    {
        GameObject tile = gameManager.GetComponent<GameManager>().board[(int)pos.x, (int)pos.y];
        if (tile != null)
        {
            return tile;
        }
        return null;
    }

    void DestroyPellet()
    {
        GameObject obj = GetTilePos(transform.position); // global position
        if (obj != null)
        {
            Tile tile = obj.GetComponent<Tile>();
            if (tile != null && !tile.isConsumed && (tile.isPellet || tile.isPowerPellet))
            {
                obj.GetComponent<SpriteRenderer>().enabled = false;
                tile.isConsumed = true;
                theAudio.PlayOneShot(chomp);
                gameManager.GetComponent<GameManager>().pelletsConsumed++; // consume one pellet
                // gameManager.GetComponent<GameManager>().playerScore += 10; // every pellet gives 10 points
                // since static, 
                GameManager.playerScore += 10;

                // start frightened mode
                if (tile.isPowerPellet)
                {
                    GameObject[] ghosts = GameObject.FindGameObjectsWithTag("GhostDemon");

                    foreach (GameObject ghost in ghosts)
                    {
                        ghost.GetComponent<Ghost>().InitiateFrightenedMode();
                    }
                }
            }
        }

    }
}
