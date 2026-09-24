using UnityEngine;

public class ShieldPickup : MonoBehaviour
{
    public AudioClip pickupSound;
    public float speed = 10f;

    private void Update()
    {
        if (GameManager.Instance != null && !GameManager.Instance.IsGamePlaying) return;
        transform.Translate(Vector3.forward * -speed * Time.deltaTime, Space.World);
        transform.Rotate(Vector3.up, 120f * Time.deltaTime, Space.World);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                player.ActivateShield();
                if (pickupSound != null) player.GetComponent<AudioSource>().PlayOneShot(pickupSound);

                gameObject.SetActive(false);
            }
        }
    }
}
