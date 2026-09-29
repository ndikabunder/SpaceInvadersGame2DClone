using UnityEngine;

public class Pickup : MonoBehaviour
{
    public float fallSpeed = 1.5f;
    public float magnetRange = 1.8f;
    public float magnetSpeed = 8f;

    Transform player;
    Vector3 baseScale;
    Vector2 vel;
    float age;

    void Start()
    {
        if (TryGetComponent<Rigidbody2D>(out var rb))
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.linearVelocity = Vector2.zero;
        }

        baseScale = transform.localScale;
        transform.localScale = Vector3.zero;
        vel = Random.insideUnitCircle.normalized * Random.Range(1.5f, 3f); // menyebar saat muncul
    }

    static float EaseOutBack(float x)
    {
        const float c1 = 1.70158f, c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(x - 1f, 3f) + c1 * Mathf.Pow(x - 1f, 2f);
    }

    void Update()
    {
        float dt = Time.deltaTime;
        age += dt;

        // muncul membesar + berdenyut
        float pop = EaseOutBack(Mathf.Clamp01(age / 0.25f));
        float pulse = 1f + Mathf.Sin(age * 10f) * 0.1f;
        transform.localScale = baseScale * pop * pulse;

        if (player == null)
        {
            var p = FindFirstObjectByType<PlayerController>();
            if (p != null) player = p.transform;
        }

        bool magnet = age > 0.3f && player != null &&
                      Vector2.Distance(player.position, transform.position) < magnetRange;

        if (magnet)
        {
            transform.position = Vector3.MoveTowards(transform.position, player.position, magnetSpeed * dt);
        }
        else
        {
            transform.position += ((Vector3)vel + Vector3.down * fallSpeed) * dt;
            vel *= Mathf.Max(0f, 1f - 4f * dt);
        }

        transform.Rotate(0f, 0f, 180f * dt);
        if (transform.position.y < -6f) Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponentInParent<PlayerController>() == null) return;

        FX.Burst(transform.position, new Color(1f, 0.85f, 0.2f), 6, 3f, 0.1f, 0.3f);
        GameManager.Instance.AddStar(1);
        SFX.Play(SFX.Id.Star, 0.6f, 0.15f);
        Destroy(gameObject);
    }
}