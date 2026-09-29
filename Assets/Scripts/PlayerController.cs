using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour, IDamageable
{
    [Header("Gerak")]
    public float followSpeed = 15f;
    public Vector2 touchOffset = new Vector2(0f, 1.2f);
    public float minX = -2.5f, maxX = 2.5f, minY = -4.5f, maxY = 4.0f;
    public float keyboardSpeed = 6f;

    [Header("Tembak")]
    public GameObject bulletPrefab;
    public Transform[] muzzles;
    public float fireRate = 0.15f;

    [Header("Nyawa")]
    public int maxHp = 5;
    public float invincibleTime = 1.2f;

    [Header("Juice")]
    public float maxTilt = 25f;
    public float tiltSmooth = 12f;

    int hp;
    float nextFire, invincibleUntil, tilt;
    Camera cam;
    SpriteRenderer[] renderers;
    HitFlash flash;
    Transform flame;
    Vector3 flameBase;

    void Start()
    {
        cam = Camera.main;
        hp = maxHp;
        renderers = GetComponentsInChildren<SpriteRenderer>();
        flash = gameObject.AddComponent<HitFlash>();

        flame = transform.Find("Flame");
        if (flame != null) flameBase = flame.localScale;

        GameManager.Instance.UpdateHp(hp, maxHp);
    }

    void Update()
    {
        if (GameManager.Instance.isGameOver) return;

        float oldX = transform.position.x;
        Move();
        float vx = (transform.position.x - oldX) / Mathf.Max(Time.deltaTime, 0.0001f);

        UpdateVisuals(vx);
        AutoFire();
        Blink();
    }

    void Move()
    {
        Vector3 pos = transform.position;
        var pointer = Pointer.current;

        if (pointer != null && pointer.press.isPressed)
        {
            Vector3 world = cam.ScreenToWorldPoint(pointer.position.ReadValue());
            Vector3 target = new Vector3(world.x + touchOffset.x, world.y + touchOffset.y, 0f);
            pos = Vector3.Lerp(pos, target, followSpeed * Time.deltaTime);
        }
        else if (Keyboard.current != null)
        {
            float h = (Keyboard.current.dKey.isPressed ? 1 : 0) - (Keyboard.current.aKey.isPressed ? 1 : 0);
            float v = (Keyboard.current.wKey.isPressed ? 1 : 0) - (Keyboard.current.sKey.isPressed ? 1 : 0);
            pos += new Vector3(h, v, 0f) * keyboardSpeed * Time.deltaTime;
        }

        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        pos.y = Mathf.Clamp(pos.y, minY, maxY);
        transform.position = pos;
    }

    void UpdateVisuals(float vx)
    {
        // miring sesuai arah gerak
        float target = Mathf.Clamp(-vx * 3f, -maxTilt, maxTilt);
        tilt = Mathf.Lerp(tilt, target, tiltSmooth * Time.deltaTime);
        transform.rotation = Quaternion.Euler(0f, 0f, tilt);

        // api mesin berkedip
        if (flame != null)
        {
            float f = 1f + Mathf.Sin(Time.time * 45f) * 0.2f + Random.Range(-0.1f, 0.1f);
            flame.localScale = new Vector3(flameBase.x * (1f + (f - 1f) * 0.5f), flameBase.y * f, flameBase.z);
        }
    }

    void AutoFire()
    {
        if (Time.time < nextFire) return;
        nextFire = Time.time + fireRate;

        foreach (var m in muzzles)
        {
            Instantiate(bulletPrefab, m.position, Quaternion.identity);
            FX.Burst(m.position, new Color(1f, 0.95f, 0.5f), 1, 0.3f, 0.14f, 0.07f); // kilatan moncong
        }

        SFX.Play(SFX.Id.Shoot, 0.4f);
    }

    void Blink()
    {
        bool inv = Time.time < invincibleUntil;
        bool visible = !inv || (int)(Time.time * 20f) % 2 == 0;
        foreach (var r in renderers) r.enabled = visible;
    }

    public void TakeDamage(int damage)
    {
        if (Time.time < invincibleUntil || hp <= 0) return;

        SFX.Play(SFX.Id.BigExplode);

        hp -= damage;
        invincibleUntil = Time.time + invincibleTime;
        GameManager.Instance.UpdateHp(hp, maxHp);

        if (hp <= 0)
        {
            FX.Explosion(transform.position, 1.6f);
            CameraShake.Add(1f);
            CameraShake.HitStop(0.2f);
            GameManager.Instance.GameOver();
            Destroy(gameObject);
            return;
        }

        flash.Flash(new Color(1f, 0.25f, 0.25f), 1.2f);
        SFX.Play(SFX.Id.PlayerHurt);
        FX.Burst(transform.position, new Color(1f, 0.4f, 0.3f), 8, 4f, 0.12f, 0.4f);
        CameraShake.Add(0.5f);
        CameraShake.HitStop(0.06f);
    }
}