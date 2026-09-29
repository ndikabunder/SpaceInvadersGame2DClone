using System.Collections;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    static CameraShake inst;

    public float maxOffset = 0.3f;
    public float decay = 1.8f;

    float trauma;
    Vector3 basePos;

    void Awake() { inst = this; basePos = transform.position; }

    public static void Add(float amount)
    {
        if (inst != null) inst.trauma = Mathf.Clamp01(inst.trauma + amount);
    }

    // Game melambat sebentar agar tabrakan terasa "berat"
    public static void HitStop(float duration)
    {
        if (inst != null) inst.StartCoroutine(inst.HitStopRoutine(duration));
    }

    IEnumerator HitStopRoutine(float d)
    {
        Time.timeScale = 0.05f;
        yield return new WaitForSecondsRealtime(d);
        Time.timeScale = 1f;
    }

    void LateUpdate()
    {
        if (trauma <= 0f) { transform.position = basePos; return; }

        float s = trauma * trauma;
        float t = Time.unscaledTime * 30f;
        float x = (Mathf.PerlinNoise(t, 10f) - 0.5f) * 2f;
        float y = (Mathf.PerlinNoise(20f, t) - 0.5f) * 2f;
        transform.position = basePos + new Vector3(x, y, 0f) * maxOffset * s;

        trauma = Mathf.Max(0f, trauma - decay * Time.unscaledDeltaTime);
    }
}