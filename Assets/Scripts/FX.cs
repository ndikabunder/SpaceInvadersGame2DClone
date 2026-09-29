using TMPro;
using UnityEngine;

public static class FX
{
    static Sprite circle, ring, square;

    public static Sprite Circle { get { if (circle == null) circle = Make(64, 0f); return circle; } }
    public static Sprite RingSprite { get { if (ring == null) ring = Make(64, 0.8f); return ring; } }
    public static Sprite Square
    {
        get
        {
            if (square == null)
            {
                var tex = new Texture2D(4, 4);
                var px = new Color[16];
                for (int i = 0; i < 16; i++) px[i] = Color.white;
                tex.SetPixels(px); tex.Apply();
                square = Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4);
            }
            return square;
        }
    }

    // inner = 0 -> lingkaran penuh, inner = 0.8 -> cincin tipis
    static Sprite Make(int size, float inner)
    {
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        float r = size / 2f;
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(r, r));
                float a = Mathf.Clamp01(r - d);
                if (inner > 0f) a *= Mathf.Clamp01(d - r * inner);
                tex.SetPixel(x, y, new Color(1, 1, 1, a));
            }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }

    // Semburan partikel bulat ke segala arah
    public static void Burst(Vector3 pos, Color color, int count, float speed, float size, float life)
    {
        for (int i = 0; i < count; i++)
        {
            Vector2 dir = Random.insideUnitCircle.normalized;
            float sp = speed * Random.Range(0.4f, 1f);
            float sz = size * Random.Range(0.6f, 1.2f);
            FXParticle.Spawn(Circle, pos, color, dir * sp,
                Vector3.one * sz, Vector3.zero, life * Random.Range(0.7f, 1.2f), 3f, 20);
        }
    }

    // Cincin gelombang kejut
    public static void Ring(Vector3 pos, Color color, float endSize, float life)
    {
        FXParticle.Spawn(RingSprite, pos, color, Vector2.zero,
            Vector3.one * 0.2f, Vector3.one * endSize, life, 0f, 19);
    }

    // Ledakan berlapis: kilatan, gelombang, api, percikan, asap
    public static void Explosion(Vector3 pos, float s = 1f)
    {
        FXParticle.Spawn(Circle, pos, new Color(1, 1, 1, 0.9f), Vector2.zero,
            Vector3.one * 0.2f * s, Vector3.one * 1.2f * s, 0.12f, 0f, 21);
        Ring(pos, new Color(1f, 0.8f, 0.4f, 0.8f), 1.8f * s, 0.3f);
        Burst(pos, new Color(1f, 0.55f, 0.1f), Mathf.RoundToInt(10 * s), 4.5f * s, 0.28f * s, 0.5f);
        Burst(pos, new Color(1f, 0.9f, 0.4f), Mathf.RoundToInt(6 * s), 3f * s, 0.18f * s, 0.35f);
        Burst(pos, new Color(0.35f, 0.35f, 0.38f), Mathf.RoundToInt(4 * s), 1.2f, 0.4f * s, 0.8f);
    }

    // Teks melayang (skor, dll). fontSize 3 kira-kira setinggi 0,3 unit
    public static void Text(Vector3 pos, string text, Color color, float fontSize = 3f)
    {
        var go = new GameObject("FloatText");
        go.transform.position = pos;
        var tmp = go.AddComponent<TextMeshPro>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.color = color;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.sortingOrder = 30;
        go.AddComponent<FXFloatText>().Init(tmp, 0.7f);
    }
}