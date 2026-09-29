using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public TMP_Text scoreText, starText;
    public Image hpFill;
    public GameObject gameOverPanel;

    public bool isGameOver;

    int score, stars;
    float displayScore;
    float hpTarget = 1f;
    float scorePunch, starPunch, hpShake, overlayAlpha;

    Image overlay;
    RectTransform hpRoot;
    Vector2 hpRootBase;

    static readonly Color hpGreen = new Color(0.24f, 0.86f, 0.52f);
    static readonly Color hpRed = new Color(1f, 0.25f, 0.25f);

    void Awake()
    {
        Instance = this;
        Time.timeScale = 1f;

        hpRoot = hpFill.transform.parent as RectTransform;
        if (hpRoot != null) hpRootBase = hpRoot.anchoredPosition;

        CreateOverlay();
    }

    // Layar berkedip merah saat kena damage (dibuat lewat kode)
    void CreateOverlay()
    {
        var go = new GameObject("DamageOverlay", typeof(RectTransform), typeof(Image));
        go.transform.SetParent(hpFill.canvas.rootCanvas.transform, false);
        go.transform.SetAsFirstSibling();

        var rt = (RectTransform)go.transform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;

        overlay = go.GetComponent<Image>();
        overlay.color = new Color(1f, 0.1f, 0.1f, 0f);
        overlay.raycastTarget = false;
    }

    void Update()
    {
        float dt = Time.unscaledDeltaTime;

        // skor menghitung naik
        if (displayScore < score)
        {
            displayScore = Mathf.MoveTowards(displayScore, score,
                Mathf.Max(300f, (score - displayScore) * 6f) * dt);
            scoreText.text = Mathf.RoundToInt(displayScore).ToString("N0");
        }

        // angka berdenyut
        scorePunch = Mathf.Lerp(scorePunch, 0f, 12f * dt);
        starPunch = Mathf.Lerp(starPunch, 0f, 12f * dt);
        scoreText.transform.localScale = Vector3.one * (1f + scorePunch);
        starText.transform.localScale = Vector3.one * (1f + starPunch);

        // HP bar halus dan berubah warna
        hpFill.fillAmount = Mathf.Lerp(hpFill.fillAmount, hpTarget, 10f * dt);
        hpFill.color = Color.Lerp(hpRed, hpGreen, hpTarget);

        // HP bar bergetar
        if (hpRoot != null && hpShake > 0f)
        {
            hpShake -= dt;
            Vector2 off = hpShake > 0f ? Random.insideUnitCircle * 10f * (hpShake / 0.3f) : Vector2.zero;
            hpRoot.anchoredPosition = hpRootBase + off;
        }

        // kedip merah memudar
        if (overlayAlpha > 0f)
        {
            overlayAlpha = Mathf.MoveTowards(overlayAlpha, 0f, 1.5f * dt);
            overlay.color = new Color(1f, 0.1f, 0.1f, overlayAlpha);
        }
    }

    public void AddScore(int v) { score += v; scorePunch = 0.25f; }

    public void AddStar(int v)
    {
        stars += v;
        starText.text = "★ " + stars;
        starPunch = 0.3f;
    }

    public void UpdateHp(int hp, int max)
    {
        float target = Mathf.Clamp01((float)hp / max);
        if (target < hpTarget)
        {
            hpShake = 0.3f;
            overlayAlpha = 0.35f;
        }
        hpTarget = target;
    }

    public void GameOver()
    {
        if (isGameOver) return;
        isGameOver = true;
        StartCoroutine(ShowGameOver());
    }

    IEnumerator ShowGameOver()
    {
        yield return new WaitForSecondsRealtime(1f); // beri waktu melihat ledakan
        SFX.Play(SFX.Id.GameOver, 0.8f, 0f);
        gameOverPanel.SetActive(true);
        if (!gameOverPanel.TryGetComponent<CanvasGroup>(out var cg))
            cg = gameOverPanel.AddComponent<CanvasGroup>();

        for (float t = 0f; t < 0.5f; t += Time.unscaledDeltaTime)
        {
            cg.alpha = t / 0.5f;
            yield return null;
        }
        cg.alpha = 1f;
    }

    public void Restart() => SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
}