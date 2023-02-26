using System.Collections;
using System.Collections.Generic;
using System.Net;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;




public class Main : NetworkBehaviour
{
    private NetworkManager netMgr;

    public It4080.NetworkSettings netSettings;
    public ChatServer chatServer;
    public It4080.Chat chat;
    public Button StartBtn;



    void Start()
    {
        netSettings.startClient += NetSettingsOnStartClient;
        netSettings.startHost += NetSettingsOnStartHost;
        netSettings.startServer += NetSettingsOnStartServer;
        netSettings.setStatusText("Not Connected");

        chat.sendMessage += ChatOnSendMessage;

        StartBtn = GameObject.Find("StartBtn").GetComponent<Button>();
        StartBtn.onClick.AddListener(btnStartOnClick);
        StartBtn.gameObject.SetActive(true);
    }


    private void ChatOnSendMessage(It4080.Chat.ChatMessage msg)
    {
        chatServer.RequestSendMessageServerRpc(msg.message);
    }


    private void setupTransport(IPAddress ip, ushort port)
    {
        var utp = NetworkManager.Singleton.GetComponent<UnityTransport>();
        utp.ConnectionData.Address = ip.ToString();
        utp.ConnectionData.Port = port;
    }


    private void printIs(string msg)
    {
        Debug.Log($"[{msg}]  {MakeIsString()}");
    }


    private string MakeIsString()
    {
        return $"server:{IsServer} host:{IsHost} client:{IsClient} owner:{IsOwner}";
    }


    private string MakeWelcomeMessage(ulong clientId)
    {
        return $"Welcome to the server.  You are player {clientId}.  Have a good time.";
    }



    private void StartServer(IPAddress ip, ushort port)
    {
        Debug.Log($"Starting server {ip}:{port}");

        NetworkManager.Singleton.OnServerStarted += HostOnServerStarted;
        NetworkManager.Singleton.OnClientConnectedCallback += HostOnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += HostOnClientDisconnected;

        netSettings.setStatusText("Starting Server");

        setupTransport(ip, port);
        NetworkManager.Singleton.StartServer();
        netSettings.hide();
    }


    private void StartHost(IPAddress ip, ushort port)
    {
        Debug.Log($"Starting host {ip}:{port}");

        NetworkManager.Singleton.OnServerStarted += HostOnServerStarted;
        NetworkManager.Singleton.OnClientConnectedCallback += HostOnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += HostOnClientDisconnected;

        netSettings.setStatusText("Starting Host");

        setupTransport(ip, port);
        NetworkManager.Singleton.StartHost();
        netSettings.hide();
    }


    private void StartClient(IPAddress ip, ushort port)
    {
        Debug.Log($"Starting client {ip}:{port}");

        NetworkManager.Singleton.OnClientConnectedCallback += ClientOnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += ClientOnClientDisconnect;

        setupTransport(ip, port);
        NetworkManager.Singleton.StartClient();

        netSettings.setStatusText("Connecting to host/server");
        netSettings.hide();
    }




    // ----------------------
    // Host Events
    // ----------------------
    private void HostOnServerStarted()
    {
        printIs("host/server started event");
        if (IsHost)
        {
            netSettings.setStatusText($"Host Running.  We are client {NetworkManager.Singleton.LocalClientId}.");
            chat.enabled = true;
            chat.enable(true);
        }
        else
        {
            netSettings.setStatusText("Server Running");
        }
        // Displays locally for the host/server only.
        chat.SystemMessage("Server/Host Started");
    }


    private void HostOnClientConnected(ulong clientId)
    {
        // Tell everyone that a new client connected
       chatServer.SendSystemMessageServerRpc($"Client {clientId} connected.");
        Debug.Log($"Client {clientId} connected.");

        // Send the welcome message to the newly connected client only
      //  chatServer.SendSystemMessageServerRpc(
         //   MakeWelcomeMessage(clientId),
        //    clientId);
    }


    private void HostOnClientDisconnected(ulong clientId)
    {
        chatServer.SendSystemMessageServerRpc($"Client {clientId} disconnected.");
    }

    private void StartGame()
    {
        NetworkManager.SceneManager.LoadScene("Arena1", UnityEngine.SceneManagement.LoadSceneMode.Single);
    }

    private void btnStartOnClick()
    {
        StartGame();
    }


    // ----------------------
    // Client Events
    // ----------------------
    private void ClientOnClientConnected(ulong clientId)
    {
        netSettings.setStatusText($"Connected as {clientId}");
        chat.enabled = true;
        chat.enable(true);
    }


