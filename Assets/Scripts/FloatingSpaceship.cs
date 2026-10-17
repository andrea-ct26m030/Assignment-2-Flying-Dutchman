using UnityEngine;

/// <summary>
/// Moves a spawned obstacle with constant speed using its Rigidbody and turns
/// around before it would run into another collider.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class FloatingSpaceship : MonoBehaviour
{
    [Header("Obstacle Avoidance")]
    [SerializeField, Min(0f)] private float lookAheadTime = 2f;
    [SerializeField] private LayerMask obstacleLayers = ~0;
    [Tooltip("Seconds the ship commits to a new heading before it checks for obstacles again.")]
    [SerializeField, Min(0f)] private float turnCooldown = 0.5f;
    [Tooltip("Random headings tried when the mirrored heading is blocked as well.")]
    [SerializeField, Range(0, 16)] private int escapeAttempts = 8;

    [Header("Collision Response")]
    [Tooltip("How fast (units per second squared) the ship returns to cruise speed after being knocked around.")]
    [SerializeField, Min(0f)] private float recoveryAcceleration = 15f;

    [Header("Stuck Detection")]
    [Tooltip("Seconds between stuck checks. 0 disables the check.")]
    [SerializeField, Min(0f)] private float stuckCheckInterval = 3f;
    [Tooltip("Ship is considered stuck if it covered less than this fraction of the expected distance.")]
    [SerializeField, Range(0f, 1f)] private float stuckDistanceFraction = 0.25f;

    private static readonly RaycastHit[] hitBuffer = new RaycastHit[16];

    private Rigidbody rb;
    private Vector3 movementDirection = Vector3.forward;
    private float speed;
    private float castRadius = 1f;
    private float nextAvoidanceTime;
    private float nextStuckCheckTime;
    private Vector3 lastStuckCheckPosition;
    private bool hitByBullet;
    private bool hitByObstacle;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void Initialize(Vector3 direction, float movementSpeed)
    {
        movementDirection = direction.normalized;
        speed = movementSpeed;

        // Dynamic body so collisions with the player use the configured mass.
        rb.isKinematic = false;
        rb.useGravity = false;
        rb.freezeRotation = true;
        rb.linearVelocity = movementDirection * speed;

        Collider ownCollider = GetComponent<Collider>();
        if (ownCollider != null)
        {
            castRadius = ownCollider.bounds.extents.magnitude;
        }

        lastStuckCheckPosition = rb.position;
        nextStuckCheckTime = Time.time + stuckCheckInterval;
    }

    private void FixedUpdate()
    {
        Vector3 velocity = rb.linearVelocity;

        // Projectile impacts must not knock the ship off course: discard their impulse.
        if (hitByBullet)
        {
            velocity = movementDirection * speed;
            hitByBullet = false;
        }

        // Real collisions (player, other ships, map) change the heading. The physics
        // impulse stays in the velocity and fades out via recoveryAcceleration below.
        if (hitByObstacle && velocity.sqrMagnitude > 0.01f)
        {
            movementDirection = velocity.normalized;
        }
        hitByObstacle = false;

        bool turned = false;
        if (Time.time >= nextAvoidanceTime)
        {
            turned = AvoidObstacles();
        }

        Vector3 cruiseVelocity = movementDirection * speed;
        rb.linearVelocity = turned
            ? cruiseVelocity
            : Vector3.MoveTowards(velocity, cruiseVelocity, recoveryAcceleration * Time.fixedDeltaTime);

        rb.MoveRotation(Quaternion.LookRotation(movementDirection));

        CheckStuck();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.GetComponentInParent<Bullet>() != null)
        {
            hitByBullet = true;
        }
        else
        {
            hitByObstacle = true;
        }
    }

    /// <returns>True if the heading was changed.</returns>
    private bool AvoidObstacles()
    {
        if (!FindObstacle(movementDirection, out RaycastHit hit))
        {
            return false;
        }

        Vector3 newDirection = Vector3.Reflect(movementDirection, hit.normal).normalized;

        // If the mirrored heading is blocked too (corner, crowd), try random headings
        // that point away from the obstacle instead of flipping back and forth.
        if (FindObstacle(newDirection, out _))
        {
            for (int attempt = 0; attempt < escapeAttempts; attempt++)
            {
                Vector3 candidate = Random.onUnitSphere;
                if (Vector3.Dot(candidate, hit.normal) < 0f)
                {
                    candidate = -candidate;
                }

                if (!FindObstacle(candidate, out _))
                {
                    newDirection = candidate;
                    break;
                }
            }
        }

        movementDirection = newDirection;
        nextAvoidanceTime = Time.time + turnCooldown;
        return true;
    }

    private bool FindObstacle(Vector3 direction, out RaycastHit nearest)
    {
        float lookAhead = speed * lookAheadTime + castRadius;

        int hitCount = Physics.SphereCastNonAlloc(
            rb.position,
            castRadius,
            direction,
            hitBuffer,
            lookAhead,
            obstacleLayers,
            QueryTriggerInteraction.Ignore);

        nearest = default;
        bool found = false;

        for (int index = 0; index < hitCount; index++)
        {
            RaycastHit hit = hitBuffer[index];

            // distance 0 means the sphere already touches the collider; its normal is useless.
            if (hit.distance <= 0f || hit.rigidbody == rb || ShouldIgnore(hit.collider))
            {
                continue;
            }

            if (!found || hit.distance < nearest.distance)
            {
                nearest = hit;
                found = true;
            }
        }

        return found;
    }

    private void CheckStuck()
    {
        if (stuckCheckInterval <= 0f || Time.time < nextStuckCheckTime)
        {
            return;
        }

        float expectedDistance = speed * stuckCheckInterval;
        float travelled = Vector3.Distance(rb.position, lastStuckCheckPosition);

        if (travelled < expectedDistance * stuckDistanceFraction)
        {
            // Stuck inside geometry or trapped between obstacles: remove it,
            // the spawner replaces it with a fresh one elsewhere.
            Destroy(gameObject);
            return;
        }

        lastStuckCheckPosition = rb.position;
        nextStuckCheckTime = Time.time + stuckCheckInterval;
    }

    /// <summary>Projectiles and the player are not treated as obstacles to steer away from.</summary>
    private static bool ShouldIgnore(Collider other)
    {
        return other.GetComponentInParent<Bullet>() != null
            || other.GetComponentInParent<SpaceshipController>() != null;
    }
}
