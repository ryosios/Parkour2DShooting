using System;
using System.Collections.Generic;
using ParkourShooter.Runtime.Audio;
using ParkourShooter.Runtime.Bosses;
using ParkourShooter.Runtime.Combat;
using ParkourShooter.Runtime.Movement;
using ParkourShooter.Runtime.Skills;
using Unity.Cinemachine;
using UnityEngine;

namespace ParkourShooter.Runtime.Characters
{
    public sealed class TeamController2D : MonoBehaviour
    {
        public static event Action<Transform> ActiveCharacterChanged;

        [SerializeField] private List<Transform> characters = new();
        [SerializeField] private CinemachineCamera followCamera;
        [SerializeField] private Boss2D boss;
        [SerializeField] private float transitionSeconds = 0.18f;

        private int activeIndex;
        private bool isSwitching;

        public Transform ActiveCharacter => characters.Count == 0 ? null : characters[activeIndex];
        public IReadOnlyList<Transform> Characters => characters;

        private void Start()
        {
            SetActiveCharacter(0, true);
        }

        private void Update()
        {
            if (isSwitching || characters.Count <= 1)
            {
                return;
            }

            if (Input.GetKeyDown(KeyCode.A))
            {
                SwitchTo(activeIndex - 1);
            }
            else if (Input.GetKeyDown(KeyCode.D))
            {
                SwitchTo(activeIndex + 1);
            }
        }

        private void SwitchTo(int requestedIndex)
        {
            var nextIndex = (requestedIndex + characters.Count) % characters.Count;
            if (nextIndex == activeIndex)
            {
                return;
            }

            StartCoroutine(SwitchRoutine(nextIndex));
        }

        private System.Collections.IEnumerator SwitchRoutine(int nextIndex)
        {
            isSwitching = true;

            var previous = characters[activeIndex];
            var next = characters[nextIndex];
            next.position = previous.position;
            next.gameObject.SetActive(true);
            SetCharacterControl(next, false);
            SetCharacterVisualAlpha(previous, 1f);
            SetCharacterVisualAlpha(next, 0.35f);

            yield return new WaitForSeconds(transitionSeconds);

            previous.gameObject.SetActive(false);
            SetCharacterVisualAlpha(previous, 1f);
            SetCharacterVisualAlpha(next, 1f);
            activeIndex = nextIndex;
            SetActiveCharacter(activeIndex, true);
            AudioManager.Instance?.PlaySe(AudioCueType.CharacterSwitch);
            isSwitching = false;
        }

        private void SetActiveCharacter(int index, bool snapReferences)
        {
            activeIndex = Mathf.Clamp(index, 0, characters.Count - 1);

            for (var i = 0; i < characters.Count; i++)
            {
                var character = characters[i];
                var isActive = i == activeIndex;
                character.gameObject.SetActive(isActive);
                SetCharacterControl(character, isActive);
            }

            var active = ActiveCharacter;
            if (active == null)
            {
                return;
            }

            if (followCamera != null)
            {
                followCamera.Follow = active;
            }

            if (boss != null)
            {
                boss.SetFollowTarget(active);
            }

            ActiveCharacterChanged?.Invoke(active);
        }

        private static void SetCharacterControl(Transform character, bool enabled)
        {
            foreach (var motor in character.GetComponents<ParkourPlayerMotor2D>())
            {
                motor.enabled = enabled;
            }

            foreach (var autoAttack in character.GetComponents<AutoAttackController2D>())
            {
                autoAttack.enabled = enabled;
            }

            foreach (var skill in character.GetComponents<SkillController2D>())
            {
                skill.enabled = enabled;
            }

            var body = character.GetComponent<Rigidbody2D>();
            if (body != null)
            {
                body.simulated = enabled;
            }
        }

        private static void SetCharacterVisualAlpha(Transform character, float alpha)
        {
            var spriteRenderer = character.GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                return;
            }

            var color = spriteRenderer.color;
            color.a = alpha;
            spriteRenderer.color = color;
        }
    }
}
