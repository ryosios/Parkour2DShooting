using System;
using ParkourShooter.Runtime.Audio;
using ParkourShooter.Runtime.Combat;
using UnityEngine;

namespace ParkourShooter.Runtime.Bosses
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public sealed class Boss2D : MonoBehaviour, Damageable2D
    {
        public static event Action<Boss2D> Defeated;
        public event Action<int, int> HealthChanged;

        [Header("Target")]
        [SerializeField] private Transform followTarget;
        [SerializeField] private Vector2 followOffset = new(10f, 1.5f);
        [SerializeField] private float followSmoothTime = 0.18f;

        [Header("Health")]
        [SerializeField] private int maxHp = 30;

        [Header("Pattern")]
        [SerializeField] private Transform muzzle;
        [SerializeField] private float fireRate = 1.2f;
        [SerializeField] private int bulletDamage = 1;
        [SerializeField] private float bulletSpeed = 8f;
        [SerializeField] private float bulletLifetime = 5f;

        [Header("Reward")]
        [SerializeField] private int scoreValue = 1000;

        private int currentHp;
        private float nextFireTime;
        private Vector2 followVelocity;

        public bool IsAlive => currentHp > 0;
        public int ScoreValue => scoreValue;
        public int CurrentHp => currentHp;
        public int MaxHp => maxHp;

        public void SetFollowTarget(Transform target)
        {
            followTarget = target;
            followVelocity = Vector2.zero;
        }

        private void Awake()
        {
            currentHp = maxHp;

            var body = GetComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Kinematic;
            body.gravityScale = 0f;

            var bossCollider = GetComponent<Collider2D>();
            bossCollider.isTrigger = true;
        }

        private void Start()
        {
            HealthChanged?.Invoke(currentHp, maxHp);
        }

        private void FixedUpdate()
        {
            if (followTarget == null || !IsAlive)
            {
                return;
            }

            var targetPosition = (Vector2)followTarget.position + followOffset;
            var nextPosition = Vector2.SmoothDamp(transform.position, targetPosition, ref followVelocity, followSmoothTime);
            transform.position = new Vector3(nextPosition.x, nextPosition.y, 0f);
        }

        private void Update()
        {
            if (!IsAlive || Time.time < nextFireTime)
            {
                return;
            }

            Fire();
            nextFireTime = Time.time + 1f / Mathf.Max(0.01f, fireRate);
        }

        public void ApplyDamage(int damage)
        {
            if (!IsAlive)
            {
                return;
            }

            currentHp = Mathf.Max(0, currentHp - Mathf.Max(0, damage));
            HealthChanged?.Invoke(currentHp, maxHp);
            if (currentHp == 0)
            {
                Defeat();
            }
        }

        private void Fire()
        {
            var spawnPosition = muzzle != null ? muzzle.position : transform.position + Vector3.left * 1.1f;
            var bulletObject = new GameObject("BossProjectile");
            bulletObject.transform.position = spawnPosition;
            bulletObject.transform.localScale = new Vector3(0.4f, 0.22f, 1f);

            var body = bulletObject.AddComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            body.constraints = RigidbodyConstraints2D.FreezeRotation;

            var collider = bulletObject.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;

            var renderer = bulletObject.AddComponent<SpriteRenderer>();
            renderer.sprite = GeneratedCombatSprite.Get();
            renderer.color = new Color(1f, 0.2f, 0.35f);
            renderer.sortingOrder = 20;

            var projectile = bulletObject.AddComponent<BossProjectile2D>();
            projectile.Configure(bulletSpeed, bulletLifetime, bulletDamage);
            AudioManager.Instance?.PlaySe(AudioCueType.BossShot);
        }

        private void Defeat()
        {
            Defeated?.Invoke(this);
            Destroy(gameObject);
        }
    }
}
