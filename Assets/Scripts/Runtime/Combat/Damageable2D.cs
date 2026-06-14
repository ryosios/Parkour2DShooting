namespace ParkourShooter.Runtime.Combat
{
    public interface Damageable2D
    {
        bool IsAlive { get; }
        void ApplyDamage(int damage);
    }
}
