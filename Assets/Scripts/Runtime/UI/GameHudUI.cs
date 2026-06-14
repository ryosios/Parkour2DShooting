using ParkourShooter.Runtime.Bosses;
using ParkourShooter.Runtime.Cards;
using ParkourShooter.Runtime.Characters;
using ParkourShooter.Runtime.Score;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace ParkourShooter.Runtime.UI
{
    /// <summary>
    /// スコア、アクティブキャラクター、ボス HP、獲得カードを表示する HUD です。
    /// </summary>
    public sealed class GameHudUI : MonoBehaviour
    {
        /// <summary>スコア表示元の ScoreManager です。</summary>
        [SerializeField] private ScoreManager scoreManager;

        /// <summary>カード獲得イベントを購読する CardManager です。</summary>
        [SerializeField] private CardManager cardManager;

        /// <summary>アクティブキャラクター表示元の TeamController2D です。</summary>
        [SerializeField] private TeamController2D teamController;

        /// <summary>HP 表示元の Boss2D です。</summary>
        [SerializeField] private Boss2D boss;

        [Header("Text")]
        /// <summary>スコア表示用テキストです。</summary>
        [SerializeField] private Text scoreText;

        /// <summary>現在のアクティブキャラクター表示用テキストです。</summary>
        [SerializeField] private Text activeCharacterText;

        /// <summary>ボス HP 表示用テキストです。</summary>
        [SerializeField] private Text bossHpText;

        /// <summary>獲得カード一覧表示用テキストです。</summary>
        [SerializeField] private Text cardText;

        /// <summary>
        /// HUD 更新に必要なイベントを購読します。
        /// </summary>
        private void OnEnable()
        {
            if (scoreManager != null)
            {
                scoreManager.ScoreChanged += UpdateScore;
            }

            if (cardManager != null)
            {
                cardManager.CardAcquired += AddCard;
            }

            if (boss != null)
            {
                boss.HealthChanged += UpdateBossHp;
            }

            TeamController2D.ActiveCharacterChanged += UpdateActiveCharacter;
        }

        /// <summary>
        /// 初期表示を現在のゲーム状態に合わせます。
        /// </summary>
        private void Start()
        {
            UpdateScore(scoreManager != null ? scoreManager.CurrentScore : 0);
            UpdateActiveCharacter(teamController != null ? teamController.ActiveCharacter : null);
            UpdateBossHp(boss != null ? boss.CurrentHp : 0, boss != null ? boss.MaxHp : 0);

            if (cardText != null)
            {
                cardText.text = "Cards: -";
            }
        }

        /// <summary>
        /// HUD 更新イベントの購読を解除します。
        /// </summary>
        private void OnDisable()
        {
            if (scoreManager != null)
            {
                scoreManager.ScoreChanged -= UpdateScore;
            }

            if (cardManager != null)
            {
                cardManager.CardAcquired -= AddCard;
            }

            if (boss != null)
            {
                boss.HealthChanged -= UpdateBossHp;
            }

            TeamController2D.ActiveCharacterChanged -= UpdateActiveCharacter;
        }

        /// <summary>
        /// スコア表示を更新します。
        /// </summary>
        /// <param name="score">現在のスコアです。</param>
        private void UpdateScore(int score)
        {
            if (scoreText != null)
            {
                scoreText.text = $"Score: {score}";
            }
        }

        /// <summary>
        /// アクティブキャラクター名の表示を更新します。
        /// </summary>
        /// <param name="activeCharacter">現在操作中のキャラクターです。</param>
        private void UpdateActiveCharacter(Transform activeCharacter)
        {
            if (activeCharacterText != null)
            {
                activeCharacterText.text = activeCharacter != null
                    ? $"Active: {activeCharacter.name}"
                    : "Active: -";
            }
        }

        /// <summary>
        /// ボス HP 表示を更新します。
        /// </summary>
        /// <param name="currentHp">現在 HP です。</param>
        /// <param name="maxHp">最大 HP です。</param>
        private void UpdateBossHp(int currentHp, int maxHp)
        {
            if (bossHpText != null)
            {
                bossHpText.text = maxHp > 0 ? $"Boss HP: {currentHp}/{maxHp}" : "Boss HP: -";
            }
        }

        /// <summary>
        /// 獲得カード名を HUD に追加し、簡易アニメーションを再生します。
        /// </summary>
        /// <param name="card">獲得したカード定義です。</param>
        private void AddCard(EquipmentCardDefinition card)
        {
            if (cardText == null || card == null)
            {
                return;
            }

            if (cardText.text == "Cards: -")
            {
                cardText.text = $"Cards: {card.CardName}";
            }
            else
            {
                cardText.text += $", {card.CardName}";
            }

            cardText.transform
                .DOPunchScale(Vector3.one * 0.15f, 0.25f, 6, 0.5f)
                .SetLink(cardText.gameObject);
        }
    }
}
