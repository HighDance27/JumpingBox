using UnityEngine;

public class BoostPickup : MonoBehaviour
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
            if (GameManager.Instance != null) GameManager.Instance.ActivateBoost();

            AudioSource audio = other.GetComponent<AudioSource>();
            if (audio != null && pickupSound != null) audio.PlayOneShot(pickupSound);

            gameObject.SetActive(false);
        }
    }
}
