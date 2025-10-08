using UnityEngine;

public class Statistics : MonoBehaviour
{
    private static Statistics _instance;
    private static Stats stats;
    private static bool initialized = false;

    private struct Stats
    {
        public int HostagesDied;
        public int HostagesRescued;
        public int EnemiesKilled;
        public float timeTaken;

        public static Stats CreateDefault()
        {
            return new Stats
            {
                HostagesDied = 0,
                HostagesRescued = 0,
                EnemiesKilled = 0,
                timeTaken = 0
            };
        }
    }

    public static Statistics Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject singletonObject = new GameObject(nameof(Statistics));
                _instance = singletonObject.AddComponent<Statistics>();
                DontDestroyOnLoad(singletonObject);

                if (!initialized)
                {
                    stats = Stats.CreateDefault();
                    stats.timeTaken = Time.time;
                    initialized = true;
                }
            }

            return _instance;
        }
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);

        if (!initialized)
        {
            stats = Stats.CreateDefault();
            stats.timeTaken = Time.time;
            initialized = true;
        }
    }

    public void IncrementHostagesDied()
    {
        stats.HostagesDied++;
        Debug.Log($"HostagesDied incremented: {stats.HostagesDied}");
    }

    public void IncrementHostagesRescued()
    {
        stats.HostagesRescued++;
        Debug.Log($"HostagesRescued incremented: {stats.HostagesRescued}");
    }

    public void IncrementEnemiesKilled()
    {
        stats.EnemiesKilled++;
        Debug.Log($"EnemiesKilled incremented: {stats.EnemiesKilled}");
    }

    public void CalculateTimeTaken()
    {
        stats.timeTaken = Time.time - stats.timeTaken;
    }

    public int GetHostagesDied() => stats.HostagesDied;
    public int GetHostagesRescued() => stats.HostagesRescued;
    public int GetEnemiesKilled() => stats.EnemiesKilled;
    public float GetTimeTaken() => stats.timeTaken;

    public (int HostagesDied, int HostagesRescued, int EnemiesKilled, float timeTaken) GetStats()
    {
        return (stats.HostagesDied, stats.HostagesRescued, stats.EnemiesKilled, stats.timeTaken);
    }

    public void ResetStats()
    {
        stats = Stats.CreateDefault();
        stats.timeTaken = Time.time;
    }
}
