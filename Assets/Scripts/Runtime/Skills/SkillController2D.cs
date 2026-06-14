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
    public sealed class SkillController2D : MonoBehaviour
    {
        [SerializeField] private SkillEffectType effectType = SkillEffectType.AttackBoost;
        [SerializeField] private AutoAttackController2D autoAttack;
        [SerializeField] private float durationSeconds = 4f;
        [SerializeField] private float cooldownSeconds = 8f;
        [SerializeField] private float attackDamageMultiplier = 2f;
        [SerializeField] private int additionalProjectiles = 1;

        private CancellationTokenSource activeSkillCts;
        private float nextAvailableTime;
        private bool isActive;

        private void Awake()
        {
            if (autoAttack == null)
            {
                autoAttack = GetComponent<AutoAttackController2D>();
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                TryActivate();
            }
        }

        private void OnDisable()
        {
            CancelActiveSkill();
        }

        private void OnDestroy()
        {
            CancelActiveSkill();
        }

        private void TryActivate()
        {
            if (isActive || Time.time < nextAvailableTime)
            {
                return;
            }

            ActivateAsync(this.GetCancellationTokenOnDestroy()).Forget();
        }

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

        private void ApplyInstantEffects()
        {
            if (effectType == SkillEffectType.BulletClear ||
                effectType == SkillEffectType.AttackBoostAndBulletClear)
            {
                BossProjectile2D.ClearAll();
            }
        }

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

        private void ClearTimedEffects()
        {
            if (autoAttack != null)
            {
                autoAttack.ClearSkillModifier();
            }
        }

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
