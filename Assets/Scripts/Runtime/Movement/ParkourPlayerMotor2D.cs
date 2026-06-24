using System.Collections.Generic;
using ParkourShooter.Runtime.Characters;
using ParkourShooter.Runtime.Score;
using UnityEngine;

namespace ParkourShooter.Runtime.Movement
{
    /// <summary>
    /// オートラン、上下ブースト、グレイズ浮遊、グレイズスコア加算を制御します。
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public sealed class ParkourPlayerMotor2D : MonoBehaviour
    {
        [Header("Auto Run")]
        /// <summary>カード補正前の基本オートラン速度です。</summary>
        [SerializeField] private float autoRunSpeed = 7f;

        [Header("Boost Jump")]
        /// <summary>上下ブースト時に通常走行速度へ加える前方向速度です。</summary>
        [SerializeField] private float boostForwardImpulse = 4f;

        /// <summary>ブーストで増えた前方向速度が通常速度へ戻る速さです。</summary>
        [SerializeField] private float boostForwardDecay = 8f;

        /// <summary>W 入力で上方向へ加えるインパルスです。</summary>
        [SerializeField] private float upperBoostImpulse = 11f;

        /// <summary>S 入力で下方向へ加えるインパルスです。</summary>
        [SerializeField] private float lowerBoostImpulse = 9f;

        [Header("Graze Run")]
        /// <summary>グレイズ領域内で 1 秒あたりに加算する基本スコアです。</summary>
        [SerializeField] private float grazeScorePerSecond = 20f;

        /// <summary>グレイズスコアを加算する ScoreManager です。</summary>
        [SerializeField] private ScoreManager scoreManager;

        /// <summary>プレイヤー移動に使用する Rigidbody2D です。</summary>
        private Rigidbody2D body;

        /// <summary>通常時の Rigidbody2D 重力倍率です。</summary>
        private float defaultGravityScale;

        /// <summary>現在接触中のグレイズ領域一覧です。</summary>
        private readonly List<GrazeArea2D> activeGrazeAreas = new();

        /// <summary>カードによる移動速度加算です。</summary>
        private float moveSpeedBonus;

        /// <summary>カードによるグレイズスコア倍率加算です。</summary>
        private float grazeScoreMultiplierBonus;

        /// <summary>小数点以下のグレイズスコア繰り越しです。</summary>
        private float grazeScoreRemainder;

        /// <summary>グレイズ領域へ吸着し始めた瞬間のX方向速度です。</summary>
        private float grazeEntryForwardSpeed;

        /// <summary>現在のキャラクター状態です。</summary>
        public CharacterState State { get; private set; } = CharacterState.GroundRun;

        /// <summary>グレイズ領域内にいるかどうかです。</summary>
        public bool IsInGrazeArea => activeGrazeAreas.Count > 0;

        /// <summary>グレイズスコアに掛ける現在の倍率です。</summary>
        public float GrazeScoreMultiplier => 1f + grazeScoreMultiplierBonus;

        /// <summary>
        /// 装備カードによる移動速度とグレイズ倍率の補正を設定します。
        /// </summary>
        /// <param name="newMoveSpeedBonus">移動速度加算値です。</param>
        /// <param name="newGrazeScoreMultiplierBonus">グレイズスコア倍率加算値です。</param>
        public void SetCardModifier(float newMoveSpeedBonus, float newGrazeScoreMultiplierBonus)
        {
            moveSpeedBonus = Mathf.Max(0f, newMoveSpeedBonus);
            grazeScoreMultiplierBonus = Mathf.Max(0f, newGrazeScoreMultiplierBonus);
        }

        /// <summary>
        /// Rigidbody2D と既定重力を初期化し、ScoreManager を補完します。
        /// </summary>
        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            defaultGravityScale = body.gravityScale;
            body.constraints = RigidbodyConstraints2D.FreezeRotation;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;

            if (scoreManager == null)
            {
                scoreManager = FindFirstObjectByType<ScoreManager>();
            }
        }

