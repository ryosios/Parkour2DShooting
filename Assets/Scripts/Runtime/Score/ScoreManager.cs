using System;
using ParkourShooter.Runtime.Bosses;
using ParkourShooter.Runtime.Enemies;
using UnityEngine;

namespace ParkourShooter.Runtime.Score
{
    /// <summary>
    /// 敵撃破、ボス撃破、グレイズなどから加算されるスコアを管理します。
    /// </summary>
    public sealed class ScoreManager : MonoBehaviour
    {
        /// <summary>スコアが変化した時に現在値を通知するイベントです。</summary>
        public event Action<int> ScoreChanged;

        /// <summary>現在のスコアです。</summary>
        [SerializeField] private int currentScore;

        /// <summary>現在のスコアです。</summary>
        public int CurrentScore => currentScore;

        /// <summary>
        /// 敵とボスの撃破イベントを購読します。
        /// </summary>
        private void OnEnable()
        {
            Enemy2D.Defeated += OnEnemyDefeated;
            Boss2D.Defeated += OnBossDefeated;
        }

        /// <summary>
        /// 敵とボスの撃破イベント購読を解除します。
        /// </summary>
        private void OnDisable()
        {
            Enemy2D.Defeated -= OnEnemyDefeated;
            Boss2D.Defeated -= OnBossDefeated;
        }

        /// <summary>
        /// 指定量のスコアを加算し、変更イベントを通知します。
        /// </summary>
        /// <param name="amount">加算するスコア量です。</param>
        public void AddScore(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            currentScore += amount;
            ScoreChanged?.Invoke(currentScore);
        }

        /// <summary>
        /// 敵撃破時に敵の報酬スコアを加算します。
        /// </summary>
        /// <param name="enemy">撃破された敵です。</param>
        private void OnEnemyDefeated(Enemy2D enemy)
        {
            AddScore(enemy.ScoreValue);
        }

        /// <summary>
        /// ボス撃破時にボスの報酬スコアを加算します。
        /// </summary>
        /// <param name="boss">撃破されたボスです。</param>
        private void OnBossDefeated(Boss2D boss)
        {
            AddScore(boss.ScoreValue);
        }
    }
}
