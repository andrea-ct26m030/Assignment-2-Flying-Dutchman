using UnityEngine;
using System.Collections.Generic;

public class ObstacleSpawner : MonoBehaviour
{
    [Header("Obstacles")]
    [SerializeField] private GameObject[] obstaclePrefabs;
    [SerializeField, Min(0)] private int amount = 20;

    [Header("Spawn Area")]
    [SerializeField, Min(0f)] private float spawnRadius = 50f;
    [SerializeField, Min(0f)] private float despawnRadius = 75f;
    [Tooltip("How many random positions are tried per spawn to find a spot free of other colliders.")]
    [SerializeField, Range(1, 32)] private int spawnAttempts = 10;

    [Header("Random Scale")]
    [SerializeField, Min(0f)] private float minScale = 0.5f;
    [SerializeField, Min(0f)] private float maxScale = 2f;

    [Header("Mass")]
    [Tooltip("Mass of an obstacle at scale 1. Mass grows with the cube of the scale multiplier.")]
    [SerializeField, Min(0.01f)] private float baseMass = 50f;

    [Header("Movement")]
    [SerializeField, Min(0f)] private float minSpeed = 1f;
    [SerializeField, Min(0f)] private float maxSpeed = 5f;
    [SerializeField] private bool faceMovementDirection = true;

    private readonly List<GameObject> spawnedObstacles = new List<GameObject>();

    private void Start()
    {
        SpawnObstacles();
    }

    [ContextMenu("Spawn Obstacles")]
    public void SpawnObstacles()
    {
        if (obstaclePrefabs == null || obstaclePrefabs.Length == 0)
        {
            Debug.LogWarning("No obstacle prefabs have been assigned.", this);
            return;
        }

        while (spawnedObstacles.Count < amount)
        {
            if (!SpawnObstacle())
            {
                break;
            }
        }
    }

    private void Update()
    {
        float despawnRadiusSquared = despawnRadius * despawnRadius;

        for (int index = spawnedObstacles.Count - 1; index >= 0; index--)
        {
            GameObject obstacle = spawnedObstacles[index];
            bool wasDestroyed = obstacle == null;
            bool isOutOfRange = !wasDestroyed
                && (obstacle.transform.position - transform.position).sqrMagnitude > despawnRadiusSquared;

            if (!wasDestroyed && !isOutOfRange)
            {
                continue;
            }

            spawnedObstacles.RemoveAt(index);

            if (isOutOfRange)
            {
                Destroy(obstacle);
            }
        }

        // Top up to the target amount. Spawns that found no free spot are retried here.
        while (spawnedObstacles.Count < amount && SpawnObstacle())
        {
        }
    }

    private bool SpawnObstacle()
    {
        GameObject prefab = GetRandomPrefab();
        if (prefab == null)
        {
            return false;
        }

        float scaleMultiplier = Random.Range(minScale, maxScale);

        if (!TryFindFreeSpawnPosition(prefab, scaleMultiplier, out Vector3 spawnPosition))
        {
            // No free spot found this frame; the next Update tries again.
            return false;
        }

        Vector3 movementDirection = Random.onUnitSphere;
        Quaternion spawnRotation = faceMovementDirection
            ? Quaternion.LookRotation(movementDirection)
            : Random.rotationUniform;

        GameObject obstacle = Instantiate(prefab, spawnPosition, spawnRotation, transform);
        obstacle.transform.localScale *= scaleMultiplier;

        // Larger obstacles get a higher mass so collisions react realistically.
        Rigidbody rb = obstacle.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = obstacle.AddComponent<Rigidbody>();
        }
        rb.mass = baseMass * scaleMultiplier * scaleMultiplier * scaleMultiplier;

        FloatingSpaceship floatingSpaceship = obstacle.GetComponent<FloatingSpaceship>();
        if (floatingSpaceship == null)
        {
            floatingSpaceship = obstacle.AddComponent<FloatingSpaceship>();
        }

        floatingSpaceship.Initialize(movementDirection, Random.Range(minSpeed, maxSpeed));
        spawnedObstacles.Add(obstacle);
        return true;
    }

    /// <summary>
    /// Picks a random position inside the spawn radius that does not overlap
    /// existing colliders, so obstacles never spawn inside the map or each other.
    /// </summary>
    private bool TryFindFreeSpawnPosition(GameObject prefab, float scaleMultiplier, out Vector3 position)
    {
        float clearance = EstimateRadius(prefab) * scaleMultiplier;

        for (int attempt = 0; attempt < spawnAttempts; attempt++)
        {
            position = transform.position + Random.insideUnitSphere * spawnRadius;

            if (!Physics.CheckSphere(position, clearance, ~0, QueryTriggerInteraction.Ignore))
            {
                return true;
            }
        }

        position = default;
        return false;
    }

    private static float EstimateRadius(GameObject prefab)
    {
        Collider prefabCollider = prefab.GetComponentInChildren<Collider>();

        switch (prefabCollider)
        {
            case BoxCollider box:
                return box.size.magnitude * 0.5f;
            case SphereCollider sphere:
                return sphere.radius;
            case CapsuleCollider capsule:
                return Mathf.Max(capsule.radius, capsule.height * 0.5f);
            default:
                return 5f;
        }
    }

    private GameObject GetRandomPrefab()
    {
        int startIndex = Random.Range(0, obstaclePrefabs.Length);

        for (int offset = 0; offset < obstaclePrefabs.Length; offset++)
        {
            GameObject prefab = obstaclePrefabs[(startIndex + offset) % obstaclePrefabs.Length];
            if (prefab != null)
            {
                return prefab;
            }
        }

        Debug.LogWarning("All assigned obstacle prefab slots are empty.", this);
        return null;
    }

    private void OnValidate()
    {
        maxScale = Mathf.Max(minScale, maxScale);
        maxSpeed = Mathf.Max(minSpeed, maxSpeed);
        despawnRadius = Mathf.Max(spawnRadius, despawnRadius);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, spawnRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, despawnRadius);
    }
}
