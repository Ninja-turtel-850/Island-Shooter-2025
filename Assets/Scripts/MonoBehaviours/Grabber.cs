using UnityEngine;

public class Grabber : MonoBehaviour
{
    [Header("Ray parameters")]
    public float Distance = 2f;
    public Vector3 Position;
    public Vector3 Forward;
    [SerializeField] private LayerMask Mask;

    private Ray Ray;
    private RaycastHit Hit;

    public bool TryGetPickupable(out IPickupable pickupable)
    {
        pickupable = null;

        // Create a ray from the center of the screen
        Ray = new Ray(Position, Forward);
#if UNITY_EDITOR
        Debug.DrawRay(Ray.origin, Ray.direction * Distance, Color.blue);
#endif

        // Shoot the ray and check if it hits something in the specified layer(s)
        if (!Physics.Raycast(Ray, out Hit, Distance, Mask))
            return false;

        // Check if the hit object has a component that implements the IPickupable interface
        if (!Hit.collider.gameObject.TryGetComponent<IPickupable>(out pickupable))
            return false;
        return true;
    }
}
