using System;
using ParkourShooter.Runtime.Bosses;
using ParkourShooter.Runtime.Enemies;
using UnityEngine;

namespace ParkourShooter.Runtime.Score
{
    public sealed class ScoreManager : MonoBehaviour
    {
        public event Action<int> ScoreChanged;

        [SerializeField] private int currentScore;

        public int CurrentScore => currentScore;

        private void OnEnable()
        {
            Enemy2D.Defeated += OnEnemyDefeated;
            Boss2D.Defeated += OnBossDefeated;
        }

        private void OnDisable()
        {
            Enemy2D.Defeated -= OnEnemyDefeated;
            Boss2D.Defeated -= OnBossDefeated;
        }

        public void AddScore(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            currentScore += amount;
            ScoreChanged?.Invoke(currentScore);
        }

        private void OnEnemyDefeated(Enemy2D enemy)
        {
            AddScore(enemy.ScoreValue);
        }

        private void OnBossDefeated(Boss2D boss)
        {
            AddScore(boss.ScoreValue);
        }
    }
}
