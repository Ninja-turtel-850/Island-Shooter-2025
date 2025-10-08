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
            Debug.Log($"Hostage at {hostage.transform.position}, in range: {hostageCountingCollider.bounds.Contains(hostage.transform.position)}");
            if (hostageCountingCollider.bounds.Contains(hostage.transform.position))
            {
                Statistics.Instance.IncrementHostagesRescued();
            }
        }

        Statistics.Instance.CalculateTimeTaken();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        SceneSwitcher sceneSwitcher = gameObject.AddComponent<SceneSwitcher>();
        sceneSwitcher.LoadSceneByName("Win");
    }
}
