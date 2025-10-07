using UnityEngine;

public class Statistics : MonoBehaviour
{
    private static Statistics _instance;
    private Stats stats;

    private struct Stats
    {
        public int HostagesDied;
        public int HostagesRescued;
        public int EnemiesKilled;
        public float timeTaken;

        // Static method to initialize a Stats instance
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
                _instance = FindFirstObjectByType<Statistics>();
                if (_instance == null)
                {
                    GameObject singletonObject = new GameObject(nameof(Statistics));
                    _instance = singletonObject.AddComponent<Statistics>();
                    DontDestroyOnLoad(singletonObject);
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
        }
        else if (_instance == this)
        {
            DontDestroyOnLoad(gameObject);
        }
        stats = Stats.CreateDefault();
        stats.timeTaken = Time.time;
    }

    public void IncrementHostagesDied() => stats.HostagesDied++;
    public void IncrementHostagesRescued() => stats.HostagesRescued++;
    public void IncrementEnemiesKilled() => stats.EnemiesKilled++;
    public void CalculateTimeTaken() => stats.timeTaken = Time.time - stats.timeTaken;
}
