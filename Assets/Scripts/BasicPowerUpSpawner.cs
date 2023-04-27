using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UIElements;

public class BasicPowerUpSpawner : NetworkBehaviour
{
    public Rigidbody powerUp;

    public bool spawnOnLoad = true;
    public float spawnDelay = 3.0f;
    private Rigidbody curPowerUp = null;
    private float timeUntilSpawn = 0.0f;
    public NetworkVariable<int> PlanetsDestroyed = new NetworkVariable<int>(0);

    public void Start()
    {
    }


    public void Update()
    {
        if (IsServer)
        {
            ServerUpdate();
        }
    }


    private void ServerUpdate()
    {
        if (timeUntilSpawn > 0f)
        {
            timeUntilSpawn -= Time.deltaTime;
            if (timeUntilSpawn <= 0)
            {
                SpawnPowerUp();
            }
        }
        else if (curPowerUp == null)
        {
            timeUntilSpawn = spawnDelay;
            PlanetsDestroyed.Value += 1;
        }
    }


    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsServer)
        {
            HostOnNetworkSpawn();
        }
    }


    private void HostOnNetworkSpawn()
    {
        if (powerUp != null && spawnOnLoad)
        {
            SpawnPowerUp();
        }
    }


    private void SpawnPowerUp()
    {
        Vector3 spawnPosition = transform.position;
        spawnPosition.y = 1;

        Rigidbody instantiatedPowerUP =
            Instantiate(powerUp, spawnPosition, Quaternion.identity);

        instantiatedPowerUP.GetComponent<NetworkObject>().Spawn();

        curPowerUp = instantiatedPowerUP;

        instantiatedPowerUP.GetComponent<BaseBonus>().PlanetSpawn = this;

        
    }


}

/* public class BasicPowerUpSpawner : NetworkBehaviour
{
    public Rigidbody bonusPrefab;

    public bool spawnOnLoad = true;
    public float refreshTime = 5f;
    public float timeUntilSpawn = 0.0f;

    public Rigidbody PowerUp = null;

    public Transform spawnPointTransform;
    public void Start()
    {
        spawnPointTransform = transform.Find("PowerUp");
    }

    public void Update()
    {

        if (IsServer)
        {
            ServerUpdate();
        }
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (IsServer && bonusPrefab != null)
            spawnBonus();
    }
    private void HostOnNetworkSpawn()
    {
        if (PowerUp != null && spawnOnLoad)
        {
            spawnBonus();
        }
    }

    private void spawnBonus()
    {
        Vector3 Spawnpos = transform.position;
        Spawnpos.y = 1;
        Rigidbody bonusSpawn = Instantiate(
        bonusPrefab, Spawnpos, Quaternion.identity);
        bonusSpawn.GetComponent<NetworkObject>().Spawn();
        PowerUp = bonusSpawn;
    }

    private void ServerUpdate()
    {
        if (timeUntilSpawn > 0f)
        {
            timeUntilSpawn -= Time.deltaTime;
            if (timeUntilSpawn <= 0f)
            {
                spawnBonus();
            }
        }

    }

}*/
