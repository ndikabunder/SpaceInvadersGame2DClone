using UnityEngine;

public class FXParticle : MonoBehaviour
{
    SpriteRenderer sr;
    Vector2 vel;
    Vector3 s0, s1;
    Color c0;
    float life, t, drag;

    public static FXParticle Spawn(Sprite sprite, Vector3 pos, Color color, Vector2 vel,
                                   Vector3 startScale, Vector3 endScale,
                                   float life, float drag, int order)
    {
        var go = new GameObject("fx");
        go.transform.position = pos;
        go.transform.localScale = startScale;

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.color = color;
        sr.sortingOrder = order;

        var p = go.AddComponent<FXParticle>();
        p.sr = sr; p.vel = vel; p.s0 = startScale; p.s1 = endScale;
        p.c0 = color; p.life = Mathf.Max(0.01f, life); p.drag = drag;
        return p;
    }

    void Update()
    {
        float dt = Time.deltaTime;
        t += dt;
        float k = t / life;
        if (k >= 1f) { Destroy(gameObject); return; }

        transform.position += (Vector3)vel * dt;
        vel *= Mathf.Max(0f, 1f - drag * dt);
        transform.localScale = Vector3.Lerp(s0, s1, k);

        var c = c0;
        c.a = c0.a * (1f - k);
        sr.color = c;
    }
}