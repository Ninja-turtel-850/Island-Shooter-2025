public interface IAmmoHolder
{
    int GetAmmo(BulletType gunType);
    void RemoveAmmo(BulletType gunType, int amount);
    void AddAmmo(BulletType gunType, int amount);
}
