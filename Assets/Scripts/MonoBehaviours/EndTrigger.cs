using UnityEngine;

public class EndTrigger : MonoBehaviour, IPickupable
{
    [SerializeField] private Collider hostageCountingCollider;

    // IPickupable implementation
    public void Pickup(Transform transform)
    {
        // Count hostages in range


        SceneSwitcher sceneSwitcher = gameObject.AddComponent<SceneSwitcher>();
        sceneSwitcher.LoadSceneByName("Win");
    }
}
