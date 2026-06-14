using ParkourShooter.Runtime.Audio;
using UnityEngine;

namespace ParkourShooter.Runtime.Combat
{
    /// <summary>
    /// 一定間隔で右方向へ弾を自動発射するプレイヤー用攻撃コントローラーです。
    /// </summary>
    public sealed class AutoAttackController2D : MonoBehaviour
    {
        [Header("Fire")]
        /// <summary>弾を生成する発射位置です。</summary>
        [SerializeField] private Transform muzzle;

        /// <summary>1 秒あたりの発射回数です。</summary>
        [SerializeField] private float fireRate = 3f;

        /// <summary>基本の同時発射数です。</summary>
        [SerializeField] private int projectileCount = 1;

        [Header("Projectile")]
        /// <summary>任意で使用する弾プレハブです。未設定時はランタイム生成します。</summary>
        [SerializeField] private Projectile2D projectilePrefab;

        /// <summary>弾の移動速度です。</summary>
        [SerializeField] private float projectileSpeed = 16f;

        /// <summary>弾が自動消滅するまでの秒数です。</summary>
        [SerializeField] private float projectileLifetime = 3f;

        /// <summary>弾の基本ダメージです。</summary>
        [SerializeField] private int damage = 1;

        /// <summary>ランタイム生成する弾の表示サイズです。</summary>
        [SerializeField] private Vector2 projectileSize = new(0.35f, 0.16f);

        /// <summary>複数発射時の Y 方向の間隔です。</summary>
        [SerializeField] private float projectileSpreadY = 0.18f;

        /// <summary>次に発射できるゲーム内時刻です。</summary>
        private float nextFireTime;

        /// <summary>スキルによるダメージ倍率です。</summary>
        private float damageMultiplier = 1f;

        /// <summary>スキルによる追加弾数です。</summary>
        private int additionalProjectileCount;

        /// <summary>カードによるダメージ加算です。</summary>
        private float cardDamageBonus;

        /// <summary>カードによる追加弾数です。</summary>
        private int cardProjectileCountBonus;

        /// <summary>
        /// 装備カードによる攻撃補正を設定します。
        /// </summary>
        /// <param name="newDamageBonus">ダメージ加算値です。</param>
        /// <param name="newProjectileCountBonus">追加弾数です。</param>
        public void SetCardModifier(float newDamageBonus, int newProjectileCountBonus)
        {
            cardDamageBonus = Mathf.Max(0f, newDamageBonus);
            cardProjectileCountBonus = Mathf.Max(0, newProjectileCountBonus);
        }

        /// <summary>
        /// スキル中の攻撃補正を設定します。
        /// </summary>
        /// <param name="newDamageMultiplier">ダメージ倍率です。</param>
        /// <param name="newAdditionalProjectileCount">追加弾数です。</param>
        public void SetSkillModifier(float newDamageMultiplier, int newAdditionalProjectileCount)
        {
            damageMultiplier = Mathf.Max(0.1f, newDamageMultiplier);
            additionalProjectileCount = Mathf.Max(0, newAdditionalProjectileCount);
        }

        /// <summary>
        /// スキルによる攻撃補正を解除します。
        /// </summary>
        public void ClearSkillModifier()
        {
            damageMultiplier = 1f;
            additionalProjectileCount = 0;
        }

        /// <summary>
        /// 発射間隔を監視し、発射可能なタイミングで弾を生成します。
        /// </summary>
        private void Update()
        {
            if (Time.time < nextFireTime)
            {
                return;
            }

            Fire();
            nextFireTime = Time.time + 1f / Mathf.Max(0.01f, fireRate);
        }

        /// <summary>
        /// 現在の補正を反映した弾を生成して発射します。
        /// </summary>
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

        /// <summary>
        /// プレハブ未設定時に使用する簡易弾オブジェクトを生成します。
        /// </summary>
        /// <param name="spawnPosition">弾を生成するワールド座標です。</param>
        /// <returns>生成された Projectile2D です。</returns>
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
