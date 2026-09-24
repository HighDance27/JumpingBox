using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerController : MonoBehaviour
{
    private Rigidbody rb;
    public float jumpForce;
    private int jumpsRemaining;
    private bool isDead = false;
    private Renderer playerRenderer;
    private Color originalColor;

    private AudioSource audioSource;
    public AudioClip jumpSound;
    public AudioClip gameOverSFX;

    [Header("Shield Power-Up")]
    public float invincibilityDuration = 3.0f;
    public float shieldDuration = 10.0f;
    public float blinkInterval = 0.1f;
    public UpgradeData shieldDurationUpgrade;
    public AudioClip shieldCrackSFX;
    private Coroutine invincibilityCoroutine;
    private Coroutine shieldTimerCoroutine;

    private bool isShielded = false;
    private bool isInvincible = false;

    [Header("Effects")]
    public GameObject explodeParticle;
    public ParticleSystem runDustParticles;
    private Vector3 startPos;
    private Quaternion startRot;

    [Header("Upgrades")]
    public UpgradeData jumpUpgrade;
    public UpgradeData doubleJumpUpgrade;

    // Cache raycast list to prevent garbage collection allocation every tap
    private static readonly List<RaycastResult> raycastResults = new List<RaycastResult>();

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();

        startPos = transform.position;
        startRot = transform.rotation;

        playerRenderer = GetComponent<Renderer>();
        if (playerRenderer != null)
        {
            originalColor = playerRenderer.material.color;
        }
    }

    private void Start()
    {
        SetRunningParticles(false);
    }

    private void Update()
    {
        if (isDead) return;
        if (GameManager.Instance != null && !GameManager.Instance.IsGamePlaying
        || GameManager.Instance.IsGamePaused)
            return;

        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                if (IsTouchOverUI(touch.position))

                    return;
                ExecuteJump();
            }
            return; // Prevent mouse fallback on touch devices
        }

        // For PC input
        if (Input.GetMouseButtonDown(0))
        {
            if (IsTouchOverUI(Input.mousePosition))
                return;
            ExecuteJump();
        }
    }

    private bool IsTouchOverUI(Vector2 screenPosition)
    {
        if (EventSystem.current == null) return false;

        PointerEventData eventData = new PointerEventData(EventSystem.current)
        {
            position = screenPosition
        };

        raycastResults.Clear();
        EventSystem.current.RaycastAll(eventData, raycastResults);

        return raycastResults.Count > 0;
    }

    private void ExecuteJump()
    {
        // No jumping when boosted
        if (GameManager.Instance != null && GameManager.Instance.IsBoostActive)
        {
            return;
        }

        if (jumpsRemaining <= 0) return;
        jumpsRemaining--;

        float dynamicJumpForce = jumpForce;
        if (jumpUpgrade != null)
        {
            dynamicJumpForce *= jumpUpgrade.GetCurrentValue();
        }

        // Reset vertical velocity before applying force
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        rb.AddForce(Vector3.up * dynamicJumpForce, ForceMode.Impulse);
        audioSource.PlayOneShot(jumpSound);
        SetRunningParticles(false);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            jumpsRemaining = 1 + (doubleJumpUpgrade != null ? doubleJumpUpgrade.GetCurrentLevel() : 0);
            SetRunningParticles(true);
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            int extraJumpsAllowed = doubleJumpUpgrade != null ? doubleJumpUpgrade.GetCurrentLevel() : 0;
            if (jumpsRemaining > extraJumpsAllowed)
            {
                jumpsRemaining = extraJumpsAllowed;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Obstacle") && !isDead)
        {
            if (GameManager.Instance.IsBoostActive)
            {
                other.gameObject.SetActive(false);
                if (explodeParticle != null && PooledObject.Instance != null)
                {
                    PooledObject.Instance.Get(explodeParticle, transform.position, Quaternion.identity);
                }
                return;
            }

            if (isInvincible) return;

            if (isShielded)
            {
                isShielded = false;
                if (shieldTimerCoroutine != null) StopCoroutine(shieldTimerCoroutine);

                if (playerRenderer != null)
                {
                    playerRenderer.material.color = originalColor;
                }

                if (audioSource != null && shieldCrackSFX != null)
                {
                    audioSource.PlayOneShot(shieldCrackSFX);
                }

                if (explodeParticle != null && PooledObject.Instance != null)
                {
                    PooledObject.Instance.Get(explodeParticle, transform.position, Quaternion.identity);
                }

                other.gameObject.SetActive(false);

                if (invincibilityCoroutine != null) StopCoroutine(invincibilityCoroutine);
                invincibilityCoroutine = StartCoroutine(InvincibilityRoutine());
                return;
            }

            isDead = true;
            jumpsRemaining = 0;

            CoinCollector collector = GetComponentInChildren<CoinCollector>();
            if (collector != null)
            {
                collector.SetVisualEnabled(false);
            }

            if (HapticManager.Instance != null)
            {
                HapticManager.Instance.TriggerDeathVibration();
            }

            SetRunningParticles(false);
            GameManager.Instance.GameOver(gameOverSFX.length);
            rb.linearVelocity = Vector3.zero;

            audioSource.PlayOneShot(gameOverSFX);
            if (explodeParticle != null && PooledObject.Instance != null)
            {
                PooledObject.Instance.Get(explodeParticle, transform.position, Quaternion.identity);
            }
            GetComponent<Renderer>().enabled = false;
            this.enabled = false;
        }
    }

    public void ActivateShield()
    {
        if (isShielded && shieldTimerCoroutine != null)
        {
            StopCoroutine(shieldTimerCoroutine);
        }

        isShielded = true;

        if (playerRenderer != null)
        {
            playerRenderer.material.color = Color.white;
        }

        shieldTimerCoroutine = StartCoroutine(ShieldTimerRoutine());
    }

    private IEnumerator ShieldTimerRoutine()
    {
        float finalShieldDuration = shieldDuration;
        if (shieldDurationUpgrade != null)
        {
            finalShieldDuration *= shieldDurationUpgrade.GetCurrentValue();
        }

        float warningDuration = 1.0f;
        float waitTime = finalShieldDuration - warningDuration;

        // if shield duration is somehow less than 3 seconds, skip the standard wait
        if (waitTime > 0)
        {
            yield return new WaitForSeconds(waitTime);
        }
        else
        {
            // If duration is too short, recalculate so the warning takes up the whole duration
            warningDuration = finalShieldDuration;
        }

        Renderer renderer = GetComponent<Renderer>();

        float warningBlinkInterval = 0.05f;
        float timePerLoop = warningBlinkInterval * 2f;
        int warningLoops = Mathf.CeilToInt(warningDuration / timePerLoop);

        for (int i = 0; i < warningLoops; i++)
        {
            // If the player hits something , stop blinking 
            if (!isShielded)
            {
                renderer.enabled = true;
                yield break; // Exit the coroutine 
            }

            renderer.enabled = false;
            yield return new WaitForSeconds(warningBlinkInterval);

            renderer.enabled = true;
            yield return new WaitForSeconds(warningBlinkInterval);
        }

        if (isShielded)
        {
            isShielded = false;
            if (playerRenderer != null)
            {
                playerRenderer.material.color = originalColor;
            }
        }
    }

    public void BoostInvincibility()
    {
        if (invincibilityCoroutine != null) StopCoroutine(invincibilityCoroutine);
        invincibilityCoroutine = StartCoroutine(InvincibilityRoutine());
    }

    private IEnumerator InvincibilityRoutine()
    {
        isInvincible = true;

        Renderer renderer = GetComponent<Renderer>();

        float timePerLoop = blinkInterval * 2f;
        int totalLoops = Mathf.CeilToInt(invincibilityDuration / timePerLoop);

        for (int i = 0; i < totalLoops; i++)
        {
            renderer.enabled = false;
            yield return new WaitForSeconds(blinkInterval);

            renderer.enabled = true;
            yield return new WaitForSeconds(blinkInterval);
        }
        renderer.enabled = true;
        isInvincible = false;
    }

    public void ResetPlayer()
    {
        transform.position = startPos;
        transform.rotation = startRot;

        if (playerRenderer != null)
        {
            playerRenderer.enabled = true;
            playerRenderer.material.color = originalColor;
        }

        GetComponent<Renderer>().enabled = true;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        CoinCollector collector = GetComponentInChildren<CoinCollector>();
        if (collector != null)
        {
            collector.UpdateCollectorRadius();
        }

        if (shieldTimerCoroutine != null) StopCoroutine(shieldTimerCoroutine);
        if (invincibilityCoroutine != null) StopCoroutine(invincibilityCoroutine);

        isShielded = false;
        isInvincible = false;
        isDead = false;
        jumpsRemaining = 1 + (doubleJumpUpgrade != null ? doubleJumpUpgrade.GetCurrentLevel() : 0);
        SetRunningParticles(true);
        this.enabled = true;
    }

    private void SetRunningParticles(bool play)
    {
        if (runDustParticles == null) return;

        if (play && !isDead)
        {
            if (!runDustParticles.isPlaying)
            {
                runDustParticles.Play();
            }
        }
        else
        {
            runDustParticles.Stop();
            runDustParticles.Clear();
        }
    }
}
