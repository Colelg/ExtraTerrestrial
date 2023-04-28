using Unity.Netcode;
using UnityEngine;

public class Player : NetworkBehaviour
{
    private float movementSpeed = 100;
    private float rotationSpeed = 300;
    private Camera _camera;
    public BulletSpawner bulletSpawner;
    public TMPro.TMP_Text txtHealth;
    public TMPro.TMP_Text txtPlanets;
    public TMPro.TMP_Text txtSpeed;
    public TMPro.TMP_Text txtBulletSpeed;
    public TMPro.TMP_Text txtHits;

    private Color[] colors = new Color[]
    {
        Color.black, Color.blue, Color.cyan, Color.gray, Color.green, Color.yellow
    };
    private int colorIndex = 0;

    public NetworkVariable<Color> netPlayerColor = new NetworkVariable<Color>();
    public NetworkVariable<int> netPlayerHealth = new NetworkVariable<int>(100);
    public NetworkVariable<int> netBulletSpeed = new NetworkVariable<int>(150);
    public NetworkVariable<int> PlanetsDestroyed = new NetworkVariable<int>(0);
    public NetworkVariable<int> netPlayerSpeed = new NetworkVariable<int>(100);
    public NetworkVariable<float> PlayerAddedSpeed = new NetworkVariable<float>();
    public NetworkVariable<int> netPlayerHits = new NetworkVariable<int>(0);


    public override void OnNetworkSpawn()
    {
        netPlayerColor.OnValueChanged += OnPlayerColorChanged;
        _camera = transform.Find("Camera").GetComponent<Camera>();
        _camera.enabled = IsOwner;

        netPlayerColor.Value = colors[colorIndex];
        ApplyPlayerColor();
        UpdateScoreDis();

        bulletSpawner = transform.Find("BulletSpawner").GetComponent<BulletSpawner>();
        if (IsClient)
        {
            netPlayerHealth.OnValueChanged += ClientOnScoreChanged;
        }

    }

    void ClientOnScoreChanged(int previous, int current)
    {
        UpdateScoreDis();
        if (IsOwner)
        {
            Debug.Log($"Client {NetworkManager.Singleton.LocalClientId} Score is now {netPlayerHealth.Value} ({previous} --> {current})");
        }

    }

    void OnCollisionEnter(Collision collision)
    {
        Debug.Log("There was collision");
        if (IsHost)
        {
            if (collision.gameObject.CompareTag("Bullet"))
            {
                HostHandleBulletCollision(collision.gameObject);
                Debug.Log("Bullet Player Collision");
            }
        }
    }


    private Vector3 CalcMovementFromInput(float delta)
    {
        bool isShiftKeyDown = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        float x_move = 0.0f;
        float z_move = Input.GetAxis("Vertical");

        if (isShiftKeyDown)
        {
            x_move = Input.GetAxis("Horizontal");
        }

        Vector3 moveVect = new Vector3(x_move, 0, z_move);
        moveVect *= PlayerAddedSpeed.Value + movementSpeed * delta;
        return moveVect;
    }

    private Vector3 CalculateRotationFromInput(float delta)
    {
        bool isShiftKeyDown = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        float y_rot = 0.0f;

        if (isShiftKeyDown)
        {
            y_rot = Input.GetAxis("Horizontal");
        }

        Vector3 rotVect = new Vector3(0, y_rot, 0);
        rotVect *= rotationSpeed * delta;

        return rotVect;
    }


    [ServerRpc]
    void RequestNextColorServerRpc(ServerRpcParams serverRpcParams = default)
    {

        Debug.Log($"host color {colorIndex} for {serverRpcParams.Receive.SenderClientId}");
        netPlayerColor.Value = colors[colorIndex];
        ServerChangeColor();
    }

    void ServerChangeColor()
    {
        colorIndex += 1;
        if (colorIndex > colors.Length)
        {
            colorIndex = 0;

        }
        netPlayerColor.Value = colors[colorIndex];
    }

