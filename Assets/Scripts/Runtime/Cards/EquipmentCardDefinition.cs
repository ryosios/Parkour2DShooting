using UnityEngine;

namespace ParkourShooter.Runtime.Cards
{
    /// <summary>
    /// スコア到達時に獲得する装備カードの定義データです。
    /// </summary>
    [System.Serializable]
    public sealed class EquipmentCardDefinition
    {
        /// <summary>HUD に表示するカード名です。</summary>
        [SerializeField] private string cardName = "Attack Up";

        /// <summary>このカードを獲得するために必要なスコアです。</summary>
        [SerializeField] private int scoreThreshold = 100;

        /// <summary>カードが付与する効果の種類です。</summary>
        [SerializeField] private CardEffectType effectType = CardEffectType.AttackUp;

        /// <summary>効果量です。効果種別に応じて加算値または弾数として扱います。</summary>
        [SerializeField] private float value = 1f;

        /// <summary>HUD に表示するカード名です。</summary>
        public string CardName => cardName;

        /// <summary>このカードを獲得するために必要なスコアです。</summary>
        public int ScoreThreshold => scoreThreshold;

        /// <summary>カードが付与する効果の種類です。</summary>
        public CardEffectType EffectType => effectType;

        /// <summary>効果量です。</summary>
        public float Value => value;
    }
}
