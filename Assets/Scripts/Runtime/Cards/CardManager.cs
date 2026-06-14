using System.Collections.Generic;
using System;
using ParkourShooter.Runtime.Audio;
using ParkourShooter.Runtime.Characters;
using ParkourShooter.Runtime.Combat;
using ParkourShooter.Runtime.Movement;
using ParkourShooter.Runtime.Score;
using UnityEngine;

namespace ParkourShooter.Runtime.Cards
{
    public sealed class CardManager : MonoBehaviour
    {
        public event Action<EquipmentCardDefinition> CardAcquired;

        [SerializeField] private ScoreManager scoreManager;
        [SerializeField] private TeamController2D teamController;
        [SerializeField] private List<EquipmentCardDefinition> cards = new();

        private readonly HashSet<int> acquiredCardIndexes = new();
        private float attackBonus;
        private float moveSpeedBonus;
        private int projectileCountBonus;
        private float grazeMultiplierBonus;

        private void OnEnable()
        {
            if (scoreManager != null)
            {
                scoreManager.ScoreChanged += OnScoreChanged;
            }

            TeamController2D.ActiveCharacterChanged += ApplyToCharacter;
        }

        private void OnDisable()
        {
            if (scoreManager != null)
            {
                scoreManager.ScoreChanged -= OnScoreChanged;
            }

            TeamController2D.ActiveCharacterChanged -= ApplyToCharacter;
        }

        private void OnScoreChanged(int score)
        {
            for (var i = 0; i < cards.Count; i++)
            {
                if (acquiredCardIndexes.Contains(i) || score < cards[i].ScoreThreshold)
                {
                    continue;
                }

                acquiredCardIndexes.Add(i);
                ApplyCard(cards[i]);
            }
        }

        private void ApplyCard(EquipmentCardDefinition card)
        {
            switch (card.EffectType)
            {
                case CardEffectType.AttackUp:
                    attackBonus += card.Value;
                    break;
                case CardEffectType.MoveSpeedUp:
                    moveSpeedBonus += card.Value;
                    break;
                case CardEffectType.ProjectileCountUp:
                    projectileCountBonus += Mathf.RoundToInt(card.Value);
                    break;
                case CardEffectType.GrazeBonus:
                    grazeMultiplierBonus += card.Value;
                    break;
            }

            ApplyToAllCharacters();
            AudioManager.Instance?.PlaySe(AudioCueType.CardAcquired);
            CardAcquired?.Invoke(card);
        }

        private void ApplyToAllCharacters()
        {
            if (teamController == null)
            {
                return;
            }

            foreach (var character in teamController.Characters)
            {
                ApplyToCharacter(character);
            }
        }

        private void ApplyToCharacter(Transform character)
        {
            if (character == null)
            {
                return;
            }

            foreach (var autoAttack in character.GetComponents<AutoAttackController2D>())
            {
                autoAttack.SetCardModifier(attackBonus, projectileCountBonus);
            }

            foreach (var motor in character.GetComponents<ParkourPlayerMotor2D>())
            {
                motor.SetCardModifier(moveSpeedBonus, grazeMultiplierBonus);
            }
        }
    }
}
