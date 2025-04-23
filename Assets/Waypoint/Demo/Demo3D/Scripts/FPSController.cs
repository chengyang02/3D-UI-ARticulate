using UnityEngine;

public class FPSController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float sprintMultiplier = 1.5f;
    public float jumpForce = 5f;
    public float gravity = -9.81f;
    public float acceleration = 10f;  // Cuánto tarda en alcanzar la velocidad deseada
    public float deceleration = 10f;  // Cuánto tarda en detenerse al soltar las teclas

    [Header("Mouse Settings")]
    public float sensitivity = 100f;
    public Transform cameraTransform;
    public float smoothTime = 0.05f;
    public float accelerationFactor = 0.1f;

    private CharacterController controller;
    private Vector3 velocity;
    private float xRotation;
    private bool isPaused;
    private Vector2 currentMouseDelta;
    private Vector3 currentVelocity;  // Almacena la velocidad actual del jugador

    void Start()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        HandleInput();
        if (isPaused) return;
        HandleMouseLook();
        HandleMovement();
    }

    void HandleMouseLook()
    {
        Vector2 targetMouseDelta = new Vector2(
            Input.GetAxisRaw("Mouse X"),
            Input.GetAxisRaw("Mouse Y")
        ) * sensitivity * Time.deltaTime;

        currentMouseDelta = Vector2.Lerp(currentMouseDelta, targetMouseDelta, accelerationFactor);

        xRotation = Mathf.Clamp(xRotation - currentMouseDelta.y, -90f, 90f);
        cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * currentMouseDelta.x);
    }

    void HandleMovement()
    {
        // Obtener entrada suavizada
        float inputX = Input.GetAxisRaw("Horizontal");
        float inputZ = Input.GetAxisRaw("Vertical");

        // Vector de dirección
        Vector3 targetVelocity = (transform.right * inputX + transform.forward * inputZ).normalized
                                * moveSpeed * (Input.GetKey(KeyCode.LeftShift) ? sprintMultiplier : 1f);

        // Suavizar aceleración y desaceleración
        if (targetVelocity.magnitude > 0.1f)
        {
            currentVelocity = Vector3.Lerp(currentVelocity, targetVelocity, acceleration * Time.deltaTime);
        }
        else
        {
            currentVelocity = Vector3.Lerp(currentVelocity, Vector3.zero, deceleration * Time.deltaTime);
        }

        controller.Move(currentVelocity * Time.deltaTime);

        // Manejo de gravedad y saltos
        if (controller.isGrounded)
        {
            velocity.y = Input.GetButtonDown("Jump") ? Mathf.Sqrt(jumpForce * -2f * gravity) : -2f;
        }
        else
        {
            velocity.y += gravity * Time.deltaTime;
        }
        controller.Move(Vector3.up * velocity.y * Time.deltaTime);
    }

    void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            isPaused = true;
            Cursor.lockState = CursorLockMode.None;
        }

        if (Input.GetMouseButtonDown(0) && isPaused)
        {
            isPaused = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }
}
