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
    /// <summary>
    /// スコアに応じたカード獲得と、獲得済みカード効果のキャラクター反映を管理します。
    /// </summary>
    public sealed class CardManager : MonoBehaviour
    {
        /// <summary>カードを獲得した時に通知されるイベントです。</summary>
        public event Action<EquipmentCardDefinition> CardAcquired;

        /// <summary>スコア変化を監視する ScoreManager です。</summary>
        [SerializeField] private ScoreManager scoreManager;

        /// <summary>カード効果を適用するチーム管理クラスです。</summary>
        [SerializeField] private TeamController2D teamController;

        /// <summary>スコア到達で順に獲得できるカード一覧です。</summary>
        [SerializeField] private List<EquipmentCardDefinition> cards = new();

        /// <summary>獲得済みカードのインデックス一覧です。</summary>
        private readonly HashSet<int> acquiredCardIndexes = new();

        /// <summary>カードによる累計攻撃加算です。</summary>
        private float attackBonus;

        /// <summary>カードによる累計移動速度加算です。</summary>
        private float moveSpeedBonus;

        /// <summary>カードによる累計追加弾数です。</summary>
        private int projectileCountBonus;

        /// <summary>カードによる累計グレイズスコア倍率加算です。</summary>
        private float grazeMultiplierBonus;

        /// <summary>
        /// スコア変化とアクティブキャラクター切り替えを購読します。
        /// </summary>
        private void OnEnable()
        {
            if (scoreManager != null)
            {
                scoreManager.ScoreChanged += OnScoreChanged;
            }

            TeamController2D.ActiveCharacterChanged += ApplyToControlRoot;
        }

        /// <summary>
        /// 購読していたイベントを解除します。
        /// </summary>
        private void OnDisable()
        {
            if (scoreManager != null)
            {
                scoreManager.ScoreChanged -= OnScoreChanged;
            }

            TeamController2D.ActiveCharacterChanged -= ApplyToControlRoot;
        }

        /// <summary>
        /// スコア到達条件を満たした未獲得カードを獲得します。
        /// </summary>
        /// <param name="score">現在のスコアです。</param>
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

        /// <summary>
        /// カード効果を累計値へ反映し、全キャラクターへ適用します。
        /// </summary>
        /// <param name="card">獲得したカード定義です。</param>
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

        /// <summary>
        /// 現在の累計カード効果をチーム全員へ適用します。
        /// </summary>
        private void ApplyToAllCharacters()
        {
            if (teamController == null)
            {
                return;
            }

            ApplyToCharacter(teamController.ControlRoot);
        }

        /// <summary>
        /// キャラクター表示スロットが切り替わった時に共有Rootへカード効果を再適用します。
        /// </summary>
        /// <param name="_">表示中スロットです。カード効果は共有Rootへ適用します。</param>
        private void ApplyToControlRoot(Transform _)
        {
            if (teamController != null)
            {
                ApplyToCharacter(teamController.ControlRoot);
            }
        }

        /// <summary>
        /// 現在の累計カード効果を指定キャラクターへ適用します。
        /// </summary>
        /// <param name="character">効果を適用するキャラクターです。</param>
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
