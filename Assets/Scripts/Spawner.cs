using System.Collections;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public enum Formation { Line, V, Chevron, Column, Diagonal, Circle, Cluster, SideStream }
    public enum Side { Left, Right }

    [System.Serializable]
    public class Wave
    {
        public GameObject enemyPrefab;
        public Formation formation = Formation.Line;
        public int count = 5;
        public float centerX = 0f;      // pusat formasi
        public float spacing = 0.8f;    // jarak antar musuh (radius untuk Circle/Cluster)
        public float interval = 0f;     // 0 = muncul bersamaan, >0 = satu per satu
        public bool overridePattern;    // centang untuk memakai pola gerak di bawah
        public Enemy.Pattern pattern = Enemy.Pattern.Straight;
        public Side side = Side.Left;   // hanya untuk SideStream
        public float sideY = 3.5f;      // ketinggian muncul untuk SideStream
        public float delayAfter = 3f;
    }

    public Wave[] waves;
    public float spawnY = 6f;
    public float minX = -2.3f, maxX = 2.3f;

    IEnumerator Start()
    {
        yield return new WaitForSeconds(1.5f);
        while (true)
        {
            foreach (var w in waves)
            {
                for (int i = 0; i < w.count; i++)
                {
                    SpawnOne(w, i);
                    if (w.interval > 0f) yield return new WaitForSeconds(w.interval);
                }
                yield return new WaitForSeconds(w.delayAfter);
            }
        }
    }

    void SpawnOne(Wave w, int i)
    {
        Vector3 pos;
        float dirX = 1f;

        if (w.formation == Formation.SideStream)
        {
            dirX = w.side == Side.Left ? 1f : -1f;
            pos = new Vector3(-dirX * 3.2f, w.sideY, 0f);
        }
        else
        {
            Vector2 o = Offset(w, i);
            float x = Mathf.Clamp(w.centerX + o.x, minX, maxX);
            pos = new Vector3(x, spawnY + o.y, 0f);
        }

        var go = Instantiate(w.enemyPrefab, pos, Quaternion.identity);
        var e = go.GetComponent<Enemy>();
        if (e == null) return;

        e.dirX = dirX;
        e.SetPhase(i * 0.4f); // bikin zigzag bergelombang, bukan serempak

        if (w.formation == Formation.SideStream && !w.overridePattern)
            e.pattern = Enemy.Pattern.Arc;
        else if (w.overridePattern)
            e.pattern = w.pattern;
    }

    Vector2 Offset(Wave w, int i)
    {
        float c = (w.count - 1) / 2f;
        float s = w.spacing;

        switch (w.formation)
        {
            case Formation.Line: return new Vector2((i - c) * s, 0f);
            case Formation.V: return new Vector2((i - c) * s, Mathf.Abs(i - c) * s);
            case Formation.Chevron: return new Vector2((i - c) * s, (c - Mathf.Abs(i - c)) * s);
            case Formation.Column: return new Vector2(0f, i * s * 1.2f);
            case Formation.Diagonal: return new Vector2((i - c) * s, i * s * 0.8f);
            case Formation.Circle:
                float a = 360f / w.count * i * Mathf.Deg2Rad;
                return new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * s + Vector2.up * s;
            case Formation.Cluster:
                return Random.insideUnitCircle * s * 1.5f + Vector2.up * s;
        }
        return Vector2.zero;
    }

    [Header("Prefab untuk auto-isi Waves")]
    public GameObject scoutPrefab;
    public GameObject zigzagPrefab;
    public GameObject heavyPrefab;
    public GameObject bossPrefab;

    Wave W(GameObject prefab, Formation f, int count, float spacing, float interval,
           float delay, Enemy.Pattern? pattern = null, Side side = Side.Left)
    {
        return new Wave
        {
            enemyPrefab = prefab,
            formation = f,
            count = count,
            centerX = 0f,
            spacing = spacing,
            interval = interval,
            delayAfter = delay,
            overridePattern = pattern.HasValue,
            pattern = pattern ?? Enemy.Pattern.Straight,
            side = side
        };
    }

    [ContextMenu("Isi Contoh Waves")]
    void FillExampleWaves()
    {
#if UNITY_EDITOR
        UnityEditor.Undo.RecordObject(this, "Isi Contoh Waves");
#endif
        waves = new Wave[]
        {
        W(scoutPrefab,  Formation.Line,       5, 1.0f, 0f,   4f),
        W(scoutPrefab,  Formation.V,          5, 0.7f, 0f,   4f),
        W(scoutPrefab,  Formation.SideStream, 6, 0.8f, 0.5f, 4f, null, Side.Left),
        W(scoutPrefab,  Formation.SideStream, 6, 0.8f, 0.5f, 4f, null, Side.Right),
        W(zigzagPrefab, Formation.Column,     5, 0.8f, 0f,   4f, Enemy.Pattern.Sine),
        W(scoutPrefab,  Formation.Circle,     8, 0.8f, 0f,   4f),
        W(scoutPrefab,  Formation.Cluster,    6, 0.8f, 0f,   4f, Enemy.Pattern.Dive),
        W(heavyPrefab,  Formation.Line,       2, 1.6f, 0f,   6f, Enemy.Pattern.Hover),
        W(bossPrefab,   Formation.Line,       1, 1.0f, 0f,  15f),
        };
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }

    [ContextMenu("Kosongkan Waves")]
    void ClearWaves() { waves = new Wave[0]; }
}