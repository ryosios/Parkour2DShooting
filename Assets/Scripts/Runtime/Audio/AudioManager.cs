using UnityEngine;

namespace ParkourShooter.Runtime.Audio
{
    public sealed class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Sources")]
        [SerializeField] private AudioSource bgmSource;
        [SerializeField] private AudioSource seSource;
        [SerializeField] private AudioSource voiceSource;

        [Header("BGM")]
        [SerializeField] private AudioClip bgmClip;
        [SerializeField] private bool playBgmOnStart = true;

        [Header("SE")]
        [SerializeField] private AudioClip playerShotClip;
        [SerializeField] private AudioClip hitClip;
        [SerializeField] private AudioClip skillClip;
        [SerializeField] private AudioClip cardAcquiredClip;
        [SerializeField] private AudioClip characterSwitchClip;
        [SerializeField] private AudioClip bossShotClip;

        [Header("Voice")]
        [SerializeField] private AudioClip defaultVoiceClip;

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

        private void Start()
        {
            if (playBgmOnStart)
            {
                PlayBgm();
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

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

        public void StopBgm()
        {
            if (bgmSource != null)
            {
                bgmSource.Stop();
            }
        }

        public void PlaySe(AudioCueType cueType)
        {
            var clip = GetSeClip(cueType);
            if (clip == null || seSource == null)
            {
                return;
            }

            seSource.PlayOneShot(clip);
        }

        public void PlayVoice(AudioClip clip = null)
        {
            var selectedClip = clip != null ? clip : defaultVoiceClip;
            if (selectedClip == null || voiceSource == null)
            {
                return;
            }

            voiceSource.PlayOneShot(selectedClip);
        }

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

        private void EnsureSources()
        {
            bgmSource ??= CreateSource("BgmSource", true);
            seSource ??= CreateSource("SeSource", false);
            voiceSource ??= CreateSource("VoiceSource", false);
        }

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
