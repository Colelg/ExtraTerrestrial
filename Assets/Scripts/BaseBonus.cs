using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UIElements;
using Unity.VisualScripting;

public class BaseBonus : NetworkBehaviour

{
    public TMPro.TMP_Text txtScore;
    public TMPro.TMP_Text txtPlanetScore;
    public NetworkVariable<int> PlanetHealth = new NetworkVariable<int>(100);
    public BasicPowerUpSpawner PlanetSpawn;
    public float rotationSpeed = 100f;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log($"I am {this}");
        Vector3 rotateby = CalculateRotation(Time.deltaTime);
        PlanetSpawn.PlanetsDestroyed.OnValueChanged -= ClientOnPlanetDestroyed;
        PlanetHealth.OnValueChanged += ClientOnPlanetChanged;
        UpdatePlanetHealth();
    }
    private Vector3 CalculateRotation(float delta)
    {
        float y_rot = 1.0f;

        Vector3 rotVect = new Vector3(0, y_rot, 0);
        rotVect *= rotationSpeed * delta;

        return rotVect;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        Debug.Log("spawned");
        UpdatePlanetScore();
        UpdatePlanetHealth();

    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Collision");
        if (IsServer) //&& collision.gameObject.CompareTag("Bullet"))
        {
            //collision.gameObject.GetComponent<bullet>()
            PlanetHealth.Value -= 10;
            txtScore.text = "Health: " + PlanetHealth.Value.ToString();
            PlanetHealth.OnValueChanged += ClientOnPlanetChanged;
            //HostHandleBulletCollision(collision.gameObject);
            
            if (PlanetHealth.Value == 0)
            {


                Destroy(gameObject);
                HostHandleBulletCollision(gameObject);

                //if planet health is less than or equal to zero, we go from collision object back to player object, increase their planets destroyed, like bullet collision script
                //get owner id, compare to player
            }
            
        }
    }

    private void HostHandleBulletCollision(GameObject bullet)
    {
        Bullet bulletScript = (Bullet)bullet.GetComponent("Bullet");

        ulong owner = bullet.GetComponent<NetworkObject>().OwnerClientId;
        Player otherPlayer =
            NetworkManager.Singleton.ConnectedClients[owner].PlayerObject.GetComponent<Player>();
        otherPlayer.PlanetsDestroyed.Value += 1;
        PlanetSpawn.PlanetsDestroyed.OnValueChanged -= ClientOnPlanetDestroyed;
        UpdatePlanetScore();
    }


    void HostHandleBulletCollision2(GameObject bullet)
    {
        Bullet bulletScript = (Bullet)bullet.GetComponent("Bullet");

        ulong owner = bullet.GetComponent<NetworkObject>().OwnerClientId;
        Player otherPlayer =
            NetworkManager.Singleton.ConnectedClients[owner].PlayerObject.GetComponent<Player>();

        otherPlayer.PlanetsDestroyed.Value += 1;
    }


    void ClientOnPlanetChanged(int previous, int current) 
    {
        UpdatePlanetHealth();
    }

    void ClientOnPlanetDestroyed(int previous, int current)
    {
        UpdatePlanetScore();
    }

    void UpdatePlanetHealth()
    {
        if (IsOwner)
        {
            txtScore.text = "Health " + PlanetHealth.Value.ToString();

        }
    }

    void UpdatePlanetScore()
    {
        if (IsOwner)
        {
            txtPlanetScore.text = "Planets Destroyed: " + PlanetSpawn.PlanetsDestroyed.Value.ToString();
        }
    }
}