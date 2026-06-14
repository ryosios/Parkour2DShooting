using UnityEngine;
using System.Collections.Generic;

namespace ParkourShooter.Runtime.Bosses
{
    /// <summary>
    /// ボス弾の移動、寿命、スキルによる一括消去登録を担当します。
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public sealed class BossProjectile2D : MonoBehaviour
    {
        /// <summary>現在シーン上に存在するボス弾の一覧です。</summary>
        private static readonly HashSet<BossProjectile2D> ActiveProjectiles = new();

        /// <summary>ボス弾の移動速度です。</summary>
        [SerializeField] private float speed = 8f;

        /// <summary>ボス弾が自動消滅するまでの秒数です。</summary>
        [SerializeField] private float lifetime = 5f;

        /// <summary>命中時に与えるダメージです。</summary>
        [SerializeField] private int damage = 1;

        /// <summary>ボス弾の移動に使用する Rigidbody2D です。</summary>
        private Rigidbody2D body;

        /// <summary>ボス弾が消滅するゲーム内時刻です。</summary>
        private float despawnTime;

        /// <summary>現在設定されているダメージ量です。</summary>
        public int Damage => damage;

        /// <summary>
        /// シーン上の全ボス弾を破棄します。
        /// </summary>
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

        /// <summary>
        /// Rigidbody2D の基本設定を初期化します。
        /// </summary>
        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }

        /// <summary>
        /// 弾を一括消去対象へ登録し、寿命タイマーを設定します。
        /// </summary>
        private void OnEnable()
        {
            ActiveProjectiles.Add(this);
            despawnTime = Time.time + lifetime;
        }

        /// <summary>
        /// 弾を一括消去対象から解除します。
        /// </summary>
        private void OnDisable()
        {
            ActiveProjectiles.Remove(this);
        }

        /// <summary>
        /// 左方向へ移動し、寿命切れなら破棄します。
        /// </summary>
        private void FixedUpdate()
        {
            body.linearVelocity = Vector2.left * speed;

            if (Time.time >= despawnTime)
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// ボス弾の速度、寿命、ダメージを設定します。
        /// </summary>
        /// <param name="projectileSpeed">移動速度です。</param>
        /// <param name="projectileLifetime">寿命秒数です。</param>
        /// <param name="projectileDamage">命中時ダメージです。</param>
        public void Configure(float projectileSpeed, float projectileLifetime, int projectileDamage)
        {
            speed = projectileSpeed;
            lifetime = projectileLifetime;
            damage = projectileDamage;
            despawnTime = Time.time + lifetime;
        }
    }
}
