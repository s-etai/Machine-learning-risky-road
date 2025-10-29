using UnityEngine;

public class TerrainSpawner : MonoBehaviour
{
    public GameObject[] terrainPrefabs;  // Different hill prefabs
    public Transform truck;              // Reference to the truck
    public float spawnDistanceAhead = 80f; // How far ahead to keep terrain
    public float segmentWidth = 40f;       // How wide each terrain prefab is

    private float nextSpawnX = 0f;

    void Start()
    {
        // Spawn a few initial segments
        for (int i = 0; i < 3; i++)
        {
            SpawnNextSegment();
        }
    }

    void Update()
    {
        // When the truck gets close to the next spawn point, add another segment
        if (truck.position.x + spawnDistanceAhead > nextSpawnX)
        {
            SpawnNextSegment();
        }
    }

    void SpawnNextSegment()
    {
        // Pick a random prefab
        GameObject prefab = terrainPrefabs[Random.Range(0, terrainPrefabs.Length)];

        // Spawn it at the correct position
        Vector3 spawnPos = new Vector3(nextSpawnX, 0f, 0f);
        Instantiate(prefab, spawnPos, Quaternion.identity);

        // Move spawn point forward
        nextSpawnX += segmentWidth;
    }

    //Remove terrain behind truck.
    void LateUpdate()
    {
        foreach (var segment in GameObject.FindGameObjectsWithTag("Terrain"))
        {
            if (segment.transform.position.x < truck.position.x - 100f)
            {
                Destroy(segment);
            }
        }
    }

    public void ResetTerrain()
    {
        nextSpawnX = 0f;
    }

}
