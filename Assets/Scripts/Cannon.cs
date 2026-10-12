using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Spawns projectiles from the muzzle when the attack action is performed.
/// Attach to the player spaceship.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class Cannon : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputActionReference attackAction;

    [Header("Projectile")]
    [SerializeField] private Bullet projectilePrefab;
    [SerializeField] private Transform muzzle;
    [SerializeField, Min(0f)] private float projectileSpeed = 60f;
    [SerializeField, Min(0f)] private float fireCooldown = 0.2f;

    private Rigidbody shipRigidbody;
    private Collider[] shipColliders;
    private float nextFireTime;

    private void Awake()
    {
        shipRigidbody = GetComponent<Rigidbody>();
        shipColliders = GetComponentsInChildren<Collider>();
    }

    private void OnEnable()
    {
        attackAction.action.Enable();
        attackAction.action.performed += OnAttack;
    }

    private void OnDisable()
    {
        attackAction.action.performed -= OnAttack;
        attackAction.action.Disable();
    }

    private void OnAttack(InputAction.CallbackContext context)
    {
        if (Time.time < nextFireTime)
        {
            return;
        }

        if (projectilePrefab == null || muzzle == null)
        {
            Debug.LogWarning("Cannon is missing a projectile prefab or muzzle.", this);
            return;
        }

        nextFireTime = Time.time + fireCooldown;
        Fire();
    }

    private void Fire()
    {
        Bullet bullet = Instantiate(projectilePrefab, muzzle.position, muzzle.rotation);

        // Inherit the ship's velocity so the projectile never lags behind a fast ship.
        Vector3 velocity = shipRigidbody.linearVelocity + muzzle.forward * projectileSpeed;

        bullet.Launch(velocity, shipColliders);
    }
}
