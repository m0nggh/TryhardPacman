using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class PacMan : MonoBehaviourPun
{
    public float speed = 6.0f;
    public float defaultSpeed;
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

    // powerups for pacman
    public bool isReverse = false;
    public float reversedTime = 5;
    public float reversedPowerupTime;
    private float reverseBlink;
    public bool isFrozen = false;
    public float frozenTime = 10;
    public float frozenPowerupTime;
    private float frozenBlink;
    public bool isIncreasedSpeed;
    public float playerSpeedTime = 10;
    public float playerSpeedPowerupTime;
    private float spedupBlink;
    private float blinkInterval = 0.1f;

    // intialise all the runtimeanimatorcontrollers for pacman
    public RuntimeAnimatorController animateChomp;
    public RuntimeAnimatorController animateDeath;

    // Start is called before the first frame update
    void Start()
    {
        theAudio = GetComponent<AudioSource>();
        gameManager = GameObject.Find("GameManager");

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

        // initialise the difficulty level first
        InitialiseDifficultyLevel(gameManager.GetComponent<GameManager>().difficultyLevel, GameManager.playerLevel);
    }

    // sets the difficulty level for the game
    void InitialiseDifficultyLevel(int difficulty, int level)
    {
        // powerup time will be based on difficulty
        if (difficulty == 2)
        {
            reversedTime = 7;
            frozenTime = 7;
            playerSpeedTime = 7;
        }
        else if (difficulty == 3)
        {
            reversedTime = 10;
            frozenTime = 5;
            playerSpeedTime = 5;
        }

        // easy will start from level 1, medium: level 3, difficult: level 5
        if (level == 2)
        {
            speed = 7;
        }
        else if (level == 3)
        {
            speed = 8;
        }
        else if (level == 4)
        {
            speed = 9;
        }
        else if (level == 5)
        {
            speed = 10;
        }
        else if (level == 6)
        {
            speed = 11;
        }
        else if (level >= 7)
        {
            speed = 12;
        }
        defaultSpeed = speed; // set the default speed accordingly
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
        if (photonView.IsMine && canMove)
        {
            CheckInput(); // at every frame, check the input
            MovePacMan(); // to make it move continuously instead
            RotatePacMan(); // to constantly rotate pacman as he switches direction
            UpdateSpriteState(); // stop pacman from animating at walls
            photonView.RPC("DestroyPellet", RpcTarget.All); // constatly consume the pellets on the go
            TrackReversedMode(); // updates reversed mode whenever activated
            TrackFrozenMode(); // updates frozen mode whenever activated
            TrackSpeedIncreaseMode(); // updates speed increase mode whenever activated
        }
    }

    void CheckInput()
    {
        // for powerups
        if (!isReverse && GameManager.reverseAmount > 0 && Input.GetKeyDown(KeyCode.Z))
        {
            // ensure player does not press during reverse mode and there must be available ones to be used
            isReverse = true;
            GameManager.reverseAmount--; // use up one powerup
            gameManager.GetComponent<GameManager>().currentReverse.enabled = true; // enable the powerup image
        }

        if (!isFrozen && GameManager.frozenAmount > 0 && Input.GetKeyDown(KeyCode.X))
        {
            isFrozen = true;
            GameManager.frozenAmount--; // use up one powerup
            gameManager.GetComponent<GameManager>().StartFreezing();
            gameManager.GetComponent<GameManager>().currentFrozen.enabled = true; // enabled the powerup image
        }

        if (!isIncreasedSpeed && GameManager.spedupAmount > 0 && Input.GetKeyDown(KeyCode.C))
        {
            isIncreasedSpeed = true;
            GameManager.spedupAmount--; // use up one powerup
            speed *= 2; // activate the powerup
            gameManager.GetComponent<GameManager>().currentSpedup.enabled = true; // enable the powerup image
        }
        // for movements
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (isReverse)
            {
                ChangePosition(Vector2.right);
            }
            else
            {
                // default
                ChangePosition(Vector2.left);
            }
            // dir = Vector2.left;
            // MoveToNextVertex(dir);
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (isReverse)
            {
                ChangePosition(Vector2.left);
            }
            else
            {
                // default
                ChangePosition(Vector2.right);
            }
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (isReverse)
            {
                ChangePosition(Vector2.up);
            }
            else
            {
                // default
                ChangePosition(Vector2.down);
            }

        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (isReverse)
            {
                ChangePosition(Vector2.down);
            }
            else
            {
                // default
                ChangePosition(Vector2.up);
            }
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

    [PunRPC]
    void DestroyPellet()
    {
        GameObject obj = GetTilePos(transform.position); // global position
        if (obj != null)
        {
            Tile tile = obj.GetComponent<Tile>();
            if (tile != null && !tile.isConsumed)
            {
                if (tile.isPellet || tile.isPowerPellet)
                {

                    obj.GetComponent<SpriteRenderer>().enabled = false;
                    tile.isConsumed = true;
                    theAudio.PlayOneShot(chomp);
                    gameManager.GetComponent<GameManager>().pelletsConsumed++; // consume one pellet
                    // gameManager.GetComponent<GameManager>().playerScore += 10; // every pellet gives 10 points
                    // since static, 
                    // GameManager.playerScore += 10;

                    // start frightened mode
                    if (tile.isPowerPellet)
                    {
                        GameManager.playerScore += 100 * GameManager.playerLevel; // add 100 for power pellet eaten
                        GameObject[] ghosts = GameObject.FindGameObjectsWithTag("GhostDemon");

                        foreach (GameObject ghost in ghosts)
                        {
                            ghost.GetComponent<Ghost>().InitiateFrightenedMode();
                        }
                    }
                    else
                    {
                        GameManager.playerScore += 10 * GameManager.playerLevel; // multiplied by the level
                    }
                }
                else if (tile.isActivated)
                {
                    tile.isActivated = false; // deactivate the tile
                    if (tile.isBonusItem)
                    {
                        // if bonus item
                        GameManager.playerScore += tile.bonusPoints; // 500 by default
                        gameManager.GetComponent<GameManager>().StartConsumeBonusItem();
                    }
                    else if (tile.isReversed)
                    {
                        // if reversed
                        tile.isReversed = false;
                        GameManager.reverseAmount++; // add one powerup
                    }
                    else if (tile.isFreeze)
                    {
                        // if frozen
                        tile.isFreeze = false;
                        GameManager.frozenAmount++; // add one powerup
                    }
                    else if (tile.isPlayerSpeed)
                    {
                        // if sped up
                        tile.isPlayerSpeed = false;
                        GameManager.spedupAmount++; // add one powerup
                    }
                    else if (tile.isExtraLife)
                    {
                        tile.isExtraLife = false;
                        GameManager.lives++; // add one life
                    }
                }
            }
        }
    }

    void TrackReversedMode()
    {
        if (isReverse)
        {
            Image reverseImage = gameManager.GetComponent<GameManager>().currentReverse;
            reversedPowerupTime += Time.deltaTime;
            // advantage to reversed mode -> score multiplier based on time
            GameManager.playerScore += (int)reversedPowerupTime;

            if (reversedPowerupTime >= reversedTime)
            {
                isReverse = false; // set the game mode back to normal
                reversedPowerupTime = 0; // reset the timer back to 0
                reverseBlink = 0;
                reverseImage.enabled = false; // disable the powerup image
            }
            else if (reversedPowerupTime >= reversedTime - 2)
            {
                // start blinking 2 sec before duration ends
                if (reverseBlink < blinkInterval)
                {
                    reverseBlink += Time.deltaTime; // continue incrementing time
                }
                else
                {
                    reverseBlink = 0f; // reset blink time
                    if (reverseImage.enabled)
                    {
                        reverseImage.enabled = false;
                    }
                    else
                    {
                        reverseImage.enabled = true;
                    }
                }
            }
        }
    }

    void TrackFrozenMode()
    {
        if (isFrozen)
        {
            Image frozenImage = gameManager.GetComponent<GameManager>().currentFrozen;
            frozenPowerupTime += Time.deltaTime;

            if (frozenPowerupTime >= frozenTime)
            {
                isFrozen = false; // set the game mode back to normal
                frozenPowerupTime = 0; // reset the timer back to 0
                frozenBlink = 0;
                frozenImage.enabled = false; // disable the powerup image
            }
            else if (frozenPowerupTime >= frozenTime - 2)
            {
                // start blinking 2 sec before duration ends
                if (frozenBlink < blinkInterval)
                {
                    frozenBlink += Time.deltaTime; // continue incrementing time
                }
                else
                {
                    frozenBlink = 0f; // reset blink time
                    if (frozenImage.enabled)
                    {
                        frozenImage.enabled = false;
                    }
                    else
                    {
                        frozenImage.enabled = true;
                    }
                }
            }
        }
    }

    void TrackSpeedIncreaseMode()
    {
        if (isIncreasedSpeed)
        {
            Image spedupImage = gameManager.GetComponent<GameManager>().currentSpedup;
            playerSpeedPowerupTime += Time.deltaTime;

            if (playerSpeedPowerupTime >= playerSpeedTime)
            {
                isIncreasedSpeed = false; // set the game mode back to normal
                speed = defaultSpeed; // reset the speed
                playerSpeedPowerupTime = 0; // reset the timer back to 0
                spedupBlink = 0;
                spedupImage.enabled = false; // disable the powerup image
            }
            else if (playerSpeedPowerupTime >= playerSpeedTime - 2)
            {
                // start blinking 2 sec before duration ends
                if (spedupBlink < blinkInterval)
                {
                    spedupBlink += Time.deltaTime; // continue incrementing time
                }
                else
                {
                    spedupBlink = 0f; // reset blink time
                    if (spedupImage.enabled)
                    {
                        spedupImage.enabled = false;
                    }
                    else
                    {
                        spedupImage.enabled = true;
                    }
                }
            }
        }
    }
}
