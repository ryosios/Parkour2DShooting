using ParkourShooter.Runtime.Skills;
using UnityEngine;

namespace ParkourShooter.Runtime.Characters
{
    /// <summary>
    /// 共有プレイヤーRootの子として表示されるキャラクタースロットの設定です。
    /// </summary>
    public sealed class CharacterSlot2D : MonoBehaviour
    {
        /// <summary>このキャラクターへ切り替えた時に使用するスキル効果です。</summary>
        [SerializeField] private SkillEffectType skillEffect = SkillEffectType.AttackBoost;

        /// <summary>このキャラクターへ切り替えた時に使用するスキル効果です。</summary>
        public SkillEffectType SkillEffect => skillEffect;
    }
}
