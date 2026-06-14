using UnityEngine;

namespace ParkourShooter.Runtime.Cards
{
    [System.Serializable]
    public sealed class EquipmentCardDefinition
    {
        [SerializeField] private string cardName = "Attack Up";
        [SerializeField] private int scoreThreshold = 100;
        [SerializeField] private CardEffectType effectType = CardEffectType.AttackUp;
        [SerializeField] private float value = 1f;

        public string CardName => cardName;
        public int ScoreThreshold => scoreThreshold;
        public CardEffectType EffectType => effectType;
        public float Value => value;
    }
}
