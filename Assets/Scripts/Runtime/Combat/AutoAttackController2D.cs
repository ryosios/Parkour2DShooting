using ParkourShooter.Runtime.Audio;
using UnityEngine;

namespace ParkourShooter.Runtime.Combat
{
    public sealed class AutoAttackController2D : MonoBehaviour
    {
        [Header("Fire")]
        [SerializeField] private Transform muzzle;
        [SerializeField] private float fireRate = 3f;
        [SerializeField] private int projectileCount = 1;

        [Header("Projectile")]
        [SerializeField] private Projectile2D projectilePrefab;
        [SerializeField] private float projectileSpeed = 16f;
        [SerializeField] private float projectileLifetime = 3f;
        [SerializeField] private int damage = 1;
        [SerializeField] private Vector2 projectileSize = new(0.35f, 0.16f);
        [SerializeField] private float projectileSpreadY = 0.18f;

        private float nextFireTime;
        private float damageMultiplier = 1f;
        private int additionalProjectileCount;
        private float cardDamageBonus;
        private int cardProjectileCountBonus;

        public void SetCardModifier(float newDamageBonus, int newProjectileCountBonus)
        {
            cardDamageBonus = Mathf.Max(0f, newDamageBonus);
            cardProjectileCountBonus = Mathf.Max(0, newProjectileCountBonus);
        }

        public void SetSkillModifier(float newDamageMultiplier, int newAdditionalProjectileCount)
        {
            damageMultiplier = Mathf.Max(0.1f, newDamageMultiplier);
            additionalProjectileCount = Mathf.Max(0, newAdditionalProjectileCount);
        }

        public void ClearSkillModifier()
        {
            damageMultiplier = 1f;
            additionalProjectileCount = 0;
        }

        private void Update()
        {
            if (Time.time < nextFireTime)
            {
                return;
            }

            Fire();
            nextFireTime = Time.time + 1f / Mathf.Max(0.01f, fireRate);
        }

        private void Fire()
        {
            var origin = muzzle != null ? muzzle.position : transform.position + Vector3.right * 0.8f;
            var count = Mathf.Max(1, projectileCount + additionalProjectileCount + cardProjectileCountBonus);
            var centerOffset = (count - 1) * 0.5f;
            AudioManager.Instance?.PlaySe(AudioCueType.PlayerShot);

            for (var i = 0; i < count; i++)
            {
                var spawnPosition = origin + Vector3.up * ((i - centerOffset) * projectileSpreadY);
                var projectile = projectilePrefab != null
                    ? Instantiate(projectilePrefab, spawnPosition, Quaternion.identity)
                    : CreateDefaultProjectile(spawnPosition);

                projectile.Configure(
                    projectileSpeed,
                    projectileLifetime,
                    Mathf.CeilToInt((damage + cardDamageBonus) * damageMultiplier));
            }
        }

        private Projectile2D CreateDefaultProjectile(Vector3 spawnPosition)
        {
            var projectileObject = new GameObject("PlayerProjectile");
            projectileObject.transform.position = spawnPosition;
            projectileObject.transform.localScale = new Vector3(projectileSize.x, projectileSize.y, 1f);

            var body = projectileObject.AddComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            body.constraints = RigidbodyConstraints2D.FreezeRotation;

            var collider = projectileObject.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;

            var renderer = projectileObject.AddComponent<SpriteRenderer>();
            renderer.sprite = GeneratedCombatSprite.Get();
            renderer.color = new Color(1f, 0.75f, 0.25f);
            renderer.sortingOrder = 20;

            return projectileObject.AddComponent<Projectile2D>();
        }
    }
}