        /// <summary>
        /// W/S 入力で上下ブーストを発動します。
        /// </summary>
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.W))
            {
                Boost(Vector2.up, upperBoostImpulse);
            }

            if (Input.GetKeyDown(KeyCode.S))
            {
                Boost(Vector2.down, lowerBoostImpulse);
            }
        }

        /// <summary>
        /// 通常時はオートラン、グレイズ中は浮遊移動を適用します。
        /// </summary>
        private void FixedUpdate()
        {
            if (IsInGrazeArea)
            {
                ApplyGrazeFloat();
                return;
            }

            body.gravityScale = defaultGravityScale;
            var velocity = body.linearVelocity;
            var autoRunTargetSpeed = autoRunSpeed + moveSpeedBonus;
            velocity.x = velocity.x < autoRunTargetSpeed
                ? autoRunTargetSpeed
                : Mathf.MoveTowards(
                    velocity.x,
                    autoRunTargetSpeed,
                    Mathf.Max(0f, boostForwardDecay) * Time.fixedDeltaTime);
            body.linearVelocity = velocity;

            if (State != CharacterState.Jump)
            {
                State = CharacterState.GroundRun;
            }
        }

        /// <summary>
        /// グレイズ領域へ入った時に重力と Y 速度を止めます。
        /// </summary>
        /// <param name="other">接触した Collider2D です。</param>
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.TryGetComponent(out GrazeArea2D grazeArea))
            {
                return;
            }

            var wasInGrazeArea = IsInGrazeArea;
            if (!activeGrazeAreas.Contains(grazeArea))
            {
                activeGrazeAreas.Add(grazeArea);
            }

            if (!wasInGrazeArea)
            {
                grazeEntryForwardSpeed = body.linearVelocity.x;
            }

            body.gravityScale = 0f;
            StopVerticalMotion();
            State = grazeArea.GrazeType == GrazeType.Wall ? CharacterState.WallRun : CharacterState.CeilingRun;
        }

        /// <summary>
        /// グレイズ領域から出た時に通常重力へ戻します。
        /// </summary>
        /// <param name="other">離脱した Collider2D です。</param>
        private void OnTriggerExit2D(Collider2D other)
        {
            if (!other.TryGetComponent(out GrazeArea2D grazeArea))
            {
                return;
            }

            activeGrazeAreas.Remove(grazeArea);
            if (!IsInGrazeArea)
            {
                body.gravityScale = defaultGravityScale;
                State = CharacterState.Jump;
            }
        }

        /// <summary>
        /// 指定方向へブーストし、グレイズ中ならグレイズ制御から離脱します。
        /// </summary>
        /// <param name="direction">上方向または下方向の入力方向です。</param>
        /// <param name="verticalImpulse">縦方向へ加えるインパルスです。</param>
        private void Boost(Vector2 direction, float verticalImpulse)
        {
            if (IsInGrazeArea)
            {
                activeGrazeAreas.Clear();
                body.gravityScale = defaultGravityScale;
            }

            State = CharacterState.Jump;
            body.linearVelocity = new Vector2(
                autoRunSpeed + moveSpeedBonus + boostForwardImpulse,
                direction.y * verticalImpulse);
        }

        /// <summary>
        /// グレイズ中の重力無効、Y 速度停止、スコア加算を適用します。
        /// </summary>
        private void ApplyGrazeFloat()
        {
            body.gravityScale = 0f;
            var grazeArea = GetNearestGrazeArea();
            var velocity = body.linearVelocity;
            velocity.x = grazeEntryForwardSpeed;
            velocity.y = 0f;

            if (grazeArea != null)
            {
                State = grazeArea.GrazeType == GrazeType.Wall ? CharacterState.WallRun : CharacterState.CeilingRun;
            }

            body.linearVelocity = velocity;
            AddGrazeScore();
        }

        /// <summary>
        /// 複数のグレイズ領域に入っている場合、最も近い領域を取得します。
        /// </summary>
        /// <returns>最も近い GrazeArea2D です。</returns>
        private GrazeArea2D GetNearestGrazeArea()
        {
            GrazeArea2D nearest = null;
            var nearestDistance = float.MaxValue;
            var position = (Vector2)transform.position;

            for (var i = activeGrazeAreas.Count - 1; i >= 0; i--)
            {
                var grazeArea = activeGrazeAreas[i];
                if (grazeArea == null)
                {
                    activeGrazeAreas.RemoveAt(i);
                    continue;
                }

                var distance = Vector2.SqrMagnitude(grazeArea.Center - position);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearest = grazeArea;
                }
            }

            return nearest;
        }

        /// <summary>
        /// 現在の Y 速度を 0 にします。
        /// </summary>
        private void StopVerticalMotion()
        {
            var velocity = body.linearVelocity;
            velocity.y = 0f;
            body.linearVelocity = velocity;
        }

        /// <summary>
        /// グレイズ滞在時間に応じてスコアを加算します。
        /// </summary>
        private void AddGrazeScore()
        {
            if (scoreManager == null)
            {
                return;
            }

            grazeScoreRemainder += grazeScorePerSecond * GrazeScoreMultiplier * Time.fixedDeltaTime;
            var wholeScore = Mathf.FloorToInt(grazeScoreRemainder);
            if (wholeScore <= 0)
            {
                return;
            }

            grazeScoreRemainder -= wholeScore;
            scoreManager.AddScore(wholeScore);
        }
    }
}
