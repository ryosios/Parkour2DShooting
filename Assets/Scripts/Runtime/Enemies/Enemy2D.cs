using ParkourShooter.Runtime.Combat;
using UnityEngine;

namespace ParkourShooter.Runtime.Enemies
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public sealed class Enemy2D : MonoBehaviour, Damageable2D
    {
        public static event System.Action<Enemy2D> Defeated;

        [Header("Health")]
        [SerializeField] private int maxHp = 3;

        [Header("Combat")]
        [SerializeField] private int contactDamage = 1;

        [Header("Reward")]
        [SerializeField] private int scoreValue = 100;

        private int currentHp;

        public bool IsAlive => currentHp > 0;
        public int ContactDamage => contactDamage;
        public int ScoreValue => scoreValue;

        private void Awake()
        {
            currentHp = maxHp;

            var body = GetComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Kinematic;
            body.gravityScale = 0f;

            var enemyCollider = GetComponent<Collider2D>();
            enemyCollider.isTrigger = true;
        }

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

        private void Defeat()
        {
            Defeated?.Invoke(this);
            Destroy(gameObject);
        }
    }
}
