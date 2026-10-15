using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Projectile behaviour: flies with the given velocity, is destroyed on impact
/// or after its lifetime expires. Raises a static event on every hit.
/// </summary>
[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class Bullet : MonoBehaviour
{
    /// <summary>Raised whenever any bullet collides with something.</summary>
    public static event Action<Bullet, Collision> Hit;

    [Tooltip("Seconds until the projectile is destroyed if it hits nothing.")]
    [SerializeField, Min(0f)] private float lifetime = 3f;

    private Rigidbody rb;
    private Collider ownCollider;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        ownCollider = GetComponent<Collider>();
    }

    private void OnEnable()
    {
        StartCoroutine(DespawnAfterLifetime());
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    /// <summary>Sets the initial velocity and ignores collisions with the shooter's colliders.</summary>
    public void Launch(Vector3 velocity, Collider[] collidersToIgnore)
    {
        rb.linearVelocity = velocity;

        foreach (Collider shooterCollider in collidersToIgnore)
        {
            Physics.IgnoreCollision(ownCollider, shooterCollider);
        }
    }

    private IEnumerator DespawnAfterLifetime()
    {
        yield return new WaitForSeconds(lifetime);
        Destroy(gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        Hit?.Invoke(this, collision);
        Destroy(gameObject);
    }
}
