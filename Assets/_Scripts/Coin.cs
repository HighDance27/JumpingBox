using UnityEngine;

public class Coin : MonoBehaviour
{
    public float speed = 10f;
    public int value = 1;
    public float rotationSpeed = 120f;
    public GameObject collectVFX;
    public AudioClip collectSFX;

    private void Update()
    {
        if (GameManager.Instance != null && !GameManager.Instance.IsGamePlaying) return;

        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
        transform.Translate(Vector3.forward * -speed * Time.deltaTime, Space.World);
    }

    private void OnBecameInvisible()
    {
        gameObject.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Collect();
        }
    }

    public void Collect()
    {
        if (CoinManager.Instance != null)
        {
            int coinsToGive = value * GameManager.Instance.CurrentCoinMultiplier;
            CoinManager.Instance.AddCoins(coinsToGive);
        }

        AudioSource.PlayClipAtPoint(collectSFX, transform.position);

        GameObject vfx = PooledObject.Instance.Get(collectVFX, transform.position, Quaternion.identity);
        PooledVFXMover mover = vfx.GetComponent<PooledVFXMover>();
        mover.Initialize(speed, Vector3.forward * -1f);

        ParticleSystem ps = vfx.GetComponent<ParticleSystem>();
        ps.Clear();
        ps.Play();

        gameObject.SetActive(false);
    }
}
