using UnityEngine;

public class EnemyShooter : MonoBehaviour
{
    public GameObject bulletPrefab;
    public float fireRate = 2f;
    public int bulletsPerShot = 1;
    public float spreadAngle = 20f;
    public bool aimAtPlayer = false;

    float next;

    void Start() { next = Time.time + Random.Range(0.5f, fireRate); }

    void Update()
    {
        if (transform.position.y > 4.5f) return; // jangan menembak sebelum tampak
        if (Time.time < next) return;
        next = Time.time + fireRate;
        Fire();
    }

    void Fire()
    {
        SFX.Play(SFX.Id.EnemyShoot, 0.3f);
        FX.Burst(transform.position + Vector3.down * 0.3f, new Color(1f, 0.3f, 0.4f), 3, 1.5f, 0.12f, 0.15f);
        Vector2 baseDir = Vector2.down;
        if (aimAtPlayer)
        {
            var player = FindFirstObjectByType<PlayerController>();
            if (player != null) baseDir = (player.transform.position - transform.position).normalized;
        }

        float startAngle = -spreadAngle / 2f;
        float step = bulletsPerShot > 1 ? spreadAngle / (bulletsPerShot - 1) : 0f;

        for (int i = 0; i < bulletsPerShot; i++)
        {
            float a = bulletsPerShot > 1 ? startAngle + step * i : 0f;
            Vector2 dir = Quaternion.Euler(0, 0, a) * baseDir;
            var go = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
            go.GetComponent<Bullet>().direction = dir;
        }
    }
}