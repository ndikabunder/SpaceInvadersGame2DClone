using UnityEngine;

public class HitFlash : MonoBehaviour
{
    SpriteRenderer[] rs;
    Color[] orig;
    Vector3 baseScale;
    float timer, punch = 1f;
    const float duration = 0.1f;

    void Awake()
    {
        rs = GetComponentsInChildren<SpriteRenderer>();
        orig = new Color[rs.Length];
        for (int i = 0; i < rs.Length; i++) orig[i] = rs[i].color;
        baseScale = transform.localScale;
    }

    public void Flash() { Flash(Color.white, 1.12f); }

    public void Flash(Color color, float punchScale)
    {
        timer = duration;
        punch = punchScale;
        foreach (var r in rs)
            if (r != null) r.color = new Color(color.r, color.g, color.b, r.color.a);
        transform.localScale = baseScale * punch;
    }

    void Update()
    {
        if (timer <= 0f) return;
        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            for (int i = 0; i < rs.Length; i++) if (rs[i] != null) rs[i].color = orig[i];
            transform.localScale = baseScale;
        }
        else
        {
            transform.localScale = Vector3.Lerp(baseScale, baseScale * punch, timer / duration);
        }
    }
}