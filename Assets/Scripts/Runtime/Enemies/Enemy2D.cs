using ParkourShooter.Runtime.Combat;
using UnityEngine;

namespace ParkourShooter.Runtime.Enemies
{
    /// <summary>
    /// 通常敵の HP、接触ダメージ情報、撃破スコアを管理します。
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public sealed class Enemy2D : MonoBehaviour, Damageable2D
    {
        /// <summary>敵が撃破された時に通知されるイベントです。</summary>
        public static event System.Action<Enemy2D> Defeated;

        [Header("Health")]
        /// <summary>敵の最大 HP です。</summary>
        [SerializeField] private int maxHp = 3;

        [Header("Combat")]
        /// <summary>プレイヤー接触時に与える想定ダメージです。</summary>
        [SerializeField] private int contactDamage = 1;

        [Header("Reward")]
        /// <summary>撃破時に加算されるスコアです。</summary>
        [SerializeField] private int scoreValue = 100;

        /// <summary>現在の HP です。</summary>
        private int currentHp;

        /// <summary>敵が生存しているかどうかです。</summary>
        public bool IsAlive => currentHp > 0;

        /// <summary>接触時に与える想定ダメージです。</summary>
        public int ContactDamage => contactDamage;

        /// <summary>撃破時に加算されるスコアです。</summary>
        public int ScoreValue => scoreValue;

        /// <summary>
        /// HP と Rigidbody2D、Collider2D の基本設定を初期化します。
        /// </summary>
        private void Awake()
        {
            currentHp = maxHp;

            var body = GetComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Kinematic;
            body.gravityScale = 0f;

            var enemyCollider = GetComponent<Collider2D>();
            enemyCollider.isTrigger = true;
        }

        /// <summary>
        /// 敵へダメージを適用し、HP が 0 になったら撃破します。
        /// </summary>
        /// <param name="damage">適用するダメージ量です。</param>
        public void ApplyDamage(int damage)
        {
            if (!IsAlive)
            {
                return;
            }

            currentHp = Mathf.Max(0, currentHp - Mathf.Max(0, damage));
            if (currentHp == 0)
            {
                Defeat();
            }
        }

        /// <summary>
        /// 撃破イベントを通知して敵オブジェクトを破棄します。
        /// </summary>
        private void Defeat()
        {
            Defeated?.Invoke(this);
            Destroy(gameObject);
        }
    }
}
