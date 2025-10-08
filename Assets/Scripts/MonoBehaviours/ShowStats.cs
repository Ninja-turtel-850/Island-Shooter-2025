using System.Collections;
using UnityEngine;

public class ShowStats : MonoBehaviour
{
    public TMPro.TextMeshProUGUI statsText;
    public float initialDelay = 2;
    public float displayInterval = 0.5f; // Time interval between displaying each stat

    private string format = "Hostages Rescued: {0}\nHostages Died: {1}\nEnemies Killed: {2}\nTime Taken: {3}";
    private string statsString;

    void Start()
    {
        var stats = Statistics.Instance.GetStats();
        float timeTaken = stats.timeTaken;
        int hours = Mathf.FloorToInt(timeTaken / 3600);
        int minutes = Mathf.FloorToInt((timeTaken % 3600) / 60);
        int seconds = Mathf.FloorToInt(timeTaken % 60);
        int milliseconds = Mathf.FloorToInt((timeTaken * 1000) % 1000);
        string formattedTime = $"{hours:D2}:{minutes:D2}:{seconds:D2}:{milliseconds:D3}";
        statsString = string.Format(format, stats.HostagesRescued, stats.HostagesDied, stats.EnemiesKilled, formattedTime);
        StartCoroutine(ShowStatsAfterInterval());
    }

    private IEnumerator ShowStatsAfterInterval()
    {
        // Show each stat after a delay
        statsText.text = "Statistics\n\n";
        yield return new WaitForSeconds(initialDelay);
        string[] statsLines = statsString.Split('\n');
        foreach (var line in statsLines)
        {
            statsText.text += line + "\n";
            yield return new WaitForSeconds(displayInterval);
        }
    }
}
