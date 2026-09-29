using UnityEngine;

public static class SFX
{
    public enum Id { Shoot, Hit, Explode, BigExplode, Star, PlayerHurt, EnemyShoot, GameOver }

    public static float masterVolume = 0.6f;

    const int SR = 44100;
    const int VOICES = 16;

    static AudioClip[] clips;
    static AudioSource[] voices;
    static float[] lastPlayed;
    static int next;

    static void Init()
    {
        if (voices != null && voices[0] != null) return;

        var host = new GameObject("SFX");
        Object.DontDestroyOnLoad(host);
        voices = new AudioSource[VOICES];
        for (int i = 0; i < VOICES; i++)
        {
            voices[i] = host.AddComponent<AudioSource>();
            voices[i].playOnAwake = false;
        }

        int n = System.Enum.GetValues(typeof(Id)).Length;
        clips = new AudioClip[n];
        lastPlayed = new float[n];

        //                                       durasi  f awal f akhir bentuk noise lowpass decay
        clips[(int)Id.Shoot] = Synth("shoot", 0.08f, 1000, 450, 1, 0.0f, 0.25f, 2.0f);
        clips[(int)Id.Hit] = Synth("hit", 0.06f, 300, 150, 1, 0.5f, 0.50f, 3.0f);
        clips[(int)Id.Explode] = Synth("explode", 0.45f, 0, 0, 3, 1.0f, 0.08f, 2.5f);
        clips[(int)Id.BigExplode] = Synth("bigexplode", 0.90f, 0, 0, 3, 1.0f, 0.04f, 2.0f);
        clips[(int)Id.Star] = Synth("star", 0.15f, 880, 1760, 0, 0.0f, 1.00f, 2.0f);
        clips[(int)Id.PlayerHurt] = Synth("hurt", 0.30f, 320, 70, 2, 0.3f, 0.40f, 2.0f);
        clips[(int)Id.EnemyShoot] = Synth("eshoot", 0.10f, 600, 250, 0, 0.0f, 1.00f, 2.0f);
        clips[(int)Id.GameOver] = Synth("gameover", 1.00f, 300, 40, 2, 0.0f, 0.30f, 1.5f);
    }

    // shape: 0 = sine, 1 = square, 2 = saw, 3 = noise saja
    static AudioClip Synth(string name, float dur, float f0, float f1, int shape,
                           float noiseAmt, float lowpass, float decay)
    {
        int n = Mathf.RoundToInt(dur * SR);
        var data = new float[n];
        float phase = 0f, y = 0f;

        for (int i = 0; i < n; i++)
        {
            float k = (float)i / n;
            phase += Mathf.Lerp(f0, f1, k) / SR;
            phase -= Mathf.Floor(phase);

            float tone = 0f;
            if (shape == 0) tone = Mathf.Sin(phase * Mathf.PI * 2f);
            else if (shape == 1) tone = phase < 0.5f ? 1f : -1f;
            else if (shape == 2) tone = phase * 2f - 1f;

            float noise = Random.value * 2f - 1f;
            float s = tone * (1f - noiseAmt) + noise * noiseAmt;

            y += (s - y) * lowpass;                       // lowpass sederhana
            float attack = Mathf.Clamp01(i / (SR * 0.003f));
            float env = Mathf.Pow(1f - k, decay) * attack;
            data[i] = y * env * 0.8f;
        }

        var clip = AudioClip.Create(name, n, 1, SR, false);
        clip.SetData(data, 0);
        return clip;
    }

    public static void Play(Id id, float volume = 1f, float pitchVar = 0.08f)
    {
        Init();
        int i = (int)id;

        float now = Time.unscaledTime;
        if (now - lastPlayed[i] < 0.03f) return;          // cegah suara menumpuk
        lastPlayed[i] = now;

        var v = voices[next];
        next = (next + 1) % VOICES;
        v.clip = clips[i];
        v.pitch = 1f + Random.Range(-pitchVar, pitchVar);
        v.volume = volume * masterVolume;
        v.Play();
    }
}