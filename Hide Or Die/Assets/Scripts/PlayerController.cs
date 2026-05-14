using Unity.Netcode;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 5f;
    public float sprintSpeed = 8f;
    public float gravity = -9.81f;
    public float jumpHeight = 1.5f;

    [Header("Mouse Look")]
    public Transform cameraTransform;
    public float mouseSensitivity = 150f;

    private CharacterController controller;
    private Vector3 velocity;
    private float xRotation = 0f;

    [Header("Whistle")]
    public AudioSource audioSource;
    public AudioClip whistleAudio;
    public float whistleCooldown = 5f;

    private float nextWhistleTime = 0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        controller = GetComponent<CharacterController>();

        if (SpawnManager.Instance != null)
        {
            Transform spawnPoint = SpawnManager.Instance.GetSpawnPoint();

            controller.enabled = false;
            transform.position = spawnPoint.position;
            transform.rotation = spawnPoint.rotation;
            controller.enabled = true;
        }

        //Lock the mouse to the game screen
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Update is called once per frame
    void Update()
    {
        MovePlayer();
        LookAround();
        SetCursorVisibility();

        if (Input.GetKeyDown(KeyCode.Q))
        {
            TryWhistle();
        }
    }

    void TryWhistle()
    {
        if(Time.time < nextWhistleTime)
        {
            Debug.Log("Whistle is on cooldown!");
            return;
        }

        nextWhistleTime = Time.time + whistleCooldown;

        WhistleServerRpc();
    }

    [Rpc(SendTo.Server)]
    private void WhistleServerRpc()
    {
        WhistleClientRpc();
    }

    [Rpc(SendTo.Everyone)]
    private void WhistleClientRpc()
    {
        if (audioSource == null)
        {
            Debug.LogWarning("No Audiosource assigned!");
            return;
        }

        if(whistleAudio == null)
        {
            Debug.LogWarning("No whistle sound assigned");
            return;
        }

        audioSource.PlayOneShot(whistleAudio);
    }

    private static void SetCursorVisibility()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else if (Input.GetKeyDown(KeyCode.L))
        {
            //Lock the mouse to the game screen
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    void MovePlayer()
    {
        bool grounded = controller.isGrounded;

        if (grounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        // Get keyboard input
        float x = Input.GetAxis("Horizontal"); // A/D
        float z = Input.GetAxis("Vertical");   // W/S

        // Movement direction based on where player is facing
        Vector3 move = transform.right * x + transform.forward * z;

        float currentSpeed = Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : walkSpeed;

        // Jump
        if (Input.GetKeyDown(KeyCode.Space) && grounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        // Apply gravity
        velocity.y += gravity * Time.deltaTime;

        Vector3 finalMove = move * currentSpeed;
        finalMove.y = velocity.y;

        controller.Move(finalMove * Time.deltaTime);
    }
    
    void LookAround()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // Rotate player left/right
        transform.Rotate(Vector3.up * mouseX);

        // Rotate camera up/down
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -80f, 80f);

        cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }
}
