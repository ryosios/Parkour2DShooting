using System;
using System.Collections.Generic;
using ParkourShooter.Runtime.Audio;
using ParkourShooter.Runtime.Bosses;
using ParkourShooter.Runtime.Cameras;
using ParkourShooter.Runtime.Combat;
using ParkourShooter.Runtime.Movement;
using ParkourShooter.Runtime.Skills;
using Unity.Cinemachine;
using UnityEngine;

namespace ParkourShooter.Runtime.Characters
{
    /// <summary>
    /// 複数キャラクターの有効化、切り替え、カメラとボスの追従対象更新を管理します。
    /// </summary>
    public sealed class TeamController2D : MonoBehaviour
    {
        /// <summary>アクティブキャラクターが変更された時に通知されるイベントです。</summary>
        public static event Action<Transform> ActiveCharacterChanged;

        /// <summary>切り替え対象となる見た目スロット一覧です。</summary>
        [SerializeField] private List<Transform> characters = new();

        /// <summary>アクティブキャラクターを追従する CinemachineCamera です。</summary>
        [SerializeField] private CinemachineCamera followCamera;

        /// <summary>アクティブキャラクターを追従対象にするボスです。</summary>
        [SerializeField] private Boss2D boss;

        /// <summary>キャラクター切り替え演出にかける秒数です。</summary>
        [SerializeField] private float transitionSeconds = 0.18f;

        /// <summary>現在アクティブなキャラクターのインデックスです。</summary>
        private int activeIndex;

        /// <summary>切り替え演出中かどうかです。</summary>
        private bool isSwitching;

        /// <summary>現在表示中のキャラクタースロットです。</summary>
        public Transform ActiveCharacter => characters.Count == 0 ? null : characters[activeIndex];

        /// <summary>登録されている全キャラクタースロットです。</summary>
        public IReadOnlyList<Transform> Characters => characters;

        /// <summary>実際に移動・攻撃・当たり判定を持つ共有Rootです。</summary>
        public Transform ControlRoot => transform;

        /// <summary>
        /// カメラオフセット制御を準備し、初期キャラクターを有効化します。
        /// </summary>
        private void Start()
        {
            EnsureDynamicCameraOffset();
            SetActiveCharacter(0, true);
        }

        /// <summary>
        /// A/D 入力で前後のキャラクターへ切り替えます。
        /// </summary>
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

        /// <summary>
        /// 指定インデックスへ循環切り替えを開始します。
        /// </summary>
        /// <param name="requestedIndex">切り替え先として要求されたインデックスです。</param>
        private void SwitchTo(int requestedIndex)
        {
            var nextIndex = (requestedIndex + characters.Count) % characters.Count;
            if (nextIndex == activeIndex)
            {
                return;
            }

            StartCoroutine(SwitchRoutine(nextIndex));
        }

        /// <summary>
        /// 一時的に次キャラクターを半透明表示し、切り替え完了後に操作対象を更新します。
        /// </summary>
        /// <param name="nextIndex">切り替え先キャラクターのインデックスです。</param>
        /// <returns>切り替え演出用の IEnumerator です。</returns>
        private System.Collections.IEnumerator SwitchRoutine(int nextIndex)
        {
            isSwitching = true;

            var previous = characters[activeIndex];
            var next = characters[nextIndex];
            next.gameObject.SetActive(true);
            NormalizeSlotTransform(next);
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

        /// <summary>
        /// 指定キャラクタースロットだけを表示し、カメラとボスは共有Rootを追従させます。
        /// </summary>
        /// <param name="index">有効化するキャラクターのインデックスです。</param>
        /// <param name="snapReferences">即時参照更新用の予約引数です。</param>
        private void SetActiveCharacter(int index, bool snapReferences)
        {
            activeIndex = Mathf.Clamp(index, 0, characters.Count - 1);

            for (var i = 0; i < characters.Count; i++)
            {
                var character = characters[i];
                var isActive = i == activeIndex;
                character.gameObject.SetActive(isActive);
                NormalizeSlotTransform(character);
            }

            var active = ActiveCharacter;
            if (active == null)
            {
                return;
            }

            if (followCamera != null)
            {
                followCamera.Follow = ControlRoot;
            }

            if (boss != null)
            {
                boss.SetFollowTarget(ControlRoot);
            }

            ApplyActiveSlotSettings(active);
            ActiveCharacterChanged?.Invoke(active);
        }

        /// <summary>
        /// キャラクターに付与された操作系コンポーネントと Rigidbody2D を有効/無効にします。
        /// </summary>
        /// <param name="character">対象キャラクターです。</param>
        /// <param name="enabled">有効化するかどうかです。</param>
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

        /// <summary>
        /// 切り替え演出用にキャラクターの透明度だけを変更します。
        /// </summary>
        /// <param name="character">対象キャラクターです。</param>
        /// <param name="alpha">設定するアルファ値です。</param>
        private static void SetCharacterVisualAlpha(Transform character, float alpha)
        {
            var spriteRenderer = character.GetComponentInChildren<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                return;
            }

            var color = spriteRenderer.color;
            color.a = alpha;
            spriteRenderer.color = color;
        }

        /// <summary>
        /// 切り替え前後で位置、回転、速度がズレないように状態を同期します。
        /// </summary>
        /// <param name="source">同期元キャラクターです。</param>
        /// <param name="destination">同期先キャラクターです。</param>
        private static void CopyCharacterPose(Transform source, Transform destination)
        {
            destination.SetPositionAndRotation(source.position, source.rotation);

            var sourceBody = source.GetComponent<Rigidbody2D>();
            var destinationBody = destination.GetComponent<Rigidbody2D>();
            if (sourceBody == null || destinationBody == null)
            {
                Physics2D.SyncTransforms();
                return;
            }

            destinationBody.position = sourceBody.position;
            destinationBody.rotation = sourceBody.rotation;
            destinationBody.linearVelocity = sourceBody.linearVelocity;
            destinationBody.angularVelocity = sourceBody.angularVelocity;
            Physics2D.SyncTransforms();
        }

        /// <summary>
        /// 見た目スロットが共有Rootからズレないようにローカル座標を初期化します。
        /// </summary>
        /// <param name="character">対象の見た目スロットです。</param>
        private static void NormalizeSlotTransform(Transform character)
        {
            character.localPosition = Vector3.zero;
            character.localRotation = Quaternion.identity;
        }

        /// <summary>
        /// アクティブな見た目スロットに設定されたキャラクター固有設定を共有Rootへ反映します。
        /// </summary>
        /// <param name="active">現在表示中のキャラクタースロットです。</param>
        private void ApplyActiveSlotSettings(Transform active)
        {
            var slot = active.GetComponent<CharacterSlot2D>();
            var skill = GetComponent<SkillController2D>();
            if (slot != null && skill != null)
            {
                skill.SetEffectType(slot.SkillEffect);
            }
        }

        /// <summary>
        /// 既存シーンでも動的カメラオフセット制御が付くように補完します。
        /// </summary>
        private void EnsureDynamicCameraOffset()
        {
            if (followCamera == null)
            {
                return;
            }

            var dynamicOffset = followCamera.GetComponent<DynamicFollowOffset2D>();
            if (dynamicOffset == null)
            {
                dynamicOffset = followCamera.gameObject.AddComponent<DynamicFollowOffset2D>();
                dynamicOffset.Configure(
                    new Vector3(8.35f, 4.32f, -17f),
                    new Vector3(8.35f, 0f, -17f),
                    0.22f);
            }
        }
    }
}
