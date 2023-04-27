/*
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using It4080;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class Lobby : NetworkBehaviour
{
    public It4080.ConnectedPlayers connectedPlayers;
    public Button btnStart;
    public TMPro.TMP_Text txtKicked;

    void Start()
    {
        initialClear();
        txtKicked.gameObject.SetActive(false);
    }


    public override void OnNetworkSpawn()
    {
        initialClear();

        // Clients watch the allPlayers NetworkList for changes and update their
        // display appropriately.  The Server does not have to explicitly call
        // a client rpc to notify the clients of changes to the list of connected
        // players.
        if (IsClient)
        {
            NetworkHandler.Singleton.allPlayers.OnListChanged += ClientOnAllPlayersChanged;
            PopulateConnectedPlayersUsingPlayerDataList(NetworkHandler.Singleton.allPlayers);

            if (!IsHost)
            {
                NetworkManager.OnClientDisconnectCallback += ClientOnDisconnect;
            }
        }

        if (IsServer)
        {
            NetworkHandler.Singleton.allPlayers.OnListChanged += LogNetworkListEvent;
            btnStart.onClick.AddListener(OnBtnStartClicked);
        }

        btnStart.gameObject.SetActive(IsHost);
    }


    private void ClientOnDisconnect(ulong clientId)
    {
        txtKicked.gameObject.SetActive(true);
        connectedPlayers.gameObject.SetActive(false);
    }


    private void ClientOnAllPlayersChanged(NetworkListEvent<It4080.PlayerData> changeEvent)
    {
        PopulateConnectedPlayersUsingPlayerDataList(
            NetworkHandler.Singleton.allPlayers);

        if (IsHost)
        {
            EnableStartIfAllReady();
        }
    }


    private void LogNetworkListEvent(NetworkListEvent<It4080.PlayerData> changeEvent)
    {
        Debug.Log($"Player data changed:");
        Debug.Log($"    Change Type:  {changeEvent.Type}");
        Debug.Log($"    Value:        {changeEvent.Value}");
        Debug.Log($"        {changeEvent.Value.clientId}");
        Debug.Log($"        {changeEvent.Value.isReady}");
        Debug.Log($"    Prev Value:   {changeEvent.PreviousValue}");
        Debug.Log($"        {changeEvent.PreviousValue.clientId}");
        Debug.Log($"        {changeEvent.PreviousValue.isReady}");
    }


    private bool hasClearedInitially = false;
    private void initialClear()
    {
        if (!hasClearedInitially)
        {
            connectedPlayers.Clear();
            hasClearedInitially = true;
        }
    }


    private void EnableStartIfAllReady()
    {
        int readyCount = 0;
        foreach (It4080.PlayerData readyInfo in NetworkHandler.Singleton.allPlayers)
        {
            if (readyInfo.isReady)
            {
                readyCount += 1;
            }
        }

        btnStart.enabled = readyCount == NetworkHandler.Singleton.allPlayers.Count;
        if (btnStart.enabled)
        {
            btnStart.GetComponentInChildren<TextMeshProUGUI>().text = "Start Game";
        }
        else
        {
            btnStart.GetComponentInChildren<TextMeshProUGUI>().text = "<Waiting for Ready>";
        }
    }


    private PlayerCard AddPlayerCard(ulong clientId)
    {
        It4080.PlayerCard newCard = connectedPlayers.AddPlayer("temp", clientId);
        string you = "";
        string what = "";

        newCard.ShowKick(IsServer);
        if (IsServer)
        {
            newCard.KickPlayer += ServerOnKickButtonPressed;
        }

        if (clientId == NetworkManager.LocalClientId)
        {
            you = "(you)";
            newCard.ShowReady(true);
            newCard.ReadyToggled += ClientOnMyReadyToggled;
        }
        else
        {
            you = "";
            newCard.ShowReady(false);
        }

        if (clientId == NetworkManager.ServerClientId)
        {
            what = "Host";
            newCard.ShowKick(false);
            newCard.ShowReady(false);
        }
        else
        {
            what = "Player";
        }

        newCard.SetPlayerName($"{what} {clientId}{you}");
        return newCard;
    }


    private void ServerOnKickButtonPressed(ulong clientId)
    {
        Debug.Log($"Kicking {clientId}");
        NetworkManager.DisconnectClient(clientId);
        //NetworkHandler.Singleton.RemovePlayerFromList(clientId);
    }


    private void OnBtnStartClicked()
    {
        StartGame();
    }


    private void ClientOnMyReadyToggled(bool isReady)
    {
        RequestSetReadyServerRpc(isReady);
    }


    private void PopulateConnectedPlayersUsingPlayerDataList(NetworkList<It4080.PlayerData> players)
    {
        connectedPlayers.Clear();

        foreach (It4080.PlayerData p in players)
        {
            var card = AddPlayerCard(p.clientId);
            card.SetReady(p.isReady);
            string status = "Not Ready";
            if (p.isReady)
            {
                status = "READY!!";
            }
            card.SetStatus(status);
        }
    }


    // ---------------------
    // Public
    // ---------------------
    private void StartGame()
    {
        NetworkManager.SceneManager.LoadScene("Arena1",
            UnityEngine.SceneManagement.LoadSceneMode.Single);
    }


    [ServerRpc(RequireOwnership = false)]
    public void RequestSetReadyServerRpc(bool isReady, ServerRpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;
        int playerIndex = NetworkHandler.Singleton.FindPlayerIndex(clientId);
        It4080.PlayerData info = NetworkHandler.Singleton.allPlayers[playerIndex];

        info.isReady = isReady;
        NetworkHandler.Singleton.allPlayers[playerIndex] = info;
    }
}
*/




