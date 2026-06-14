using ParkourShooter.Runtime.Audio;
using UnityEngine;
using ParkourShooter.Runtime.Vfx;

namespace ParkourShooter.Runtime.Combat
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public sealed class Projectile2D : MonoBehaviour
    {
        [SerializeField] private float speed = 16f;
        [SerializeField] private float lifetime = 3f;
        [SerializeField] private int damage = 1;

        private Rigidbody2D body;
        private float despawnTime;

        public int Damage => damage;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }

        private void OnEnable()
        {
            despawnTime = Time.time + lifetime;
        }

        private void FixedUpdate()
        {
            body.linearVelocity = Vector2.right * speed;

            if (Time.time >= despawnTime)
            {
                Destroy(gameObject);
            }
        }

        public void Configure(float projectileSpeed, float projectileLifetime, int projectileDamage)
        {
            speed = projectileSpeed;
            lifetime = projectileLifetime;
            damage = projectileDamage;
            despawnTime = Time.time + lifetime;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.TryGetComponent(out Damageable2D damageable) || !damageable.IsAlive)
            {
                return;
            }

            damageable.ApplyDamage(damage);
            AudioManager.Instance?.PlaySe(AudioCueType.Hit);
            if (VfxManager.Instance != null)
            {
                VfxManager.Instance.PlayImpact(transform.position, new Color(1f, 0.85f, 0.25f));
            }

            Destroy(gameObject);
        }
    }
}
