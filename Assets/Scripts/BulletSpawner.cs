using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class BulletSpawner : NetworkBehaviour
{
    public Rigidbody bullet;
    private int bulletSpeed = 100;
    private float timeToLive = 5f;


    [ServerRpc]
    public void ShootServerRpc(Color color, ServerRpcParams rpcParams = default)
    {
        Debug.Log($"Owner:" + " " + $"{rpcParams.Receive.SenderClientId}");
        Rigidbody newBullet = Instantiate(bullet, transform.position, transform.rotation);
        newBullet.GetComponent<NetworkObject>().SpawnWithOwnership(rpcParams.Receive.SenderClientId);
        newBullet.velocity = transform.forward * bulletSpeed;
        Destroy(newBullet.gameObject, timeToLive);

        ulong owner = bullet.GetComponent<NetworkObject>().OwnerClientId;
        Player otherPlayer =
    NetworkManager.Singleton.ConnectedClients[owner].PlayerObject.GetComponent<Player>();

        bulletSpeed = bulletSpeed + otherPlayer.netBulletSpeed.Value;
        Debug.Log("Bullet Speed increased to: " + bulletSpeed);
    }

}
