using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 12f;
    public int damage = 1;
    public Vector2 direction = Vector2.up;
    public float lifetime = 3f;

    bool isPlayerBullet;

    void Start()
    {
        isPlayerBullet = gameObject.layer == LayerMask.NameToLayer("PlayerBullet");
        GetComponent<Rigidbody2D>().linearVelocity = direction.normalized * speed;

        // hadapkan peluru ke arah geraknya
        float ang = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        transform.rotation = Quaternion.Euler(0f, 0f, ang);

        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        Vector3 p = transform.position;
        if (p.y > 5.3f || p.y < -5.5f || Mathf.Abs(p.x) > 3.2f)
            Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (isPlayerBullet)
        {
            var e = other.GetComponentInParent<Enemy>();
            if (e != null)
            {
                e.TakeDamage(damage);
                FX.Burst(transform.position, new Color(1f, 0.95f, 0.4f), 4, 3f, 0.08f, 0.2f);
                Destroy(gameObject);
            }
        }
        else
        {
            var p = other.GetComponentInParent<PlayerController>();
            if (p != null)
            {
                p.TakeDamage(damage);
                FX.Burst(transform.position, new Color(1f, 0.3f, 0.4f), 4, 3f, 0.08f, 0.2f);
                Destroy(gameObject);
            }
        }
    }
}