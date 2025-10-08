using UnityEngine;

public class EndTrigger : MonoBehaviour, IPickupable
{
    [SerializeField] private Collider hostageCountingCollider;

    // IPickupable implementation  
    public void Pickup(Transform transform)
    {
        // Count hostages in range
        foreach (var hostage in FindObjectsByType<Hostage>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
        {
            if (hostageCountingCollider.bounds.Contains(hostage.transform.position))
            {
                Statistics.Instance.IncrementHostagesRescued();
            }
        }

        Statistics.Instance.CalculateTimeTaken();

        SceneSwitcher sceneSwitcher = gameObject.AddComponent<SceneSwitcher>();
        sceneSwitcher.LoadSceneByName("Win");
    }
}
