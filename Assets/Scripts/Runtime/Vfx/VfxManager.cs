using DG.Tweening;
using ParkourShooter.Runtime.Combat;
using UnityEngine;

namespace ParkourShooter.Runtime.Vfx
{
    /// <summary>
    /// 命中やスキル発動時の簡易 VFX を生成して再生します。
    /// </summary>
    public sealed class VfxManager : MonoBehaviour
    {
        /// <summary>シーン内で共有する VfxManager のインスタンスです。</summary>
        public static VfxManager Instance { get; private set; }

        /// <summary>命中 VFX の再生秒数です。</summary>
        [SerializeField] private float impactLifetime = 0.25f;

        /// <summary>スキル VFX の再生秒数です。</summary>
        [SerializeField] private float skillLifetime = 0.45f;

        /// <summary>
        /// シングルトンを初期化します。
        /// </summary>
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        /// <summary>
        /// 破棄時に共有インスタンス参照を解除します。
        /// </summary>
        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        /// <summary>
        /// 攻撃命中時の拡大フェード VFX を再生します。
        /// </summary>
        /// <param name="position">VFX を生成する位置です。</param>
        /// <param name="color">VFX の色です。</param>
        public void PlayImpact(Vector3 position, Color color)
        {
            var effect = CreateEffect("ImpactVfx", position, color, 0.25f);
            effect.transform
                .DOScale(Vector3.one * 0.9f, impactLifetime)
                .SetEase(Ease.OutQuad)
                .SetLink(effect);

            var renderer = effect.GetComponent<SpriteRenderer>();
            renderer
                .DOFade(0f, impactLifetime)
                .SetEase(Ease.OutQuad)
                .SetLink(effect)
                .OnComplete(() => Destroy(effect));
        }

        /// <summary>
        /// スキル発動時の大きめの拡大フェード VFX を再生します。
        /// </summary>
        /// <param name="position">VFX を生成する位置です。</param>
        /// <param name="color">VFX の色です。</param>
        public void PlaySkill(Vector3 position, Color color)
        {
            var effect = CreateEffect("SkillVfx", position, color, 0.8f);
            effect.transform
                .DOScale(Vector3.one * 2.4f, skillLifetime)
                .SetEase(Ease.OutCubic)
                .SetLink(effect);

            var renderer = effect.GetComponent<SpriteRenderer>();
            renderer
                .DOFade(0f, skillLifetime)
                .SetEase(Ease.OutCubic)
                .SetLink(effect)
                .OnComplete(() => Destroy(effect));
        }

        /// <summary>
        /// 白 1px スプライトを使った簡易 VFX オブジェクトを生成します。
        /// </summary>
        /// <param name="name">生成する GameObject 名です。</param>
        /// <param name="position">生成位置です。</param>
        /// <param name="color">表示色です。</param>
        /// <param name="startScale">初期スケールです。</param>
        /// <returns>生成された VFX GameObject です。</returns>
        private static GameObject CreateEffect(string name, Vector3 position, Color color, float startScale)
        {
            var effect = new GameObject(name);
            effect.transform.position = position;
            effect.transform.localScale = Vector3.one * startScale;

            var renderer = effect.AddComponent<SpriteRenderer>();
            renderer.sprite = GeneratedCombatSprite.Get();
            renderer.color = color;
            renderer.sortingOrder = 40;
            return effect;
        }
    }
}
