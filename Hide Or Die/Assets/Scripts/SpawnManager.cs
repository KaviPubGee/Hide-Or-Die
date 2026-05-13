using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public static SpawnManager Instance;

    public Transform[] spawnPoints;

    private int nextSpawnIndex = 0;

    private void Awake()
    {
        Instance = this;
    }

    public Transform GetSpawnPoint()
    {
        Transform spawnPoint = spawnPoints[nextSpawnIndex];

        nextSpawnIndex++;
        if (nextSpawnIndex >= spawnPoints.Length)
        {
            nextSpawnIndex = 0;
        }

        return spawnPoint;
    }
}