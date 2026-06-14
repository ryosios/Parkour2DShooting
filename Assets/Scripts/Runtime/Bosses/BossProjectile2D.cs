using UnityEngine;
using System.Collections.Generic;

namespace ParkourShooter.Runtime.Bosses
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public sealed class BossProjectile2D : MonoBehaviour
    {
        private static readonly HashSet<BossProjectile2D> ActiveProjectiles = new();

        [SerializeField] private float speed = 8f;
        [SerializeField] private float lifetime = 5f;
        [SerializeField] private int damage = 1;

        private Rigidbody2D body;
        private float despawnTime;

        public int Damage => damage;

        public static void ClearAll()
        {
            var snapshot = new List<BossProjectile2D>(ActiveProjectiles);
            foreach (var projectile in snapshot)
            {
                if (projectile != null)
                {
                    Destroy(projectile.gameObject);
                }
            }

            ActiveProjectiles.Clear();
        }

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }

        private void OnEnable()
        {
            ActiveProjectiles.Add(this);
            despawnTime = Time.time + lifetime;
        }

        private void OnDisable()
        {
            ActiveProjectiles.Remove(this);
        }

        private void FixedUpdate()
        {
            body.linearVelocity = Vector2.left * speed;

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
    }
}
