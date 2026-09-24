using UnityEngine;

[CreateAssetMenu(fileName = "NewObstacleData", menuName = "Runner/Obstacle Data")]
public class ObstacleData : ScriptableObject
{
    public GameObject prefab;
    public Vector3 spawnOffset = Vector3.zero;

    [Range(1f, 100f)]
    public float spawnWeight = 10f;


}
