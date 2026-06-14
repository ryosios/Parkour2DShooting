using System.Threading;
using Cysharp.Threading.Tasks;
using ParkourShooter.Runtime.Audio;
using ParkourShooter.Runtime.Bosses;
using ParkourShooter.Runtime.Characters;
using ParkourShooter.Runtime.Combat;
using ParkourShooter.Runtime.Vfx;
using UnityEngine;

namespace ParkourShooter.Runtime.Skills
{
    /// <summary>
    /// プレイヤースキルの入力、クールダウン、持続効果、即時効果を制御します。
    /// </summary>
    public sealed class SkillController2D : MonoBehaviour
    {
        /// <summary>発動するスキル効果の種類です。</summary>
        [SerializeField] private SkillEffectType effectType = SkillEffectType.AttackBoost;

        /// <summary>攻撃強化を適用する AutoAttackController2D です。</summary>
        [SerializeField] private AutoAttackController2D autoAttack;

        /// <summary>持続効果が続く秒数です。</summary>
        [SerializeField] private float durationSeconds = 4f;

        /// <summary>再使用可能になるまでの秒数です。</summary>
        [SerializeField] private float cooldownSeconds = 8f;

        /// <summary>攻撃強化中のダメージ倍率です。</summary>
        [SerializeField] private float attackDamageMultiplier = 2f;

        /// <summary>攻撃強化中に追加する弾数です。</summary>
        [SerializeField] private int additionalProjectiles = 1;

        /// <summary>発動中スキルをキャンセルするための CancellationTokenSource です。</summary>
        private CancellationTokenSource activeSkillCts;

        /// <summary>次にスキルを使用できるゲーム内時刻です。</summary>
        private float nextAvailableTime;

        /// <summary>スキルが発動中かどうかです。</summary>
        private bool isActive;

        /// <summary>
        /// 未設定の AutoAttackController2D を同じ GameObject から補完します。
        /// </summary>
        private void Awake()
        {
            if (autoAttack == null)
            {
                autoAttack = GetComponent<AutoAttackController2D>();
            }
        }

        /// <summary>
        /// Space 入力でスキル発動を試みます。
        /// </summary>
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                TryActivate();
            }
        }

        /// <summary>
        /// 無効化時に発動中スキルをキャンセルします。
        /// </summary>
        private void OnDisable()
        {
            CancelActiveSkill();
        }

        /// <summary>
        /// 破棄時に発動中スキルをキャンセルします。
        /// </summary>
        private void OnDestroy()
        {
            CancelActiveSkill();
        }

        /// <summary>
        /// クールダウンと発動中状態を確認してスキルを開始します。
        /// </summary>
        private void TryActivate()
        {
            if (isActive || Time.time < nextAvailableTime)
            {
                return;
            }

            ActivateAsync(this.GetCancellationTokenOnDestroy()).Forget();
        }

        /// <summary>
        /// スキル効果を適用し、持続時間後に解除します。
        /// </summary>
        /// <param name="destroyToken">オブジェクト破棄時にキャンセルされるトークンです。</param>
        private async UniTaskVoid ActivateAsync(CancellationToken destroyToken)
        {
            isActive = true;
            nextAvailableTime = Time.time + cooldownSeconds;
            activeSkillCts = CancellationTokenSource.CreateLinkedTokenSource(destroyToken);

            ApplyInstantEffects();
            ApplyTimedEffects();
            AudioManager.Instance?.PlaySe(AudioCueType.Skill);
            if (VfxManager.Instance != null)
            {
                VfxManager.Instance.PlaySkill(transform.position, new Color(0.35f, 0.85f, 1f));
            }

            try
            {
                await UniTask.Delay(
                    Mathf.CeilToInt(durationSeconds * 1000f),
                    cancellationToken: activeSkillCts.Token);
            }
            finally
            {
                ClearTimedEffects();
                activeSkillCts?.Dispose();
                activeSkillCts = null;
                isActive = false;
            }
        }

        /// <summary>
        /// 弾消しなど、発動瞬間に完了する効果を適用します。
        /// </summary>
        private void ApplyInstantEffects()
        {
            if (effectType == SkillEffectType.BulletClear ||
                effectType == SkillEffectType.AttackBoostAndBulletClear)
            {
                BossProjectile2D.ClearAll();
            }
        }

        /// <summary>
        /// 攻撃強化など、一定時間続く効果を適用します。
        /// </summary>
        private void ApplyTimedEffects()
        {
            if (autoAttack == null)
            {
                return;
            }

            if (effectType == SkillEffectType.AttackBoost ||
                effectType == SkillEffectType.AttackBoostAndBulletClear)
            {
                autoAttack.SetSkillModifier(attackDamageMultiplier, additionalProjectiles);
            }
        }

        /// <summary>
        /// 一定時間続く効果を解除します。
        /// </summary>
        private void ClearTimedEffects()
        {
            if (autoAttack != null)
            {
                autoAttack.ClearSkillModifier();
            }
        }

        /// <summary>
        /// 発動中スキルをキャンセルし、持続効果を解除します。
        /// </summary>
        private void CancelActiveSkill()
        {
            if (activeSkillCts == null)
            {
                return;
            }

            activeSkillCts.Cancel();
            ClearTimedEffects();
        }
    }
}
