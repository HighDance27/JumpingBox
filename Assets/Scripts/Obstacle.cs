using UnityEngine;

public class Obstacles : MonoBehaviour
{
    [Header("Use negative values for speed")]
    public float speed;

    private void Update()
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    private void OnBecameInvisible()
    {
        Destroy(gameObject);
    }
}
