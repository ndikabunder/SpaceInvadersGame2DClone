using TMPro;
using UnityEngine;

public class FXFloatText : MonoBehaviour
{
    TMP_Text tmp;
    Vector3 start;
    Color c0;
    float t, life = 0.7f;

    public void Init(TMP_Text text, float lifetime)
    {
        tmp = text; life = lifetime;
        start = transform.position;
        c0 = text.color;
    }

    void Update()
    {
        t += Time.deltaTime;
        float k = t / life;
        if (k >= 1f) { Destroy(gameObject); return; }

        // naik pelan-pelan (ease out)
        transform.position = start + Vector3.up * (1f - Mathf.Pow(1f - k, 3f)) * 0.9f;

        // muncul membesar lalu mengecil
        float s = k < 0.15f ? Mathf.Lerp(0.4f, 1.2f, k / 0.15f)
                            : Mathf.Lerp(1.2f, 1f, (k - 0.15f) / 0.85f);
        transform.localScale = Vector3.one * s;

        var c = c0;
        c.a = k > 0.6f ? 1f - (k - 0.6f) / 0.4f : 1f;
        tmp.color = c;
    }
}