    private void ClientOnClientDisconnect(ulong clientId)
    {
        // Must manually create a system message in our chat control
        // here since we do not have a connection to the server.
        chat.SystemMessage("Disconnected from Server.");
        netSettings.setStatusText("Connection Lost");
        netSettings.show();
        chat.enable(false);
    }



    // ----------------------
    // netSettings Events
    // ----------------------
    private void NetSettingsOnStartClient(IPAddress ip, ushort port)
    {
        StartClient(ip, port);
    }


    private void NetSettingsOnStartHost(IPAddress ip, ushort port)
    {
        StartHost(ip, port);
    }


    private void NetSettingsOnStartServer(IPAddress ip, ushort port)
    {
        StartServer(ip, port);
    }

}

/*
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;

public class Main : NetworkBehaviour
{
    public It4080.NetworkSettings netSettings;
    public It4080.Chat chat;
    public ChatServer chatServer;
    


    // Start is called before the first frame update
    void Start(){
        chat.SystemMessage("Chat Server Created:");
        It4080.Chat.ChatMessage msg = new It4080.Chat.ChatMessage();
        msg.message = "Chat Server";
        chat.ShowMessage(msg);
        netSettings.startServer += NetSettingsOnServerStart;
        netSettings.startHost += NetSettingsOnHostStart;
        netSettings.startClient += NetSettingsOnClientStart;
        netSettings.setStatusText("Not Connected");
    }
    private void startClient(IPAddress ip, ushort port) {
        var utp = NetworkManager.Singleton.GetComponent<UnityTransport>();
        utp.ConnectionData.Address = ip.ToString();
        utp.ConnectionData.Port = port;

        netSettings.setStatusText($"Connected as: {NetworkManager.Singleton.LocalClientId} ");
        NetworkManager.Singleton.OnClientConnectedCallback += ClientOnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += ClientOnClientDisconnected;

        NetworkManager.Singleton.StartClient();
        netSettings.hide();
        Debug.Log("Started client");
    }

    
    private void startHost(IPAddress ip, ushort port) {
        var utp = NetworkManager.Singleton.GetComponent<UnityTransport>();
        utp.ConnectionData.Address = ip.ToString();
        utp.ConnectionData.Port = port;

        netSettings.setStatusText($"Connected as host: {NetworkManager.Singleton.LocalClientId}");
        NetworkManager.Singleton.OnClientConnectedCallback += HostOnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += HostOnClientDisconnected;

        NetworkManager.Singleton.StartHost();
        netSettings.hide();
        Debug.Log("Started host");
        It4080.Chat.ChatMessage msg = new It4080.Chat.ChatMessage();
        msg.message = "Host Connected";
        chat.ShowMessage(msg);
    }

    private void startServer(IPAddress ip, ushort port) {
        var utp = NetworkManager.Singleton.GetComponent<UnityTransport>();
        utp.ConnectionData.Address = ip.ToString();
        utp.ConnectionData.Port = port;

        NetworkManager.Singleton.OnClientConnectedCallback += HostOnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += HostOnClientDisconnected;
        netSettings.setStatusText("Server");

        NetworkManager.Singleton.StartServer();
        netSettings.hide();
        Debug.Log("Started server");
        printIs("startServer");
    }



    // ------------------------------------
    // Events
    private void NetSettingsOnServerStart(IPAddress ip, ushort port) {
        startServer(ip, port);
        Debug.Log("Server started");
        printIs("");
    }

    private void NetSettingsOnHostStart(IPAddress ip, ushort port) {
        startHost(ip, port);
        Debug.Log("Host started");
        printIs("");
    }

    private void NetSettingsOnClientStart(IPAddress ip, ushort port) {
        startClient(ip, port);
        Debug.Log("Client started");
        printIs("");
    }

    private void printIs(string msg) {
        Debug.Log($"[{msg}] server:{IsServer} host:{IsHost} client:{IsClient} owner:{IsOwner}");
    }

    private void HostOnClientConnected(ulong clientID) {
        Debug.Log($"Client connected to me: {clientID}");
        chat.SystemMessage($"{clientID} connected to server.");
    }

    private void HostOnClientDisconnected(ulong clientID)
    {
        Debug.Log($"Client disconnected from me: {clientID}");
    }

    private void ClientOnClientConnected(ulong clientID) {
        Debug.Log($"Client connected: {clientID}");
        chat.SystemMessage($"{clientID} connected to server.");
    }

    private void ClientOnClientDisconnected(ulong clientID) {
        Debug.Log($"Client disconnected: {clientID}");
    }


}
 */