    [ServerRpc]
    void RequestPositionForMovementServerRpc(Vector3 poschange, Vector3 rotChange, ServerRpcParams serverRpcParams = default)
    {
        transform.Translate(poschange);
        transform.Rotate(rotChange);
    }

    private void HostHandleBulletCollision(GameObject bullet)
    {
        Bullet bulletScript = (Bullet)bullet.GetComponent("Bullet");
        netPlayerHealth.Value -= 1;

        ServerChangeColor();


        ulong owner = bullet.GetComponent<NetworkObject>().OwnerClientId;
        Player otherPlayer =
            NetworkManager.Singleton.ConnectedClients[owner].PlayerObject.GetComponent<Player>();

        otherPlayer.netPlayerSpeed.Value += 1;
        otherPlayer.netBulletSpeed.Value += 1;
        otherPlayer.netPlayerHits.Value += 1;
        otherPlayer.PlayerAddedSpeed.Value = 1;
        if (otherPlayer.netPlayerHealth.Value < 100)
        {
            otherPlayer.netPlayerHealth.Value += 1;
        }

        if(netPlayerHits.Value < 5)
        {
            otherPlayer.PlayerAddedSpeed.Value += 1;
            otherPlayer.netPlayerSpeed.Value += 1;
        }



        Debug.Log("The host handled the bullet collision");
        UpdateScoreDis();
        Destroy(bullet);
    }

    private void UpdateScore()
    {
        if (IsOwner)
        {
            netPlayerHealth.Value -= 1;
            Debug.Log($"Score: {netPlayerHealth.Value}");
        }
    }

    private void UpdateScoreDis()
    {
        if (IsOwner)
        {
            print("Score is being updated on Gui");
            txtHealth.text = "Health: " + netPlayerHealth.Value.ToString();
            txtHits.text = "Enemies Hit: " + netPlayerHits.Value.ToString();
            txtBulletSpeed.text = "Bullet Speed: " + netBulletSpeed.Value.ToString();
            txtPlanets.text = "Planets Destroyed: " + PlanetsDestroyed.Value.ToString();
            txtSpeed.text = "Speed: " + PlayerAddedSpeed.Value.ToString();   


        }
    }





    // Start is called before the first frame updates
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (IsOwner)
        {
            UpdateOwner();
            UpdateScoreDis();
        }
    }

    void UpdateOwner()
    {
        Vector3 moveBy = CalcMovementFromInput(Time.deltaTime);
        Vector3 rotateBy = CalculateRotationFromInput(Time.deltaTime);
        RequestPositionForMovementServerRpc(moveBy, rotateBy);
        if (Input.GetButtonDown("Fire1"))
        {
            RequestNextColorServerRpc();
        }
        if (Input.GetButtonDown("Fire2"))
        {
            bulletSpawner.ShootServerRpc(netPlayerColor.Value);
        }

    }

    public void ApplyPlayerColor()
    {
        transform.Find("body").GetComponent<MeshRenderer>().material.color = netPlayerColor.Value;
        transform.Find("Antenna1").GetComponent<MeshRenderer>().material.color = netPlayerColor.Value;
        transform.Find("Antenna2").GetComponent<MeshRenderer>().material.color = netPlayerColor.Value;
        transform.Find("Probe1").GetComponent<MeshRenderer>().material.color = netPlayerColor.Value;
        transform.Find("Probe2").GetComponent<MeshRenderer>().material.color = netPlayerColor.Value;

    }

    public void OnPlayerColorChanged(Color previous, Color current)
    {
        ApplyPlayerColor();
    }
    // GetComponent<MeshRenderer>().material.color = netPlayerColor.Value;
    // transform.Find("LArm").GetComponent<MeshRenderer>().material.color = netPlayerColor.Value;
    // transform.Find("RArm").GetComponent<MeshRenderer>().material.color = netPlayerColor.Value;
}
