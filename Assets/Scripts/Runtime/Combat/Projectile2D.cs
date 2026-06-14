using ParkourShooter.Runtime.Audio;
using UnityEngine;
using ParkourShooter.Runtime.Vfx;

namespace ParkourShooter.Runtime.Combat
{
    /// <summary>
    /// プレイヤー弾の移動、寿命、命中処理を担当します。
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public sealed class Projectile2D : MonoBehaviour
    {
        /// <summary>弾の移動速度です。</summary>
        [SerializeField] private float speed = 16f;

        /// <summary>弾が自動消滅するまでの秒数です。</summary>
        [SerializeField] private float lifetime = 3f;

        /// <summary>命中時に与えるダメージです。</summary>
        [SerializeField] private int damage = 1;

        /// <summary>弾の移動に使用する Rigidbody2D です。</summary>
        private Rigidbody2D body;

        /// <summary>弾が消滅するゲーム内時刻です。</summary>
        private float despawnTime;

        /// <summary>
        /// 現在設定されているダメージ量です。
        /// </summary>
        public int Damage => damage;

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
        /// 有効化時に寿命タイマーを設定します。
        /// </summary>
        private void OnEnable()
        {
            despawnTime = Time.time + lifetime;
        }

        /// <summary>
        /// 右方向へ移動し、寿命切れなら破棄します。
        /// </summary>
        private void FixedUpdate()
        {
            body.linearVelocity = Vector2.right * speed;

            if (Time.time >= despawnTime)
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// 弾の速度、寿命、ダメージを設定します。
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

        /// <summary>
        /// Damageable2D に命中した時にダメージ、効果音、VFX を発生させます。
        /// </summary>
        /// <param name="other">接触した Collider2D です。</param>
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
