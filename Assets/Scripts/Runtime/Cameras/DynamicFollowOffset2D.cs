using Unity.Cinemachine;
using UnityEngine;

namespace ParkourShooter.Runtime.Cameras
{
    /// <summary>
    /// キャラクターの高さに応じて CinemachineFollow のオフセットを滑らかに切り替えます。
    /// </summary>
    [RequireComponent(typeof(CinemachineCamera))]
    [RequireComponent(typeof(CinemachineFollow))]
    public sealed class DynamicFollowOffset2D : MonoBehaviour
    {
        /// <summary>開始時や低い位置にいる時のカメラオフセットです。</summary>
        [SerializeField] private Vector3 lowerScreenOffset = new(8.35f, 4.32f, -17f);

        /// <summary>ジャンプ後にキャラクターを中央寄せするカメラオフセットです。</summary>
        [SerializeField] private Vector3 upperScreenOffset = new(8.35f, 0f, -17f);

        /// <summary>基準 Y からこの高さ以上に上がると中央寄せへ切り替えます。</summary>
        [SerializeField] private float riseToUpperOffset = 2.2f;

        /// <summary>中央寄せ後、この高さ以下まで戻ると下寄せへ切り替えます。</summary>
        [SerializeField] private float fallToLowerOffset = 1.1f;

        /// <summary>FollowOffset を切り替える時の補間時間です。</summary>
        [SerializeField] private float smoothTime = 0.22f;

        /// <summary>制御対象の CinemachineCamera です。</summary>
        private CinemachineCamera cinemachineCamera;

        /// <summary>実際に FollowOffset を保持する CinemachineFollow です。</summary>
        private CinemachineFollow cinemachineFollow;

        /// <summary>SmoothDamp 用の内部速度です。</summary>
        private Vector3 offsetVelocity;

        /// <summary>現在追従しているターゲットです。</summary>
        private Transform currentTarget;

        /// <summary>現在のターゲットに切り替わった時点の Y 座標です。</summary>
        private float baselineTargetY;

        /// <summary>下寄せオフセットを使用中かどうかです。</summary>
        private bool useLowerScreenOffset = true;

        /// <summary>
        /// 必要な Cinemachine コンポーネントを取得し、初期オフセットへ合わせます。
        /// </summary>
        private void Awake()
        {
            cinemachineCamera = GetComponent<CinemachineCamera>();
            cinemachineFollow = GetComponent<CinemachineFollow>();

            SnapToInitialOffset();
        }

        /// <summary>
        /// ターゲットの高さに応じて FollowOffset を滑らかに更新します。
        /// </summary>
        private void LateUpdate()
        {
            var target = cinemachineCamera != null ? cinemachineCamera.Follow : null;
            if (target == null || cinemachineFollow == null)
            {
                return;
            }

            EnsureTargetBaseline(target);
            UpdateOffsetMode(target);
            var targetOffset = useLowerScreenOffset ? lowerScreenOffset : upperScreenOffset;
            cinemachineFollow.FollowOffset = Vector3.SmoothDamp(
                cinemachineFollow.FollowOffset,
                targetOffset,
                ref offsetVelocity,
                smoothTime);
        }

        /// <summary>
        /// 外部からカメラオフセット設定を初期化します。
        /// </summary>
        /// <param name="lowerOffset">低い位置にいる時のオフセットです。</param>
        /// <param name="upperOffset">上昇後のオフセットです。</param>
        /// <param name="newSmoothTime">オフセット補間時間です。</param>
        public void Configure(Vector3 lowerOffset, Vector3 upperOffset, float newSmoothTime)
        {
            lowerScreenOffset = lowerOffset;
            upperScreenOffset = upperOffset;
            smoothTime = Mathf.Max(0.01f, newSmoothTime);
            currentTarget = null;
            useLowerScreenOffset = true;
            SnapToInitialOffset();
        }

        /// <summary>
        /// 現在の FollowOffset を下寄せ初期値へ即時反映します。
        /// </summary>
        private void SnapToInitialOffset()
        {
            if (cinemachineFollow == null)
            {
                cinemachineFollow = GetComponent<CinemachineFollow>();
            }

            if (cinemachineFollow == null)
            {
                return;
            }

            cinemachineFollow.FollowOffset = lowerScreenOffset;
            offsetVelocity = Vector3.zero;
        }

        /// <summary>
        /// 追従ターゲットが変わった時に高さ判定の基準値を更新します。
        /// </summary>
        /// <param name="target">現在追従しているターゲットです。</param>
        private void EnsureTargetBaseline(Transform target)
        {
            if (currentTarget == target)
            {
                return;
            }

            currentTarget = target;
            baselineTargetY = target.position.y;
            useLowerScreenOffset = true;
            SnapToInitialOffset();
        }

        /// <summary>
        /// 基準 Y からの上昇量に応じて使用するオフセット種別を更新します。
        /// </summary>
        /// <param name="target">高さを判定するターゲットです。</param>
        private void UpdateOffsetMode(Transform target)
        {
            var riseFromBaseline = target.position.y - baselineTargetY;
            if (useLowerScreenOffset)
            {
                useLowerScreenOffset = riseFromBaseline < riseToUpperOffset;
            }
            else
            {
                useLowerScreenOffset = riseFromBaseline < fallToLowerOffset;
            }
        }
    }
}
