using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Player : NetworkBehaviour
{
    private float movementSpeed = 100f;
    private float rotationSpeed = 300f;
    private Camera _camera;
    public BulletSpawner bulletSpawner;

    private Color[] colors = new Color[]
    {
        Color.black, Color.blue, Color.cyan, Color.gray, Color.green, Color.yellow
    };
    private int colorIndex = 0;

    public NetworkVariable<Color> netPlayerColor = new NetworkVariable<Color>();


    public override void OnNetworkSpawn()
    {
        netPlayerColor.OnValueChanged += OnPlayerColorChanged;
        _camera = transform.Find("Camera").GetComponent<Camera>();
        _camera.enabled = IsOwner;

        netPlayerColor.Value = colors[colorIndex];
        ApplyPlayerColor();

        bulletSpawner = transform.Find("BulletSpawner").GetComponent<BulletSpawner>();

    }

    void OnCollisionEnter(Collision collision)
    {
        if (IsHost)
        {
            if (collision.gameObject.CompareTag("Bullet"))
            {
                HostHandleBulletCollision(collision.gameObject);
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
        moveVect *= movementSpeed * delta;

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
        colorIndex += 1;
        if (colorIndex > colors.Length - 1)
        {
            colorIndex = 0;
        }

        Debug.Log($"host color index = {colorIndex} for {serverRpcParams.Receive.SenderClientId}");
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
        RequestNextColorServerRpc();

        ulong owner = bullet.GetComponent<NetworkObject>().OwnerClientId;
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
         //transform.Find("LArm").GetComponent<MeshRenderer>().material.color = netPlayerColor.Value;
         //transform.Find("RArm").GetComponent<MeshRenderer>().material.color = netPlayerColor.Value;
    }

    public void OnPlayerColorChanged(Color previous, Color current)
    {
        ApplyPlayerColor();
    }
    // GetComponent<MeshRenderer>().material.color = netPlayerColor.Value;
    // transform.Find("LArm").GetComponent<MeshRenderer>().material.color = netPlayerColor.Value;
    // transform.Find("RArm").GetComponent<MeshRenderer>().material.color = netPlayerColor.Value;
}
