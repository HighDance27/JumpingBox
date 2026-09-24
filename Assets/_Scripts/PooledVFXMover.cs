using UnityEngine;

public class PooledVFXMover : MonoBehaviour
{
    public float scrollSpeed = 10f;
    public Vector3 moveDirection = Vector3.forward * -1f;

    private void Update()
    {
        transform.Translate(moveDirection * (scrollSpeed * Time.deltaTime), Space.World);
    }

    public void Initialize(float speed, Vector3 direction)
    {
        scrollSpeed = speed;
        moveDirection = direction;
    }
}
