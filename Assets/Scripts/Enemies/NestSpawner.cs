using System.Collections.Generic;
using UnityEngine;

public class NestSpawner : MonoBehaviour
{
    [Header("Generación de Larvas")]
    [SerializeField] private GameObject larvaPrefab;
    [SerializeField] private float spawnInterval = 5f;
    [SerializeField] private int maxActiveLarvae = 3;
    [SerializeField] private float spawnOffsetRadius = 0.6f;

    private readonly List<GameObject> spawnedLarvae = new List<GameObject>();
    private float timer;

    private void Start()
    {
        timer = spawnInterval;
    }

    private void Update()
    {
        // Limpiar de la lista las larvas que ya murieron
        spawnedLarvae.RemoveAll(larva => larva == null);

        if (spawnedLarvae.Count >= maxActiveLarvae) return;

        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            SpawnLarva();
            timer = spawnInterval;
        }
    }

    private void SpawnLarva()
    {
        if (larvaPrefab == null) return;

        Vector2 randomOffset = Random.insideUnitCircle.normalized * spawnOffsetRadius;
        Vector3 spawnPos = transform.position + (Vector3)randomOffset;

        GameObject newLarva = Instantiate(larvaPrefab, spawnPos, Quaternion.identity);
        spawnedLarvae.Add(newLarva);
    }
}