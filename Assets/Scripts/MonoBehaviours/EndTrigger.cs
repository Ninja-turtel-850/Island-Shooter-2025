using UnityEngine;

public class EndTrigger : MonoBehaviour, IPickupable
{
    [SerializeField] private Collider hostageCountingCollider;
    [SerializeField] private Animator animator; // ← voeg animator toe
    [SerializeField] private float animationDuration = 3f; // tijdsduur voordat de scene laadt

    private bool hasPlayed = false;

    // IPickupable implementation  
    public void Pickup(Transform transform)
    {
        if (hasPlayed) return; // voorkom dubbel activeren
        hasPlayed = true;

        // ✅ Start animatie
        if (animator != null)
        {
            animator.SetTrigger("Play");
        }

        // Start coroutine zodat de scene pas laadt na de animatie
        transform.GetComponent<MonoBehaviour>().StartCoroutine(HandleEndSequence());
    }

    private System.Collections.IEnumerator HandleEndSequence()
    {
        // Tel de gegijzelden in range
        foreach (var hostage in FindObjectsByType<Hostage>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
        {
            Debug.Log($"Hostage at {hostage.transform.position}, in range: {hostageCountingCollider.bounds.Contains(hostage.transform.position)}");
            if (hostageCountingCollider.bounds.Contains(hostage.transform.position))
            {
                Statistics.Instance.IncrementHostagesRescued();
            }
        }

        Statistics.Instance.CalculateTimeTaken();

        // Wacht tot animatie klaar is
        yield return new WaitForSeconds(animationDuration);

        // Laad de Win-scene
        SceneSwitcher sceneSwitcher = gameObject.AddComponent<SceneSwitcher>();
        sceneSwitcher.LoadSceneByName("Win");
    }
}
