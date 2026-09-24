using UnityEngine;

[CreateAssetMenu(fileName = "NewBiome", menuName = "Runner/Biome Data")]
public class BiomeData : ScriptableObject
{
    [Header("Name & Score threshold")]
    public string biomeName = "Plains";
    public int scoreThreshold = 0;

    [Header("Background")]
    public Color dayColor = new Color(0.53f, 0.81f, 0.98f);
    public Color sunsetColor = new Color(0.95f, 0.51f, 0.25f);
    public Color nightColor = new Color(0.05f, 0.05f, 0.12f);

    [Header("Lighting")]
    public Color dayLightColor = new Color(1f, 0.95f, 0.84f);
    public Color nightLightColor = new Color(0.18f, 0.24f, 0.35f);

    [Header("Ground Material")]
    public Material ground;

    [Header("Biome Obstacles")]
    public ObstacleData[] biomeObstacles;
}
