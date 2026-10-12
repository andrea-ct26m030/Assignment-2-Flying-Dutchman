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

    [Header("Random Scale")]
    [SerializeField, Min(0f)] private float minScale = 0.5f;
    [SerializeField, Min(0f)] private float maxScale = 2f;

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

            SpawnObstacle();
        }
    }

    private bool SpawnObstacle()
    {
        GameObject prefab = GetRandomPrefab();
        if (prefab == null)
        {
            return false;
        }

        Vector3 spawnPosition = transform.position + Random.insideUnitSphere * spawnRadius;
        Vector3 movementDirection = Random.onUnitSphere;
        Quaternion spawnRotation = faceMovementDirection
            ? Quaternion.LookRotation(movementDirection)
            : Random.rotationUniform;

        GameObject obstacle = Instantiate(prefab, spawnPosition, spawnRotation, transform);

        float scaleMultiplier = Random.Range(minScale, maxScale);
        obstacle.transform.localScale *= scaleMultiplier;

        FloatingSpaceship floatingSpaceship = obstacle.GetComponent<FloatingSpaceship>();
        if (floatingSpaceship == null)
        {
            floatingSpaceship = obstacle.AddComponent<FloatingSpaceship>();
        }

        floatingSpaceship.Initialize(movementDirection, Random.Range(minSpeed, maxSpeed));
        spawnedObstacles.Add(obstacle);
        return true;
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
