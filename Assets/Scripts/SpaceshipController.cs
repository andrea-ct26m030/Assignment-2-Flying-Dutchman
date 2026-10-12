using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class SpaceshipController : MonoBehaviour
{
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference rollAction;
    [SerializeField] private InputActionReference lookAction;
    [SerializeField] private float lookSensitivity = 0.02f;

    [SerializeField] private float rollAcceleration = 5f;
    [SerializeField] private float acceleration = 10f;

    private Vector2 pendingLook;

    private Rigidbody rb;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        // Collect mouse movement between physics updates.
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            pendingLook += lookAction.action.ReadValue<Vector2>();
        }
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        Physics.invokeCollisionCallbacks = true;

        Collider[] playerColliders = GetComponentsInChildren<Collider>();
        foreach (Collider playerCollider in playerColliders)
        {
            playerCollider.providesContacts = true;
        }
    }

    private void OnEnable()
    {
        moveAction.action.Enable();
        rollAction.action.Enable();
        lookAction.action.Enable();

    }

    private void OnDisable()
    {
        moveAction.action.Disable();
        rollAction.action.Disable();
        lookAction.action.Disable();

        lookAction.action.Disable();
        pendingLook = Vector2.zero;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void FixedUpdate()
    {
        float moveInput = moveAction.action.ReadValue<float>();

        // Move along the spaceship's forward direction.
        Vector3 direction = rb.rotation * Vector3.forward;

        rb.AddForce(
            direction * moveInput * acceleration,
            ForceMode.Acceleration
        );


        float rollInput = rollAction.action.ReadValue<float>();

        // Rotate around the spaceship's local forward axis.
        // Negative sign makes A roll left and D roll right.
        rb.AddRelativeTorque(
            Vector3.forward * -rollInput * rollAcceleration,
            ForceMode.Acceleration
        );

        // Mouse Y controls pitch; mouse X controls yaw.
        Vector3 steering = new Vector3(
            -pendingLook.y,
            pendingLook.x,
            0f
        );

        rb.AddRelativeTorque(
            steering * lookSensitivity,
            ForceMode.VelocityChange
        );

        pendingLook = Vector2.zero;
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"Player collided with {collision.gameObject.name}.", collision.gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"Player entered trigger {other.gameObject.name}.", other.gameObject);
    }
}
