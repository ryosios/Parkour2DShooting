using UnityEngine;

namespace ParkourShooter.Runtime.Audio
{
    /// <summary>
    /// BGM、効果音、ボイスを一元的に再生するシーン内の音声管理クラスです。
    /// </summary>
    public sealed class AudioManager : MonoBehaviour
    {
        /// <summary>
        /// シーン内で共有する AudioManager のインスタンスです。
        /// </summary>
        public static AudioManager Instance { get; private set; }

        [Header("Sources")]
        /// <summary>BGM 再生に使用する AudioSource です。</summary>
        [SerializeField] private AudioSource bgmSource;

        /// <summary>効果音再生に使用する AudioSource です。</summary>
        [SerializeField] private AudioSource seSource;

        /// <summary>ボイス再生に使用する AudioSource です。</summary>
        [SerializeField] private AudioSource voiceSource;

        [Header("BGM")]
        /// <summary>開始時に再生する既定の BGM です。</summary>
        [SerializeField] private AudioClip bgmClip;

        /// <summary>シーン開始時に既定 BGM を再生するかどうかです。</summary>
        [SerializeField] private bool playBgmOnStart = true;

        [Header("SE")]
        /// <summary>プレイヤー射撃時の効果音です。</summary>
        [SerializeField] private AudioClip playerShotClip;

        /// <summary>攻撃命中時の効果音です。</summary>
        [SerializeField] private AudioClip hitClip;

        /// <summary>スキル発動時の効果音です。</summary>
        [SerializeField] private AudioClip skillClip;

        /// <summary>カード獲得時の効果音です。</summary>
        [SerializeField] private AudioClip cardAcquiredClip;

        /// <summary>キャラクター切り替え時の効果音です。</summary>
        [SerializeField] private AudioClip characterSwitchClip;

        /// <summary>ボス射撃時の効果音です。</summary>
        [SerializeField] private AudioClip bossShotClip;

        [Header("Voice")]
        /// <summary>明示的なボイス指定がない時に再生する既定ボイスです。</summary>
        [SerializeField] private AudioClip defaultVoiceClip;

        /// <summary>
        /// シングルトンを初期化し、必要な AudioSource を補完します。
        /// </summary>
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            EnsureSources();
        }

        /// <summary>
        /// 設定に応じて開始時 BGM を再生します。
        /// </summary>
        private void Start()
        {
            if (playBgmOnStart)
            {
                PlayBgm();
            }
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
        /// 指定された BGM、または既定 BGM をループ再生します。
        /// </summary>
        /// <param name="clip">任意で差し替える BGM クリップです。</param>
        public void PlayBgm(AudioClip clip = null)
        {
            var selectedClip = clip != null ? clip : bgmClip;
            if (selectedClip == null || bgmSource == null)
            {
                return;
            }

            bgmSource.clip = selectedClip;
            bgmSource.loop = true;
            bgmSource.Play();
        }

        /// <summary>
        /// 現在再生中の BGM を停止します。
        /// </summary>
        public void StopBgm()
        {
            if (bgmSource != null)
            {
                bgmSource.Stop();
            }
        }

        /// <summary>
        /// 指定された種類の効果音を一回再生します。
        /// </summary>
        /// <param name="cueType">再生したい効果音の種類です。</param>
        public void PlaySe(AudioCueType cueType)
        {
            var clip = GetSeClip(cueType);
            if (clip == null || seSource == null)
            {
                return;
            }

            seSource.PlayOneShot(clip);
        }

        /// <summary>
        /// 指定されたボイス、または既定ボイスを一回再生します。
        /// </summary>
        /// <param name="clip">任意で差し替えるボイスクリップです。</param>
        public void PlayVoice(AudioClip clip = null)
        {
            var selectedClip = clip != null ? clip : defaultVoiceClip;
            if (selectedClip == null || voiceSource == null)
            {
                return;
            }

            voiceSource.PlayOneShot(selectedClip);
        }

        /// <summary>
        /// 効果音の種類に対応する AudioClip を取得します。
        /// </summary>
        /// <param name="cueType">検索する効果音の種類です。</param>
        /// <returns>対応する AudioClip です。未設定の場合は null です。</returns>
        private AudioClip GetSeClip(AudioCueType cueType)
        {
            return cueType switch
            {
                AudioCueType.PlayerShot => playerShotClip,
                AudioCueType.Hit => hitClip,
                AudioCueType.Skill => skillClip,
                AudioCueType.CardAcquired => cardAcquiredClip,
                AudioCueType.CharacterSwitch => characterSwitchClip,
                AudioCueType.BossShot => bossShotClip,
                _ => null
            };
        }

        /// <summary>
        /// 未設定の AudioSource を子オブジェクトとして生成します。
        /// </summary>
        private void EnsureSources()
        {
            bgmSource ??= CreateSource("BgmSource", true);
            seSource ??= CreateSource("SeSource", false);
            voiceSource ??= CreateSource("VoiceSource", false);
        }

        /// <summary>
        /// 2D 再生用の AudioSource を生成します。
        /// </summary>
        /// <param name="sourceName">生成する GameObject 名です。</param>
        /// <param name="loop">ループ再生を有効にするかどうかです。</param>
        /// <returns>生成された AudioSource です。</returns>
        private AudioSource CreateSource(string sourceName, bool loop)
        {
            var sourceObject = new GameObject(sourceName);
            sourceObject.transform.SetParent(transform);
            sourceObject.transform.localPosition = Vector3.zero;

            var source = sourceObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = loop;
            source.spatialBlend = 0f;
            return source;
        }
    }
}
