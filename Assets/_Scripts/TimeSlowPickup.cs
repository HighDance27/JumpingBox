using UnityEngine;

public class TimeSlowPickup : MonoBehaviour
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
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ActivateTimeSlow();
            }

            if (pickupSound != null)
            {
                other.GetComponent<AudioSource>().PlayOneShot(pickupSound);
            }

            gameObject.SetActive(false);
        }
    }
}