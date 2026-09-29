using UnityEngine;

public class Enemy : MonoBehaviour, IDamageable
{
    public enum Pattern { Straight, Sine, Boss, Diagonal, Arc, Hover, Dive }

    public Pattern pattern = Pattern.Straight;
    public int hp = 1;
    public float speed = 3f;
    public int score = 100;

    [Header("Sine")]
    public float amplitude = 1.5f;
    public float frequency = 2f;

    [Header("Diagonal / Arc (diisi Spawner)")]
    public float dirX = 1f;          // +1 = bergerak ke kanan, -1 = ke kiri
    public float turnRate = 60f;     // derajat/detik untuk Arc

    [Header("Hover")]
    public float hoverY = 3f;
    public float hoverTime = 3f;

    [Header("Dive")]
    public float diveY = 2.5f;
    public float diveSpeedMultiplier = 2.5f;

    [Header("Drop")]
    public GameObject starPrefab;
    public int starCount = 1;

    float startX, t, phase, hoverTimer;
    bool leaving, diving;
    Vector2 arcDir, diveDir;
    HitFlash flash;
    bool dead;

    public void SetPhase(float p) { phase = p; }

    void Start()
    {
        startX = transform.position.x;
        if (dirX == 0f) dirX = 1f;
        arcDir = new Vector2(dirX, -0.2f).normalized;

        if (TryGetComponent<Rigidbody2D>(out var rb))
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.linearVelocity = Vector2.zero;
        }

        if (!TryGetComponent(out flash)) flash = gameObject.AddComponent<HitFlash>();
    }

    void Update()
    {
        float dt = Time.deltaTime;
        t += dt;
        Vector3 p = transform.position;

        switch (pattern)
        {
            case Pattern.Straight:
                p.y -= speed * dt;
                break;

            case Pattern.Sine:
                p.y -= speed * dt;
                p.x = startX + Mathf.Sin((t + phase) * frequency) * amplitude;
                break;

            case Pattern.Boss:
                if (p.y > 3.2f) p.y -= speed * dt;
                else p.x = Mathf.Sin(t * 0.8f) * 1.6f;
                break;

            case Pattern.Diagonal:
                p += new Vector3(dirX * speed * 0.6f, -speed * 0.8f, 0f) * dt;
                break;

            case Pattern.Arc:
                arcDir = Quaternion.Euler(0, 0, -dirX * turnRate * dt) * arcDir;
                p += (Vector3)(arcDir * speed * dt);
                break;

            case Pattern.Hover:
                if (leaving) p.y -= speed * 2f * dt;
                else if (p.y > hoverY) p.y -= speed * dt;
                else
                {
                    hoverTimer += dt;
                    if (hoverTimer >= hoverTime) leaving = true;
                }
                break;

            case Pattern.Dive:
                if (!diving)
                {
                    p.y -= speed * dt;
                    if (p.y <= diveY)
                    {
                        var pl = FindFirstObjectByType<PlayerController>();
                        diveDir = pl != null
                            ? ((Vector2)(pl.transform.position - p)).normalized
                            : Vector2.down;
                        diving = true;
                    }
                }
                else p += (Vector3)(diveDir * speed * diveSpeedMultiplier * dt);
                break;
        }

        transform.position = p;

        if (p.y < -7f || p.y > 14f || Mathf.Abs(p.x) > 5f) Destroy(gameObject);
    }

    public void TakeDamage(int damage)
    {
        if (transform.position.y > 4.8f) return; // kebal selama di atas layar
        hp -= damage;
        if (hp <= 0) { Die(); return; }
        if (flash != null) flash.Flash();
        SFX.Play(SFX.Id.Hit, 0.5f);
    }

    void Die()
    {
        if (dead) return;   // mencegah skor dihitung dua kali
        dead = true;

        bool boss = pattern == Pattern.Boss;
        bool big = boss || score >= 500;


        SFX.Play(big ? SFX.Id.BigExplode : SFX.Id.Explode, big ? 1f : 0.7f);

        GameManager.Instance.AddScore(score);



        Vector3 pos = transform.position;

        FX.Explosion(pos, big ? 2f : 1f);
        FX.Text(pos + Vector3.up * 0.4f, "+" + score,
                big ? new Color(1f, 0.85f, 0.2f) : Color.white);
        CameraShake.Add(big ? 0.6f : 0.1f);

        if (boss)
        {
            for (int i = 0; i < 6; i++)
                FX.Explosion(pos + (Vector3)(Random.insideUnitCircle * 1.2f), 1.2f);
            CameraShake.HitStop(0.15f);
        }

        if (starPrefab != null)
            for (int i = 0; i < starCount; i++)
                Instantiate(starPrefab, pos + (Vector3)Random.insideUnitCircle * 0.4f, Quaternion.identity);

        Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        var player = other.GetComponentInParent<PlayerController>();
        if (player != null)
        {
            player.TakeDamage(1);
            if (pattern != Pattern.Boss) hp = 0; Die2();
        }
    }

    void Die2() { if (hp <= 0) Die(); }
}