using UnityEngine;

public class AmmoPack : MonoBehaviour, IPickupable
{
    [SerializeField] private BulletType BulletType;
    [SerializeField] private int Amount = 10;
    private Vector3 home;

    void Start()
    {
        home = transform.position;
    }

    void Update()
    {
        // slight bobbing effect for funsies
        transform.position = new Vector3(home.x, home.y + Mathf.Sin(Time.time) * 0.1f, home.z);

        // and a slow rotation
        transform.Rotate(Vector3.up, 20 * Time.deltaTime);
    }

    // Implement the IPickupable interface method
    public void Pickup(Transform picker)
    {
        // Just debug log for now. :/
        Debug.Log($"{picker.name} picked up an ammo pack!");
        if (picker.TryGetComponent<IAmmoHolder>(out var ammoHolder))
        {
            ammoHolder.AddAmmo(BulletType, Amount);
            Destroy(gameObject);
        }
    }
}
