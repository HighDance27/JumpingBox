using UnityEngine;

[CreateAssetMenu(fileName = "DifficultyProgression", menuName = "Runner/Difficulty Progression")]

public class DifficultyProgressionData : ScriptableObject
{
    [Header("Score Range")]
    [Tooltip("Score at which maximum difficulty is reached")]
    public float maxDifficultyScore = 100f;

    [Header("Progression Curve")]
    [Tooltip("X: Progress (0 = start, 1 = max difficulty), Y: Normalized factor (0 to 1)")]
    public AnimationCurve difficultyCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Obstacle Speed Scaling")]
    public float baseObstacleSpeed = 10f;
    public float maxObstacleSpeed = 22f;

    [Header("Spawn Interval Scaling (Seconds)")]
    public float startMinSpawnInterval = 1.4f;
    public float startMaxSpawnInterval = 2.4f;
    public float endMinSpawnInterval = 0.6f;
    public float endMaxSpawnInterval = 1.1f;

    public float GetDifficultyProgress(int currentScore)
    {
        float rawProgress = Mathf.Clamp01(currentScore / maxDifficultyScore);
        return difficultyCurve.Evaluate(rawProgress);
    }

    public float GetObstacleSpeed(int currentScore)
    {
        float t = GetDifficultyProgress(currentScore);
        return Mathf.Lerp(baseObstacleSpeed, maxObstacleSpeed, t);
    }

    public Vector2 GetSpawnIntervalRange(int currentScore)
    {
        float t = GetDifficultyProgress(currentScore);
        float minInterval = Mathf.Lerp(startMinSpawnInterval, endMinSpawnInterval, t);
        float maxInterval = Mathf.Lerp(startMaxSpawnInterval, endMaxSpawnInterval, t);
        return new Vector2(minInterval, maxInterval);
    }
}
