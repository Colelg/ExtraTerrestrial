using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;



public class ChatServer : NetworkBehaviour
{

    public It4080.Chat chat;

    [ClientRpc]
    public void SendChatMessageClientRpc(string message, ClientRpcParams clientRpcParams = default)
    {

        Debug.Log(message);
        //Chat.txtChatLog.text += $"\n{message}";
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestSendMessageServerRpc(string message, ServerRpcParams serverRpcParams = default)
    {

        Debug.Log(message);

        //Chat.txtChatLog.text += $"\n{message}";
    }

    [ServerRpc(RequireOwnership = false)]
    public void SendChatMessageServerRpc(string message, ServerRpcParams serverRpcParams = default)
    {
        Debug.Log($"Host got message: {message}");
        SendChatMessageClientRpc(message);
    }



   [ServerRpc(RequireOwnership = false)]
   public void SendSystemMessageServerRpc(string message, ServerRpcParams serverRpcParams = default)
   {

   }

    public void DisplayMessageLocally(string message)
    {
        Debug.Log(message);
        //ScrlChatLog.text += $"\n{message}";

    }
}
