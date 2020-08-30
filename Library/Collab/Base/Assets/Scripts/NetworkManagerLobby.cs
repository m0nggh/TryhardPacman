using Mirror;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkManagerLobby : NetworkManager
{
    private int minPlayer = 1;

    [Header("Room")]
    public PacManLobbyPlayer roomPlayerPrefab;

    [Header("Game")]
    public PacManPlayer pacManPrefab;

    public static event Action OnClientConnected;
    public static event Action OnClientDisconnected;
    public static event Action<NetworkConnection> OnServerReadied; 

    public List<PacManLobbyPlayer> RoomPlayers { get; } = new List<PacManLobbyPlayer>();

    public List<PacManPlayer> GamePlayers { get; } = new List<PacManPlayer>();

    public override void OnStartServer() => spawnPrefabs = Resources.LoadAll<GameObject>("SpawnablePrefabs").ToList();

    public override void OnStartClient() {
        var spawnablePrefabs = Resources.LoadAll<GameObject>("SpawnablePrefabs");

        foreach (var prefab in spawnablePrefabs) {
            ClientScene.RegisterPrefab(prefab);
        } 
    }

    public override void OnClientConnect(NetworkConnection conn) {
        base.OnClientConnect(conn);

        OnClientConnected?.Invoke();
    }

    public override void OnClientDisconnect(NetworkConnection conn) {
        base.OnClientDisconnect(conn);

        OnClientDisconnected?.Invoke();
    }

    public override void OnServerConnect(NetworkConnection conn) {
        if (numPlayers >= maxConnections) {
            conn.Disconnect();
            return;
        }

        if (SceneManager.GetActiveScene().name != "GameLobby") {
            conn.Disconnect();
            return;
        }
    }

    public override void OnServerDisconnect(NetworkConnection conn) {
        if (conn.identity != null) {
            var player = conn.identity.GetComponent<PacManLobbyPlayer>();
            
            RoomPlayers.Remove(player);

            NotifyPlayersOfReadyState();
        }

        base.OnServerDisconnect(conn);
    }

    public override void OnServerAddPlayer(NetworkConnection conn) {
        // allow for addition of players if game has not yet proceeded i.e. still in "GameLobby"
        if (SceneManager.GetActiveScene().name == "GameLobby") {

            bool isLeader = RoomPlayers.Count == 0;
            
            PacManLobbyPlayer roomPlayerInstance = Instantiate(roomPlayerPrefab);

            roomPlayerInstance.IsLeader = isLeader;

            NetworkServer.AddPlayerForConnection(conn, roomPlayerInstance.gameObject);
        }
    }

    public override void OnStopServer() {
        RoomPlayers.Clear();
    }

    public void NotifyPlayersOfReadyState() {
        foreach (var player in RoomPlayers) {
            player.HandleReadyToStart(IsReadyToStart());
        }
    }

    private bool IsReadyToStart() {
        if (numPlayers < minPlayer) { return false; }
        foreach (var player in RoomPlayers) {
            if (!player.IsReady) { return false; }
        }

        return true;
    }

    public void StartGame() {
        if (SceneManager.GetActiveScene().name == "GameLobby") {
            if (!IsReadyToStart()) {
                return;
            }
            ServerChangeScene("level1");
        }
    }

    public override void ServerChangeScene(string newSceneName) {
        if (SceneManager.GetActiveScene().name == "GameLobby" && newSceneName.StartsWith("level")) {
            for (int i = RoomPlayers.Count - 1; i >= 0; i--) {
                var conn = RoomPlayers[i].connectionToClient;

                var gamePlayerInstance = Instantiate(pacManPrefab);
                gamePlayerInstance.SetDisplayName(RoomPlayers[i].DisplayName);

                // destroy the Lobby player(s)
                NetworkServer.Destroy(conn.identity.gameObject);

                // use same connection for game player(s)
                NetworkServer.ReplacePlayerForConnection(conn, gamePlayerInstance.gameObject);
            }
        }
        base.ServerChangeScene(newSceneName);
    }

    public override void OnServerReady(NetworkConnection conn) {
        base.OnServerReady(conn);

        OnServerReadied?.Invoke(conn);
    }

}
