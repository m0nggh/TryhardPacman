using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviourPunCallbacks
{
    [Header("Panels and UI")]
    [SerializeField] private GameObject findOpponentPanel = null;
    [SerializeField] private GameObject waitingStatusPanel = null;
    [SerializeField] private GameObject difficultySelectPanel = null;
    [SerializeField] private Text waitingStatusText = null;
    [SerializeField] private GameObject currentPacman = null;

    [Header("Images")]
    [SerializeField] private GameObject[] levelImages = null;
    [SerializeField] private Sprite[] pacmans = null;
    private bool isConnecting = false;
    private const string GameVersion = "0.1";
    private int LevelChosen = 1;

    // default value for the game, can be changed by player during selection scene
    private byte MaxPlayersPerRoom = 2;

    private void Awake() => PhotonNetwork.AutomaticallySyncScene = true;

    public void GoToMainMenu()
    {
        SceneManager.LoadScene("StartMenu2");
    }

    // to be enabled by buttons to determine play mode (change if you have anything more efficient lmao)
    public void SinglePlayer()
    {
        MaxPlayersPerRoom = 1;
        PhotonNetwork.OfflineMode = true;
    }
    public void TwoPlayers()
    {
        MaxPlayersPerRoom = 2;
    }
    public void ThreePlayers()
    {
        MaxPlayersPerRoom = 3;
    }
    public void FourPlayers()
    {
        MaxPlayersPerRoom = 4;
    }
    public void FivePlayers()
    {
        MaxPlayersPerRoom = 5;
    }

    // to store preferred difficulty (for single player)
    public void Easy()
    {
        PlayerPrefs.SetInt("DifficultyLevel", 1);
    }
    public void Medium()
    {
        PlayerPrefs.SetInt("DifficultyLevel", 2);
    }
    public void Hard()
    {
        PlayerPrefs.SetInt("DifficultyLevel", 3);
    }

    public void GoLeft()
    {

        // only master client can click on the button
        if (!PhotonNetwork.IsMasterClient)
        {
            return;
        }

        for (int i = 0; i < levelImages.Length; i++)
        {
            if (levelImages[i].activeInHierarchy)
            {
                levelImages[i].SetActive(false);
                if (i == 0)
                {
                    levelImages[levelImages.Length - 1].SetActive(true);
                    LevelChosen = levelImages.Length;
                }
                else
                {
                    levelImages[i - 1].SetActive(true);
                    LevelChosen = i;
                }
                Debug.Log("Level chosen now is " + LevelChosen);
                break;
            }
        }
    }

    public void GoRight()
    {

        // only master client can click on the button
        if (!PhotonNetwork.IsMasterClient)
        {
            return;
        }

        for (int i = 0; i < levelImages.Length; i++)
        {
            if (levelImages[i].activeInHierarchy)
            {
                levelImages[i].SetActive(false);
                if (i == levelImages.Length - 1)
                {
                    levelImages[0].SetActive(true);
                    LevelChosen = 0;
                }
                else
                {
                    levelImages[i + 1].SetActive(true);
                    LevelChosen = i + 2;
                }
                Debug.Log("Level chosen now is " + LevelChosen);
                break;
            }
        }
    }

    public void EnterGame()
    {
        string level = "Level" + LevelChosen;
        PhotonNetwork.LoadLevel(level);
    }

    // enable by button
    public void FindOpponent()
    {
        isConnecting = true;

        findOpponentPanel.SetActive(false);
        waitingStatusPanel.SetActive(true);

        waitingStatusText.text = "Searching...";

        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.JoinRandomRoom();
        }
        else
        {
            PhotonNetwork.GameVersion = GameVersion;
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
        SceneManager.LoadScene("StartMenu2");
    }

    public void ActualLeaveRoom() => PhotonNetwork.LeaveRoom();

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected To Master");

        if (isConnecting)
        {
            PhotonNetwork.JoinRandomRoom();
        }
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        waitingStatusPanel.SetActive(false);
        findOpponentPanel.SetActive(true);

        Debug.Log($"Disconnected due to: {cause}");
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("No clients are waiting for an opponent, creating a new room");

        PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = MaxPlayersPerRoom });
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Client successfully joined a room");
        PlayerPrefs.SetInt("TotalPlayers", MaxPlayersPerRoom);

        int playerCount = PhotonNetwork.CurrentRoom.PlayerCount;
        PhotonNetwork.LocalPlayer.NickName = "" + playerCount;

        currentPacman.SetActive(true);
        currentPacman.GetComponent<Image>().sprite = pacmans[playerCount - 1]; 

        if (playerCount != MaxPlayersPerRoom)
        {
            waitingStatusText.text = "Waiting For Opponent";
            Debug.Log("Client is waiting for an opponent");
        }
        else
        {
            waitingStatusText.text = "Opponent Found";
            Debug.Log("Match is ready to begin");

            // if single player mode, just load the level selection
            if (MaxPlayersPerRoom == 1)
            {
                waitingStatusPanel.SetActive(false);
                difficultySelectPanel.SetActive(true);
            }
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        if (PhotonNetwork.CurrentRoom.PlayerCount == MaxPlayersPerRoom)
        {
            PhotonNetwork.CurrentRoom.IsOpen = false;

            waitingStatusText.text = "Opponent Found";
            Debug.Log("Match is ready to begin");

            if (PhotonNetwork.IsMasterClient) {
                PhotonNetwork.LoadLevel("MultiplayerLevel");
            }
            
        }
    }

}
