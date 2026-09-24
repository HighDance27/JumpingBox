using UnityEngine;

public class Obstacles : MonoBehaviour
{
    [Header("Use negative values for speed")]
    public float speed;

    public void Initialize(ObstacleData data, float dynamicSpeed)
    {
        speed = -dynamicSpeed;
    }

    private void Update()
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    private void OnBecameInvisible()
    {
        gameObject.SetActive(false);
    }
}