using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using TMPro;

public class Lobby : NetworkBehaviour
{
    public It4080.ConnectedPlayers connectedPlayers;

    private Button btnStart;

    private void Start()
    {
        btnStart = GameObject.Find("start").GetComponent<Button>();

        btnStart.onClick.AddListener(btnStartOnClick);

        btnStart.gameObject.SetActive(IsHost);
    }


    public override void OnNetworkSpawn()
    {
        InitialClear();
        if (IsClient)
        {

            NetworkHandler.Singleton.allPlayers.OnListChanged += ClientOnAllPlayersChanged;

            AddPlayersUsingPlayerData(NetworkHandler.Singleton.allPlayers);

            if (IsHost)
            {
                NetworkManager.OnClientDisconnectCallback += ClientOnDisconnect;
            }

        }
       

    }

    public void InitialClear()
    {
        connectedPlayers.Clear();

    }

    public void AddPlayersUsingPlayerData(NetworkList<It4080.PlayerData> players)
    {
        connectedPlayers.Clear();
        
        foreach (It4080.PlayerData player in players)
        {
            var card = AddPlayerCard(player.clientId);

            card.SetReady(player.isReady);

            string status = "Not Ready";

            if (player.isReady)
            {
                status = "Ready";
            }
            card.SetStatus(status);

            // Connect to the ReadyToggled event on the PlayerCard if we are
            // the player represented by the card we are creating.
            if (player.clientId == NetworkManager.Singleton.LocalClientId)
            {
                card.ReadyToggled += ClientOnReadyToggled;
            }

            // If we are the host then we can connect to the KickPlayer event
            // on the PlayerCard.  This allows us to then kick the player when
            // the button is pressed.
            if (IsHost)
            {
                card.KickPlayer += HostKickPlayer;
            }
        }
    }


    private void HostKickPlayer(ulong clientId)
    {
        Debug.Log($"Get outta here {clientId}");
    }

    private It4080.PlayerCard AddPlayerCard(ulong clientId)
    {
        It4080.PlayerCard card = connectedPlayers.AddPlayer("temp", clientId);

        string ready = "";

        string type = "";

        card.ShowKick(IsServer);

        if (clientId == NetworkManager.LocalClientId)
        {
            ready = "(you)";
            card.ShowReady(true);
        }
        else
        {
            ready = "";
            card.ShowReady(false);
        }

        if (clientId == NetworkManager.ServerClientId)
        {
            type = "Host";
            card.ShowReady(false);
            card.ShowKick(false);
        }
        else
        {
            type = "Player";
        }
        card.SetPlayerName($"{type} {clientId} {ready}");

        return card;
    }

    private void ClientOnDisconnect(ulong clientId)
    {
        Debug.Log($"Client {clientId} has disconnected");
    }

    private void ClientOnAllPlayersChanged(NetworkListEvent<It4080.PlayerData> change)
    {
        AddPlayersUsingPlayerData(NetworkHandler.Singleton.allPlayers);
    }

    private void ClientOnReadyToggled(bool isReady)
    {
        Debug.Log($"Ready = {isReady}");
        EnableStartIfAllReady();
    }

    private void btnStartOnClick()
    {
        StartGame();
    }

    private void StartGame()
    {
        NetworkManager.SceneManager.LoadScene("Arena1",
            UnityEngine.SceneManagement.LoadSceneMode.Single);
    }

    private void EnableStartIfAllReady()
    {
        int ready = 0;
        foreach (It4080.PlayerData players in NetworkHandler.Singleton.allPlayers)
        {
            if (players.isReady)
            {
                ready++;
            }
        }

        // This should be .Count -1 since the Host does not have a ready button.
        btnStart.enabled = ready == NetworkHandler.Singleton.allPlayers.Count;
        if (btnStart.enabled)
        {
            btnStart.GetComponentInChildren<TextMeshProUGUI>().text = "Start";
        } else
        {
            btnStart.GetComponentInChildren<TextMeshProUGUI>().text = "Players not yet ready";
        }
    }
}
