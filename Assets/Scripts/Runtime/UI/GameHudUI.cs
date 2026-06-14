using ParkourShooter.Runtime.Bosses;
using ParkourShooter.Runtime.Cards;
using ParkourShooter.Runtime.Characters;
using ParkourShooter.Runtime.Score;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace ParkourShooter.Runtime.UI
{
    public sealed class GameHudUI : MonoBehaviour
    {
        [SerializeField] private ScoreManager scoreManager;
        [SerializeField] private CardManager cardManager;
        [SerializeField] private TeamController2D teamController;
        [SerializeField] private Boss2D boss;

        [Header("Text")]
        [SerializeField] private Text scoreText;
        [SerializeField] private Text activeCharacterText;
        [SerializeField] private Text bossHpText;
        [SerializeField] private Text cardText;

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

        private void UpdateScore(int score)
        {
            if (scoreText != null)
            {
                scoreText.text = $"Score: {score}";
            }
        }

        private void UpdateActiveCharacter(Transform activeCharacter)
        {
            if (activeCharacterText != null)
            {
                activeCharacterText.text = activeCharacter != null
                    ? $"Active: {activeCharacter.name}"
                    : "Active: -";
            }
        }

        private void UpdateBossHp(int currentHp, int maxHp)
        {
            if (bossHpText != null)
            {
                bossHpText.text = maxHp > 0 ? $"Boss HP: {currentHp}/{maxHp}" : "Boss HP: -";
            }
        }

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
