using System;
using ParkourShooter.Runtime.Audio;
using ParkourShooter.Runtime.Combat;
using UnityEngine;

namespace ParkourShooter.Runtime.Bosses
{
    /// <summary>
    /// ボス本体の追従移動、ランダム上下移動、射撃、HP 管理を担当します。
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public sealed class Boss2D : MonoBehaviour, Damageable2D
    {
        /// <summary>ボスが撃破された時に通知されるイベントです。</summary>
        public static event Action<Boss2D> Defeated;

        /// <summary>HP が変化した時に現在 HP と最大 HP を通知するイベントです。</summary>
        public event Action<int, int> HealthChanged;

        [Header("Target")]
        /// <summary>X 方向で追従する対象キャラクターです。</summary>
        [SerializeField] private Transform followTarget;

        /// <summary>追従対象から見たボスの基準位置オフセットです。Y は初期配置のみに使います。</summary>
        [SerializeField] private Vector2 followOffset = new(10f, 1.5f);

        /// <summary>X 方向の追従を滑らかにする時間です。</summary>
        [SerializeField] private float xFollowSmoothTime = 0.18f;

        /// <summary>X 方向の差がこの値以下なら移動を止め、微小な追従揺れを抑えます。</summary>
        [SerializeField] private float xFollowDeadZone = 0.04f;

        [Header("Screen Bounds")]
        /// <summary>ボスの移動範囲を制限する描画カメラです。未設定時は MainCamera を使用します。</summary>
        [SerializeField] private Camera viewportCamera;

        /// <summary>画面左右に確保するビューポート座標単位の余白です。</summary>
        [SerializeField, Range(0f, 0.25f)] private float horizontalViewportPadding = 0.03f;

        /// <summary>画面上下に確保するビューポート座標単位の余白です。</summary>
        [SerializeField, Range(0f, 0.25f)] private float verticalViewportPadding = 0.05f;

        [Header("Vertical Patrol")]
        /// <summary>ボスがランダム移動する Y 座標の範囲です。</summary>
        [SerializeField] private Vector2 verticalRange = new(-1.2f, 2.4f);

        /// <summary>ボスがランダム移動する画面内 Y 座標の範囲です。</summary>
        [SerializeField] private Vector2 verticalViewportRange = new(0.28f, 0.72f);

        /// <summary>Y 方向の移動速度です。</summary>
        [SerializeField] private float verticalMoveSpeed = 2.4f;

        /// <summary>目的地に到着してから次の移動を始めるまでの待機秒数範囲です。</summary>
        [SerializeField] private Vector2 verticalWaitSeconds = new(0.8f, 1.8f);

        [Header("Health")]
        /// <summary>ボスの最大 HP です。</summary>
        [SerializeField] private int maxHp = 30;

        [Header("Pattern")]
        /// <summary>ボス弾を生成する発射位置です。</summary>
        [SerializeField] private Transform muzzle;

        /// <summary>1 秒あたりのボス弾発射回数です。</summary>
        [SerializeField] private float fireRate = 1.2f;

        /// <summary>ボス弾が与えるダメージです。</summary>
        [SerializeField] private int bulletDamage = 1;

        /// <summary>ボス弾の移動速度です。</summary>
        [SerializeField] private float bulletSpeed = 8f;

        /// <summary>ボス弾が自動消滅するまでの秒数です。</summary>
        [SerializeField] private float bulletLifetime = 5f;

        [Header("Reward")]
        /// <summary>ボス撃破時に加算されるスコアです。</summary>
        [SerializeField] private int scoreValue = 1000;

        /// <summary>現在の HP です。</summary>
        private int currentHp;

        /// <summary>次に射撃できるゲーム内時刻です。</summary>
        private float nextFireTime;

        /// <summary>X 追従 SmoothDamp 用の内部速度です。</summary>
        private float xFollowVelocity;

        /// <summary>現在目指しているランダム Y 座標です。</summary>
        private float verticalTargetY;

        /// <summary>現在目指している画面内の Y 座標です。</summary>
        private float verticalTargetViewportY;

        /// <summary>次の Y 移動を開始できるゲーム内時刻です。</summary>
        private float nextVerticalMoveTime;

        /// <summary>現在 Y 方向の目的地へ移動中かどうかです。</summary>
        private bool isMovingVertically;

        /// <summary>ボス本体の移動に使用する Rigidbody2D です。</summary>
        private Rigidbody2D body;

        /// <summary>画面内制限で見た目の大きさを取得する Renderer です。</summary>
        private Renderer bossRenderer;

        /// <summary>ボスが生存しているかどうかです。</summary>
        public bool IsAlive => currentHp > 0;

        /// <summary>撃破時に加算されるスコアです。</summary>
        public int ScoreValue => scoreValue;

        /// <summary>現在の HP です。</summary>
        public int CurrentHp => currentHp;

        /// <summary>最大 HP です。</summary>
        public int MaxHp => maxHp;

        /// <summary>
        /// X 方向で追従する対象を設定します。
        /// </summary>
        /// <param name="target">新しい追従対象です。</param>
        public void SetFollowTarget(Transform target)
        {
            followTarget = target;
            xFollowVelocity = 0f;
        }

        /// <summary>
        /// HP、Rigidbody2D、当たり判定設定を初期化します。
        /// </summary>
        private void Awake()
        {
            currentHp = maxHp;

            body = GetComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Kinematic;
            body.gravityScale = 0f;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
            bossRenderer = GetComponentInChildren<Renderer>();

            var bossCollider = GetComponent<Collider2D>();
            bossCollider.isTrigger = true;
        }

        /// <summary>
        /// Y 方向ランダム移動の初期待機時間と HP 表示を初期化します。
        /// </summary>
        private void Start()
        {
            verticalTargetY = transform.position.y;
            verticalTargetViewportY = ResolveViewportCamera()
                ? viewportCamera.WorldToViewportPoint(transform.position).y
                : 0.5f;
            nextVerticalMoveTime = Time.time + UnityEngine.Random.Range(
                verticalWaitSeconds.x,
                verticalWaitSeconds.y);
            isMovingVertically = false;
            HealthChanged?.Invoke(currentHp, maxHp);
        }

        /// <summary>
        /// X は対象へ追従し、Y はランダム目的地へ移動します。
        /// </summary>
        private void FixedUpdate()
        {
            if (followTarget == null || !IsAlive)
            {
                return;
            }

            UpdateVerticalTarget();

            var currentPosition = body.position;
            var targetX = followTarget.position.x + followOffset.x;
            var xDifference = targetX - currentPosition.x;
            var nextX = currentPosition.x;
            if (Mathf.Abs(xDifference) > xFollowDeadZone)
            {
                nextX = Mathf.SmoothDamp(
                    currentPosition.x,
                    targetX,
                    ref xFollowVelocity,
                    xFollowSmoothTime,
                    Mathf.Infinity,
                    Time.fixedDeltaTime);
            }
            else
            {
                xFollowVelocity = 0f;
            }

            var nextY = Mathf.MoveTowards(
                currentPosition.y,
                GetVerticalTargetWorldY(currentPosition),
                verticalMoveSpeed * Time.fixedDeltaTime);

            body.MovePosition(ClampToViewport(new Vector2(nextX, nextY)));
        }

        /// <summary>
        /// Perspective 投影後のボス全体が余白込みでカメラ内に収まるよう、移動先を補正します。
        /// </summary>
        /// <param name="desiredPosition">補正前の移動先ワールド座標です。</param>
        /// <returns>カメラ内に制限された移動先ワールド座標です。</returns>
        private Vector2 ClampToViewport(Vector2 desiredPosition)
        {
            if (!ResolveViewportCamera())
            {
                return desiredPosition;
            }

            var desiredWorldPosition = new Vector3(
                desiredPosition.x,
                desiredPosition.y,
                transform.position.z);
            var viewportPosition = viewportCamera.WorldToViewportPoint(desiredWorldPosition);
            if (viewportPosition.z <= 0f)
            {
                return desiredPosition;
            }

            var rendererExtents = bossRenderer != null ? bossRenderer.bounds.extents : Vector3.zero;
            var horizontalExtent = Mathf.Abs(
                viewportCamera.WorldToViewportPoint(desiredWorldPosition + Vector3.right * rendererExtents.x).x -
                viewportPosition.x);
            var verticalExtent = Mathf.Abs(
                viewportCamera.WorldToViewportPoint(desiredWorldPosition + Vector3.up * rendererExtents.y).y -
                viewportPosition.y);

            var minimumX = horizontalViewportPadding + horizontalExtent;
            var maximumX = 1f - horizontalViewportPadding - horizontalExtent;
            var minimumY = verticalViewportPadding + verticalExtent;
            var maximumY = 1f - verticalViewportPadding - verticalExtent;

            viewportPosition.x = minimumX <= maximumX
                ? Mathf.Clamp(viewportPosition.x, minimumX, maximumX)
                : 0.5f;
            viewportPosition.y = minimumY <= maximumY
                ? Mathf.Clamp(viewportPosition.y, minimumY, maximumY)
                : 0.5f;

            var clampedWorldPosition = viewportCamera.ViewportToWorldPoint(viewportPosition);
            return new Vector2(clampedWorldPosition.x, clampedWorldPosition.y);
        }

        /// <summary>MainCamera を補完し、画面座標を使用できるか判定します。</summary>
        /// <returns>使用可能なカメラがある場合は true です。</returns>
        private bool ResolveViewportCamera()
        {
            if (viewportCamera == null)
            {
                viewportCamera = Camera.main;
            }

            return viewportCamera != null;
        }

        /// <summary>
        /// 現在の画面内目標 Y を、ボスと同じ奥行きのワールド Y 座標へ変換します。
        /// </summary>
        /// <param name="currentPosition">現在のボス座標です。</param>
        /// <returns>移動先のワールド Y 座標です。</returns>
        private float GetVerticalTargetWorldY(Vector2 currentPosition)
        {
            if (!ResolveViewportCamera())
            {
                return verticalTargetY;
            }

            var viewportPosition = viewportCamera.WorldToViewportPoint(new Vector3(
                currentPosition.x,
                currentPosition.y,
                transform.position.z));
            viewportPosition.y = verticalTargetViewportY;
            return viewportCamera.ViewportToWorldPoint(viewportPosition).y;
        }

        /// <summary>
        /// 発射間隔に応じてボス弾を生成します。
        /// </summary>
        private void Update()
        {
            if (!IsAlive || Time.time < nextFireTime)
            {
                return;
            }

            Fire();
            nextFireTime = Time.time + 1f / Mathf.Max(0.01f, fireRate);
        }

        /// <summary>
        /// ボスへダメージを適用し、HP が 0 になったら撃破します。
        /// </summary>
        /// <param name="damage">適用するダメージ量です。</param>
        public void ApplyDamage(int damage)
        {
            if (!IsAlive)
            {
                return;
            }

            currentHp = Mathf.Max(0, currentHp - Mathf.Max(0, damage));
            HealthChanged?.Invoke(currentHp, maxHp);
            if (currentHp == 0)
            {
                Defeat();
            }
        }

        /// <summary>
        /// 左方向へ進むボス弾を生成します。
        /// </summary>
        private void Fire()
        {
            var spawnPosition = muzzle != null ? muzzle.position : transform.position + Vector3.left * 1.1f;
            var bulletObject = new GameObject("BossProjectile");
            bulletObject.transform.position = spawnPosition;
            bulletObject.transform.localScale = new Vector3(0.4f, 0.22f, 1f);

            var body = bulletObject.AddComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            body.constraints = RigidbodyConstraints2D.FreezeRotation;

            var collider = bulletObject.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;

            var renderer = bulletObject.AddComponent<SpriteRenderer>();
            renderer.sprite = GeneratedCombatSprite.Get();
            renderer.color = new Color(1f, 0.2f, 0.35f);
            renderer.sortingOrder = 20;

            var projectile = bulletObject.AddComponent<BossProjectile2D>();
            projectile.Configure(bulletSpeed, bulletLifetime, bulletDamage);
            AudioManager.Instance?.PlaySe(AudioCueType.BossShot);
        }

        /// <summary>
        /// Y 方向のランダム目的地と到着後の待機状態を更新します。
        /// </summary>
        private void UpdateVerticalTarget()
        {
            var hasArrived = ResolveViewportCamera()
                ? Mathf.Abs(
                    viewportCamera.WorldToViewportPoint(new Vector3(
                        body.position.x,
                        body.position.y,
                        transform.position.z)).y - verticalTargetViewportY) <= 0.01f
                : Mathf.Abs(body.position.y - verticalTargetY) <= 0.02f;
            if (isMovingVertically)
            {
                if (hasArrived)
                {
                    isMovingVertically = false;
                    nextVerticalMoveTime = Time.time + UnityEngine.Random.Range(
                        verticalWaitSeconds.x,
                        verticalWaitSeconds.y);
                }

                return;
            }

            if (Time.time < nextVerticalMoveTime)
            {
                return;
            }

            verticalTargetY = UnityEngine.Random.Range(verticalRange.x, verticalRange.y);
            verticalTargetViewportY = UnityEngine.Random.Range(
                Mathf.Min(verticalViewportRange.x, verticalViewportRange.y),
                Mathf.Max(verticalViewportRange.x, verticalViewportRange.y));
            isMovingVertically = true;
        }

        /// <summary>
        /// 撃破イベントを通知してボスを破棄します。
        /// </summary>
        private void Defeat()
        {
            Defeated?.Invoke(this);
            Destroy(gameObject);
        }
    }
}
