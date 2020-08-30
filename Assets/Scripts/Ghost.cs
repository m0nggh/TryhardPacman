using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Ghost : MonoBehaviourPun
{
    // attempt on human controlled
    public bool isHuman = false; // default for all

    // all the different speed variables and whether ghosts can move
    public float speed = 5.9f;
    public float defaultSpeed = 5.9f;
    public float frightenedSpeed = 3.0f;
    public float consumedSpeed = 15f;
    public float prevSpeed;

    public bool canMove = true;

    // vertices for the game setup
    public Vertex start;
    // default scatter vertices: Blinky: top right, Pinky: top left, Inky: bottom right, Clyde: bottom left
    public Vertex scatterVertex;
    public Vertex ghostHouse;
    private GameObject pacman;

    // music component for the game
    private AudioSource backgroundMusic;

    // similarly to Pacman
    private Vector2 dir;
    private Vector2 nextDir; // only used for human controls but ai controlled will not use this attribute
    private Vertex currVertex;
    private Vertex prevVertex;
    private Vertex endVertex;
    public GameObject gameManager;

    // extra game mode element for ghost
    public enum Mode
    {
        Chase, Scatter, Frightened, Consumed
    }

    public float scatterModeTime1 = 7;
    public float chaseModeTime1 = 20;
    public float scatterModeTime2 = 5;
    public float chaseModeTime2 = 22;

    private int modeIndex = 1;
    private float modeTime = 0;
    private Mode currMode = Mode.Scatter;
    private Mode prevMode;

    // variables for diff ghosts
    public float pinkyStartTime = 5;
    public float inkyStartTime = 12;
    public float clydeStartTime = 20;
    public float ghostStartTime = 0;
    public bool inGhosthouse = false;

    // more variables for frightened mode
    public int frightenedModeTime = 10;
    public int blinkAt = 7;
    private float frightTime = 0;
    private float blinkTime = 0;
    private float blinkInterval = 0.1f;
    private bool atWhite;

    public enum GhostType
    {
        Blinky, Pinky, Inky, Clyde
    }

    public GhostType ghostType = GhostType.Blinky; // by default red ghost first

    // initialise all the runtime animator controllers for the ghosts
    public RuntimeAnimatorController ghostLeft;
    public RuntimeAnimatorController ghostRight;
    public RuntimeAnimatorController ghostDown;
    public RuntimeAnimatorController ghostUp;
    public RuntimeAnimatorController ghostRainbow;
    public RuntimeAnimatorController ghostBlue;
    public RuntimeAnimatorController ghostWhite;

    // initialise all the eyes for ghost animation
    public Sprite eyesLeft;
    public Sprite eyesRight;
    public Sprite eyesDown;
    public Sprite eyesUp;

    // Start is called before the first frame update
    void Start()
    {
        pacman = GameObject.FindGameObjectWithTag("PacManWarrior");
        gameManager = GameObject.Find("GameManager");

        // find vertices using ghosts type
        if (ghostType == GhostType.Blinky)
        {
            start = GameObject.Find("GhostHouseEntrance").GetComponent<Vertex>();
            scatterVertex = GameObject.Find("ScatterSpot (1)").GetComponent<Vertex>();
            ghostHouse = GameObject.Find("GhostHouseSpot").GetComponent<Vertex>();
        }
        if (ghostType == GhostType.Pinky)
        {
            start = GameObject.Find("GhostHouseSpot").GetComponent<Vertex>();
            scatterVertex = GameObject.Find("ScatterSpot").GetComponent<Vertex>();
            ghostHouse = GameObject.Find("GhostHouseSpot").GetComponent<Vertex>();
        }
        if (ghostType == GhostType.Inky)
        {
            start = GameObject.Find("GhostHouseSpot (1)").GetComponent<Vertex>();
            scatterVertex = GameObject.Find("ScatterSpot (2)").GetComponent<Vertex>();
            ghostHouse = GameObject.Find("GhostHouseSpot").GetComponent<Vertex>();
        }
        if (ghostType == GhostType.Clyde)
        {
            start = GameObject.Find("GhostHouseSpot (2)").GetComponent<Vertex>();
            scatterVertex = GameObject.Find("ScatterSpot (3)").GetComponent<Vertex>();
            ghostHouse = GameObject.Find("GhostHouseSpot").GetComponent<Vertex>();
        }

        backgroundMusic = gameManager.GetComponent<AudioSource>(); // need transform???
        Debug.Log("Starting vertex for ghost: " + start);

        Vertex vertex = GetVertexPos(transform.localPosition);
        // if ghost is currently at a vertex:
        if (vertex != null)
        {
            currVertex = vertex;
            Debug.Log("Starting ghost position: " + currVertex.transform.position);
        }
        prevVertex = currVertex;

        // initialise the difficulty level
        InitialiseDifficultyLevel(GameManager.playerLevel);

        // initialise the ghosts movement out of the ghosthouse
        if (inGhosthouse)
        {
            // when in ghost house, inky has to move right first
            if (ghostType == GhostType.Inky)
            {
                dir = Vector2.right;
            }
            else if (ghostType == GhostType.Clyde) // clyde has to move left first
            {
                dir = Vector2.left;
            }
            else // pinky has to move up first
            {
                dir = Vector2.up;
            }
            // check if it is human controlled
            if (isHuman)
            {
                ChangePosition(dir);
            }
            else
            {
                endVertex = currVertex.vertices[0]; // only one vertex to choose to move to
            }
            // GetComponent<Animator>().enabled = false; // if no animation at the start
        }
        else
        {
            dir = Vector2.left;
            if (isHuman)
            {
                ChangePosition(dir);
            }
            else
            {
                endVertex = FindNearestVertex();
            }

        }


        UpdateGhostAnimation(); // initial animation

        // ------This is for one ghost only------
        // intialise the first endVertex for the ghost to start moving
        // Vector2 targetPos = pacman.transform.localPosition; // does not matter here whether local or global
        // Vector2 chaseTarget = new Vector2((int)targetPos.x, (int)targetPos.y);
        // endVertex = GetVertexPos(chaseTarget);

        // initialise first direction to move
        // dir = Vector2.right;
    }

    // sets the difficulty level for the game
    void InitialiseDifficultyLevel(int level)
    {
        // easy: level 1, medium: level 3, difficult: level 5
        // decrement scattermodetime by 0.5, increment chasemodetime by 2, decrement frightened mode time and blinkAt by 1
        // decrement start times for ghosts
        if (level == 2)
        {
            scatterModeTime1 = 6.5f;
            scatterModeTime2 = 4.5f;
            chaseModeTime1 = 22;
            chaseModeTime2 = 24;

            frightenedModeTime = 9;
            blinkAt = 6;

            pinkyStartTime = 4.5f;
            inkyStartTime = 11;
            clydeStartTime = 18;

            speed = 6.9f;
            defaultSpeed = 6.9f;
            frightenedSpeed = 3.9f;
            consumedSpeed = 16;
        }
        else if (level == 3)
        {
            scatterModeTime1 = 6;
            scatterModeTime2 = 4;
            chaseModeTime1 = 24;
            chaseModeTime2 = 26;

            frightenedModeTime = 8;
            blinkAt = 5;

            pinkyStartTime = 4;
            inkyStartTime = 10;
            clydeStartTime = 15;

            speed = 7.9f;
            defaultSpeed = 7.9f;
            frightenedSpeed = 4.9f;
            consumedSpeed = 18;
        }
        else if (level == 4)
        {
            scatterModeTime1 = 5.5f;
            scatterModeTime2 = 3.5f;
            chaseModeTime1 = 26;
            chaseModeTime2 = 28;

            frightenedModeTime = 7;
            blinkAt = 4;

            pinkyStartTime = 3.5f;
            inkyStartTime = 8;
            clydeStartTime = 12;

            speed = 8.9f;
            defaultSpeed = 8.9f;
            frightenedSpeed = 5.5f;
            consumedSpeed = 20;
        }
        else if (level == 5)
        {
            scatterModeTime1 = 5;
            scatterModeTime2 = 3;
            chaseModeTime1 = 28;
            chaseModeTime2 = 30;

            frightenedModeTime = 6;
            blinkAt = 3;

            pinkyStartTime = 3;
            inkyStartTime = 7;
            clydeStartTime = 10;

            speed = 9.9f;
            defaultSpeed = 9.9f;
            frightenedSpeed = 5.9f;
            consumedSpeed = 22;
        }
        else if (level == 6)
        {
            scatterModeTime1 = 4.5f;
            scatterModeTime2 = 2.5f;
            chaseModeTime1 = 30;
            chaseModeTime2 = 32;

            frightenedModeTime = 5;
            blinkAt = 2;

            pinkyStartTime = 2.5f;
            inkyStartTime = 5;
            clydeStartTime = 8;

            speed = 10.9f;
            defaultSpeed = 10.9f;
            frightenedSpeed = 6.5f;
            consumedSpeed = 24;
        }
        else if (level >= 7)
        {
            scatterModeTime1 = 4;
            scatterModeTime2 = 2;
            chaseModeTime1 = 32;
            chaseModeTime2 = 34;

            frightenedModeTime = 5;
            blinkAt = 2;

            pinkyStartTime = 2;
            inkyStartTime = 4;
            clydeStartTime = 6;

            speed = 11.9f;
            defaultSpeed = 11.9f;
            frightenedSpeed = 6.9f;
            consumedSpeed = 26;
        }
    }

    // restarts the game upon collision
    public void Restart()
    {
        canMove = true; // allow the ghosts to move again upon restarting
        transform.position = start.transform.position;
        transform.GetComponent<SpriteRenderer>().enabled = true; // display the ghosts again

        modeTime = 0;
        modeIndex = 1;
        currMode = Mode.Scatter; // in case if pacman dies during frightened mode
        speed = defaultSpeed; // same thing if dies during frightened mode, don't want the speed to be hyper mode
        prevSpeed = 0; // set to normal
        ghostStartTime = 0;

        // except for blinky, all in ghost house by default
        if (ghostType != GhostType.Blinky)
        {
            inGhosthouse = true;
        }

        currVertex = start;
        prevVertex = currVertex;
        // initialise the ghosts movement out of the ghosthouse
        if (inGhosthouse)
        {
            // when in ghost house, inky has to move right first
            if (ghostType == GhostType.Inky)
            {
                dir = Vector2.right;
            }
            else if (ghostType == GhostType.Clyde) // clyde has to move left first
            {
                dir = Vector2.left;
            }
            else // pinky has to move up first
            {
                dir = Vector2.up;
            }
            endVertex = currVertex.vertices[0]; // only one vertex to choose to move to
            // GetComponent<Animator>().enabled = false;
        }
        else
        {
            dir = Vector2.left;
            nextDir = Vector2.zero; // default for ghost to always move left for blinky
            endVertex = FindNearestVertex();
        }
        UpdateGhostAnimation();
    }

    // Update is called once per frame
    void Update()
    {


        // only if can move then check for these updates
        if (canMove)
        {
            if (isHuman && photonView.IsMine)
            {
                if (currMode == Mode.Consumed)
                {
                    // code controlled
                    AutoReturn();
                }
                else
                {
                    // all controlled by humans
                    UpdateMode();
                    CheckInput();
                    MoveGhost();
                    ReleaseGhosts();
                    DetectCollision();
                    UpdateGhostAnimation();
                }
            }
            else
            {
                UpdateMode(); // constantly change mode
                MoveAndChasePacman(); // constantly chase pacman
                ReleaseGhosts(); // constantly check when to release more
                DetectCollision(); // constantly check if pacman collides with the ghosts
                hasReturnedToStart(); // check whether consumed ghost has returned to the ghosthouse
            }
        }
    }

    // all human controlled actions similar to pacman

    // helper function to allow ghost to run back into ghosthouse without human intervention
    void AutoReturn()
    {
        // in case ghost is caught while motionless, set the next possible vertex and dir to move to
        if (currVertex == endVertex)
        {
            dir = currVertex.validDir[0];
            endVertex = currVertex.vertices[0];
        }
        UpdateMode();
        UpdateGhostAnimation();
        MoveAndChasePacman();
        hasReturnedToStart();
    }
    void CheckInput()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            ChangePosition(Vector2.left);
            // dir = Vector2.left;
            // MoveToNextVertex(dir);
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            ChangePosition(Vector2.right);
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            ChangePosition(Vector2.down);

        }
        else if (Input.GetKeyDown(KeyCode.W))
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

    void MoveGhost()
    {
        // no null pointer nonsense
        if (endVertex != null && endVertex != currVertex && !inGhosthouse)
        {
            // dont allow the ghost to reverse (give pacman some handicap)
            // if (nextDir == dir * -1)
            // {
            //     dir = -dir;
            //     // swap the prev and end vertices
            //     Vertex temp = prevVertex;
            //     prevVertex = endVertex;
            //     endVertex = temp;
            // }


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

    // iterate through all neighbours of current vertex and take next vertex if possible
    Vertex CanMove(Vector2 dir)
    {
        Vertex nextVertex = null;
        for (int i = 0; i < currVertex.vertices.Length; i++)
        {
            // to prevent the ghost from entering back into the ghosthouse except for consumed mode
            if (currVertex.GetComponent<Tile>().isEntrance && dir == Vector2.down)
            {
                continue;
            }
            // standard movement
            if (currVertex.validDir[i] == dir)
            {
                nextVertex = currVertex.vertices[i];
                break;
            }
        }
        return nextVertex;
    }

    // back to ghost methods
    // main driver function to check whether the ghost has gone back to the ghosthouse after being consumed
    void hasReturnedToStart()
    {
        if (currMode == Mode.Consumed)
        {
            GameObject tile = GetTilePos(transform.position);
            if (tile != null && tile.GetComponent<Tile>() != null)
            { // prevent any null pointer exception
                if (tile.GetComponent<Tile>().isGhosthouse)
                {
                    // reset the speed
                    speed = defaultSpeed;
                    // reset all the curr, prev and end vertices (should be back at ghost house already)
                    Vertex vertex = GetVertexPos(transform.position);
                    if (vertex != null)
                    {
                        currVertex = vertex;
                        prevVertex = currVertex;
                        endVertex = currVertex.vertices[0]; // only neighbour
                        dir = Vector2.up; // only direction up out of the ghosthouse
                        // special attribute for human ghosts only
                        nextDir = Vector2.up;
                        currMode = Mode.Chase; // set back to chase mode again
                        UpdateGhostAnimation(); // update the ghost animation again immediately
                    }
                }
            }
        }
    }

    // main driver function to detect collisions between pacman and the ghosts
    void DetectCollision()
    {
        Rect pacmanRect = new Rect(pacman.transform.position,
                                pacman.transform.GetComponent<SpriteRenderer>().sprite.bounds.size / 4);
        Rect ghostRect = new Rect(transform.position,
                                transform.GetComponent<SpriteRenderer>().sprite.bounds.size / 4);
        if (ghostRect.Overlaps(pacmanRect))
        {
            if (currMode == Mode.Frightened)
            {
                InitiateConsumedMode();
            }
            else
            {
                // don't want the game to restart when in consumed mode
                if (currMode != Mode.Consumed)
                {
                    gameManager.GetComponent<GameManager>().StartDeath(); // call from game manager
                }
            }
            Debug.Log("Collided at " + transform.localPosition);
        }
    }

    // main driver function to constantly change the mode based on the timer
    void UpdateMode()
    {
        // frightened mode vs chase + scatter
        if (currMode != Mode.Frightened)
        {
            // constantly add the time
            modeTime += Time.deltaTime; // unit of time
            if (modeIndex == 1)
            {
                if (currMode == Mode.Scatter && modeTime > scatterModeTime1)
                {
                    ChangeMode(Mode.Chase);
                    modeTime = 0;
                }

                if (currMode == Mode.Chase && modeTime > chaseModeTime1)
                {
                    ChangeMode(Mode.Scatter);
                    modeIndex = 2;
                    modeTime = 0;
                }
            }
            else if (modeIndex == 2)
            {
                if (currMode == Mode.Scatter && modeTime > scatterModeTime2)
                {
                    ChangeMode(Mode.Chase);
                    modeTime = 0;
                }
                // dk yet
                if (currMode == Mode.Chase && modeTime > chaseModeTime2)
                {
                    ChangeMode(Mode.Scatter);
                    modeIndex = 1;
                    modeTime = 0;
                }
            }
        }
        else if (currMode == Mode.Frightened)
        {
            frightTime += Time.deltaTime;
            if (frightTime >= frightenedModeTime)
            {
                // while changing audio, need to tell unity to play
                backgroundMusic.clip = gameManager.GetComponent<GameManager>().normalMusic;
                backgroundMusic.Play();

                frightTime = 0;
                ChangeMode(prevMode); // recall previous mode to go back to (scatter/chase)
            }

            // check when to blink
            if (frightTime >= blinkAt)
            {
                blinkTime += Time.deltaTime;
                if (blinkTime >= blinkInterval)
                {

                    blinkTime = 0f;
                    if (atWhite)
                    {
                        transform.GetComponent<Animator>().runtimeAnimatorController = ghostBlue;
                        atWhite = false;
                    }
                    else
                    {
                        transform.GetComponent<Animator>().runtimeAnimatorController = ghostWhite;
                        atWhite = true;
                    }
                }
            }
        }
    }

    // simple function to modify the current mode
    void ChangeMode(Mode mode)
    {
        // while in frightened mode, if another power pellet is eaten, update speed accordingly first so that speed does not 
        // stay at 3.0...
        if (currMode == Mode.Frightened)
        {
            speed = prevSpeed;
        }

        if (mode == Mode.Frightened)
        {
            prevSpeed = speed;
            speed = frightenedSpeed;
        }

        // this only occurs if frightened mode does not repeat itself (eating another power pellet before duration ends)
        if (currMode != mode)
        {
            // for storing the prev mode and replacing the curr mode to be the new mode
            prevMode = currMode;
            currMode = mode;
        }

        // remember to update ghost animation or else it will only update when they start moving
        UpdateGhostAnimation();
    }

    // public method for pacman to access it
    public void InitiateFrightenedMode()
    {
        // if during consumed mode, don't turn back into frightened mode when power pellet is consumed
        if (currMode != Mode.Consumed)
        {
            // set initial ghost consumed score
            GameManager.ghostConsumedScore = 200;
            // while changing audio, need to tell unity to play
            backgroundMusic.clip = gameManager.GetComponent<GameManager>().powerMusic;
            backgroundMusic.Play();
            frightTime = 0; // remember to reset since more power pellets can be eaten before frightened mode ends
            ChangeMode(Mode.Frightened);
        }
    }

    public void InitiateConsumedMode()
    {
        // gameManager.GetComponent<GameManager>().playerScore += 200; // add 200 points for ghost consumed
        // GameManager.playerScore += 200;
        GameManager.playerScore += GameManager.ghostConsumedScore;
        currMode = Mode.Consumed;
        prevSpeed = speed;
        speed = consumedSpeed;
        UpdateGhostAnimation(); // so that the eyes immediately update instead of only when move is activated
        gameManager.GetComponent<GameManager>().StartConsume(this.GetComponent<Ghost>()); // start the consume animation
        GameManager.ghostConsumedScore *= 2; // multiply the next ghost consumed score by 2
    }

    // for animating the ghosts (when ghosts reached the end vertex, it then changes animation if needed)
    // when in frightened mode, the direction should not matter anymore
    // when being consumed, the sprites of the eyes should be activated
    void UpdateGhostAnimation()
    {
        if (currMode != Mode.Frightened && currMode != Mode.Consumed)
        {
            if (dir == Vector2.left)
            {
                transform.GetComponent<Animator>().runtimeAnimatorController = ghostLeft;
            }
            else if (dir == Vector2.right)
            {
                transform.GetComponent<Animator>().runtimeAnimatorController = ghostRight;
            }
            else if (dir == Vector2.down)
            {
                transform.GetComponent<Animator>().runtimeAnimatorController = ghostDown;
            }
            else if (dir == Vector2.up)
            {
                transform.GetComponent<Animator>().runtimeAnimatorController = ghostUp;
            }
            else
            {
                // by default look left if not moving
                transform.GetComponent<Animator>().runtimeAnimatorController = ghostLeft;
            }
        }
        else if (currMode == Mode.Frightened)
        {
            transform.GetComponent<Animator>().runtimeAnimatorController = ghostRainbow;
        }
        else if (currMode == Mode.Consumed)
        {
            transform.GetComponent<Animator>().runtimeAnimatorController = null;
            if (dir == Vector2.left)
            {
                transform.GetComponent<SpriteRenderer>().sprite = eyesLeft;
            }
            else if (dir == Vector2.right)
            {
                transform.GetComponent<SpriteRenderer>().sprite = eyesRight;

            }
            else if (dir == Vector2.down)
            {
                transform.GetComponent<SpriteRenderer>().sprite = eyesDown;

            }
            else if (dir == Vector2.up)
            {
                transform.GetComponent<SpriteRenderer>().sprite = eyesUp;

            }
        }
    }

    void MoveAndChasePacman()
    {
        // the ghosts should not move inside the ghosthouse
        if (endVertex != currVertex && endVertex != null && !inGhosthouse)
        {
            if (ExceedEndpoint())
            {
                currVertex = endVertex;
                transform.localPosition = currVertex.transform.position; // global position

                // search for possible gateways to teleport
                CheckForGateway();
                // replace end vertex accordingly
                endVertex = FindNearestVertex();
                prevVertex = currVertex;
                currVertex = null;

                UpdateGhostAnimation();
            }
            else
            {
                // if end vertex is not reached, continue moving towards it
                transform.localPosition += (Vector3)(dir * speed) * Time.deltaTime;
            }
        }
    }

    // helper functions to release the ghosts from ghosthouse
    void ReleasePinky()
    {
        if (ghostType == GhostType.Pinky && inGhosthouse)
        {
            inGhosthouse = false;
            // GetComponent<Animator>().enabled = true;
        }
    }

    void ReleaseInky()
    {
        if (ghostType == GhostType.Inky && inGhosthouse)
        {
            inGhosthouse = false;
            // GetComponent<Animator>().enabled = true;
        }
    }

    void ReleaseClyde()
    {
        if (ghostType == GhostType.Clyde && inGhosthouse)
        {
            inGhosthouse = false;
            // GetComponent<Animator>().enabled = true;
        }
    }

    void ReleaseGhosts()
    {
        ghostStartTime += Time.deltaTime;
        if (ghostStartTime > pinkyStartTime)
        {
            ReleasePinky();
        }
        if (ghostStartTime > inkyStartTime)
        {
            ReleaseInky();
        }
        if (ghostStartTime > clydeStartTime)
        {
            ReleaseClyde();
        }
    }

    // helper functions to get the chase target tile 
    Vector2 GetBlinkyTarget()
    {
        // chase pacman directly
        Vector2 targetPos = pacman.transform.localPosition;
        return new Vector2((int)targetPos.x, (int)targetPos.y);
    }

    Vector2 GetPinkyTarget()
    {
        // chase 4 tiles ahead of pacman (take note of the orientation)
        Vector2 targetPos = pacman.transform.localPosition;
        Vector2 targetOrientation = pacman.GetComponent<PacMan>().orientation;

        return new Vector2((int)targetPos.x, (int)targetPos.y) + 4 * targetOrientation; // 4 tiles ahead for unit vectors
    }

    Vector2 GetInkyTarget()
    {
        // based on actual pacman, it selects the position two tiles ahead of pacman
        // take the vector from the curr blinky position to that tile and double the distance

        // first get 2 tiles ahead of pacman (while taking note of the orientation)
        Vector2 tempPos = pacman.transform.localPosition;
        Vector2 targetOrientation = pacman.GetComponent<PacMan>().orientation;

        Vector2 targetPos = new Vector2((int)tempPos.x, (int)tempPos.y) + 2 * targetOrientation; // 2 tiles ahead for unit vectors

        // calculate vector from curr blinky position
        Vector2 blinkyPos = GameObject.Find("ghost(Clone)").transform.localPosition;
        blinkyPos = new Vector2((int)blinkyPos.x, (int)blinkyPos.y);

        // double the distance
        float distance = CalcDistance(blinkyPos, targetPos) * 2;
        return new Vector2(blinkyPos.x + distance, blinkyPos.y + distance);
    }

    Vector2 GetClydeTarget()
    {
        // based on actual pacman, if dist is greater than 8 tiles, same targeting as blinky
        // if fewer than 8 tiles, target is to bottom left, his scatter vertex

        Vector2 tempPos = pacman.transform.localPosition; // pacman pos
        float distance = CalcDistance(tempPos, transform.localPosition);
        Vector2 targetPos = Vector2.zero;
        if (distance > 8)
        {
            targetPos = new Vector2((int)tempPos.x, (int)tempPos.y);
        }
        else
        {
            targetPos = scatterVertex.transform.position; // global position
        }
        return targetPos;
    }

    // get the target position according to the ghost type
    Vector2 GetTarget()
    {
        Vector2 target = Vector2.zero;
        if (ghostType == GhostType.Blinky)
        {
            target = GetBlinkyTarget();
        }
        else if (ghostType == GhostType.Pinky)
        {
            target = GetPinkyTarget();
        }
        else if (ghostType == GhostType.Inky)
        {
            target = GetInkyTarget();
        }
        else if (ghostType == GhostType.Clyde)
        {
            target = GetClydeTarget();
        }
        return target;
    }

    // get random target position during frightened mode (or should it just run away from pacman?)
    Vector2 GetRandomTarget()
    {
        return new Vector2(Random.Range(0, 28), Random.Range(0, 36)); // width and height of the gameboard
    }

    // main driver function to chase the pacman since the ghost has no keycode input to take in
    Vertex FindNearestVertex()
    {
        // Vector2 targetPos = pacman.transform.localPosition;
        // Vector2 chaseTarget = new Vector2((int)targetPos.x, (int)targetPos.y);
        Vector2 chaseTarget = Vector2.zero;

        if (currMode == Mode.Chase)
        {
            chaseTarget = GetTarget();
        }
        else if (currMode == Mode.Scatter)
        {
            chaseTarget = scatterVertex.transform.position; // global position
        }
        else if (currMode == Mode.Frightened)
        {
            chaseTarget = GetRandomTarget();
        }
        else if (currMode == Mode.Consumed)
        {
            chaseTarget = ghostHouse.transform.position; // to return to the ghosthouse
        }

        Vertex nearestVertex = null; // initialise first
        Vertex[] possibleVertices = new Vertex[4];
        Vector2[] possibleDirections = new Vector2[4];
        int count = 0; // to count the number of vertices found

        // account for dead ends
        if (currVertex.vertices.Length == 1)
        {
            dir = currVertex.validDir[0];
            return currVertex.vertices[0];
        }

        // iterate to find all possible vertices to travel to first
        for (int i = 0; i < currVertex.vertices.Length; i++)
        {
            // dont allow the ghosts to reverse unnecessarily
            if (currVertex.validDir[i] != -dir)
            {
                // things get complex when movement is allowed from ghosthouse entrance into the ghosthouse
                if (currMode != Mode.Consumed)
                {
                    GameObject tile = GetTilePos(currVertex.transform.position);
                    if (tile.GetComponent<Tile>().isEntrance)
                    {
                        if (currVertex.validDir[i] != Vector2.down)
                        {
                            // other than consumed mode, when at entrance, don't allow movement down into the ghosthouse
                            possibleVertices[count] = currVertex.vertices[i];
                            possibleDirections[count] = currVertex.validDir[i];
                            count++;
                        }
                    }
                    else
                    {
                        possibleVertices[count] = currVertex.vertices[i];
                        possibleDirections[count] = currVertex.validDir[i];
                        count++;
                    }
                }
                else
                {
                    possibleVertices[count] = currVertex.vertices[i];
                    possibleDirections[count] = currVertex.validDir[i];
                    count++;

                }
            }
        }

        // find nearest vertex to travel to
        if (count == 1)
        {
            nearestVertex = possibleVertices[0];
            dir = possibleDirections[0];
        }
        else
        {
            // more than 1 vertex found
            float smallest = float.PositiveInfinity;
            for (int i = 0; i < possibleVertices.Length; i++)
            {
                // avoid zero vectors inside
                if (possibleDirections[i] != Vector2.zero)
                {
                    float temp = CalcDistance(possibleVertices[i].transform.position, chaseTarget);
                    if (temp < smallest)
                    {
                        smallest = temp;
                        nearestVertex = possibleVertices[i];
                        dir = possibleDirections[i];
                    }
                }
            }
        }
        return nearestVertex;
    }

    // additional method to calc distance between two vector pos to determine nearest distance
    float CalcDistance(Vector2 pos1, Vector2 pos2)
    {
        float diffX = pos1.x - pos2.x;
        float diffY = pos1.y - pos2.y;
        return Mathf.Sqrt(diffX * diffX + diffY * diffY);
    }

    // below contains similar methods as Pacman
    float CalculateDistanceFromVertex(Vector2 to)
    {
        // this is from the current vertex
        Vector2 vector = to - (Vector2)prevVertex.transform.position; // global position
        return vector.sqrMagnitude;
    }

    bool ExceedEndpoint()
    {
        float toCurrPoint = CalculateDistanceFromVertex((Vector2)transform.localPosition);
        float toEndpoint = CalculateDistanceFromVertex((Vector2)endVertex.transform.position);
        return toCurrPoint > toEndpoint;
    }

    // obtain any possible gateway (similar to Pacman code)
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
        GameObject otherGateway = ObtainGateway(currVertex.transform.position);
        if (otherGateway != null)
        {
            transform.localPosition = otherGateway.transform.position; // teleport to the global position
            currVertex = otherGateway.GetComponent<Vertex>();
        }
    }

    // get the curr vertex position similar to Pacman script
    Vertex GetVertexPos(Vector2 pos)
    {
        GameObject tile = gameManager.GetComponent<GameManager>().board[(int)pos.x, (int)pos.y];
        if (tile != null)
        {
            return tile.GetComponent<Vertex>(); // will return null if not a vertex to begin with
        }
        return null;
    }

    // get the curr tile position (gameobject) if possible similar to Pacman script
    GameObject GetTilePos(Vector2 pos)
    {
        GameObject tile = gameManager.GetComponent<GameManager>().board[(int)pos.x, (int)pos.y];
        if (tile != null)
        {
            return tile;
        }
        return null;
    }
}
