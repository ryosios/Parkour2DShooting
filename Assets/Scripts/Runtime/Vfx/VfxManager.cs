using DG.Tweening;
using ParkourShooter.Runtime.Combat;
using UnityEngine;

namespace ParkourShooter.Runtime.Vfx
{
    public sealed class VfxManager : MonoBehaviour
    {
        public static VfxManager Instance { get; private set; }

        [SerializeField] private float impactLifetime = 0.25f;
        [SerializeField] private float skillLifetime = 0.45f;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

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
