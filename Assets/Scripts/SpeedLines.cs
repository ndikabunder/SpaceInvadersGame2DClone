using UnityEngine;

public class SpeedLines : MonoBehaviour
{
    public float linesPerSecond = 10f;
    public float minSpeed = 9f, maxSpeed = 15f;
    public float minLength = 0.4f, maxLength = 1.0f;
    public float alpha = 0.18f;

    float timer;

    void Update()
    {
        timer += Time.deltaTime;
        float interval = 1f / linesPerSecond;

        while (timer >= interval)
        {
            timer -= interval;
            float len = Random.Range(minLength, maxLength);
            Vector3 size = new Vector3(0.02f, len, 1f);
            FXParticle.Spawn(FX.Square,
                new Vector3(Random.Range(-2.8f, 2.8f), 5.6f, 0f),
                new Color(1, 1, 1, alpha),
                Vector2.down * Random.Range(minSpeed, maxSpeed),
                size, size, 1.3f, 0f, -8);
        }
    }
}