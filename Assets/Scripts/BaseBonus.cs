using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class BaseBonus : NetworkBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log($"I am {this}");
    }
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        Debug.Log("spawned");

    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Collision");
        if (IsServer)
        {
            Destroy(gameObject);
        }
    }
}