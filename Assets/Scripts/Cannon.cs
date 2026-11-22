using System;
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

    [Header("Magazine")]
    [SerializeField, Min(1)] private int magazineSize = 10;

    private Rigidbody shipRigidbody;
    private Collider[] shipColliders;
    private float nextFireTime;
    private int ammunition;

    public int Ammunition => ammunition;
    public int MagazineSize => magazineSize;
    public event Action<int> AmmunitionChanged;

    private void Awake()
    {
        shipRigidbody = GetComponent<Rigidbody>();
        shipColliders = GetComponentsInChildren<Collider>();
        ammunition = magazineSize;
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
        if (Time.time < nextFireTime || ammunition == 0)
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
        bullet.Destroyed += OnBulletDestroyed;

        ammunition = Mathf.Max(0, ammunition - 1);
        AmmunitionChanged?.Invoke(ammunition);

        // Inherit the ship's velocity so the projectile never lags behind a fast ship.
        Vector3 velocity = shipRigidbody.linearVelocity + muzzle.forward * projectileSpeed;

        bullet.Launch(velocity, shipColliders);
    }

    private void OnBulletDestroyed(Bullet bullet)
    {
        bullet.Destroyed -= OnBulletDestroyed;
        ammunition = Mathf.Min(ammunition + 1, magazineSize);
        AmmunitionChanged?.Invoke(ammunition);
    }
}
