using System.Collections.Generic;
using ParkourShooter.Runtime.Characters;
using ParkourShooter.Runtime.Score;
using UnityEngine;

namespace ParkourShooter.Runtime.Movement
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public sealed class ParkourPlayerMotor2D : MonoBehaviour
    {
        [Header("Auto Run")]
        [SerializeField] private float autoRunSpeed = 7f;

        [Header("Boost Jump")]
        [SerializeField] private float boostForwardImpulse = 4f;
        [SerializeField] private float upperBoostImpulse = 11f;
        [SerializeField] private float lowerBoostImpulse = 9f;

        [Header("Graze Run")]
        [SerializeField] private float grazeScorePerSecond = 20f;
        [SerializeField] private ScoreManager scoreManager;

        private Rigidbody2D body;
        private float defaultGravityScale;
        private readonly List<GrazeArea2D> activeGrazeAreas = new();
        private float moveSpeedBonus;
        private float grazeScoreMultiplierBonus;
        private float grazeScoreRemainder;

        public CharacterState State { get; private set; } = CharacterState.GroundRun;
        public bool IsInGrazeArea => activeGrazeAreas.Count > 0;
        public float GrazeScoreMultiplier => 1f + grazeScoreMultiplierBonus;

        public void SetCardModifier(float newMoveSpeedBonus, float newGrazeScoreMultiplierBonus)
        {
            moveSpeedBonus = Mathf.Max(0f, newMoveSpeedBonus);
            grazeScoreMultiplierBonus = Mathf.Max(0f, newGrazeScoreMultiplierBonus);
        }

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

        private void FixedUpdate()
        {
            if (IsInGrazeArea)
            {
                ApplyGrazeFloat();
                return;
            }

            body.gravityScale = defaultGravityScale;
            var velocity = body.linearVelocity;
            velocity.x = autoRunSpeed + moveSpeedBonus;
            body.linearVelocity = velocity;

            if (State != CharacterState.Jump)
            {
                State = CharacterState.GroundRun;
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.TryGetComponent(out GrazeArea2D grazeArea))
            {
                return;
            }

            if (!activeGrazeAreas.Contains(grazeArea))
            {
                activeGrazeAreas.Add(grazeArea);
            }

            body.gravityScale = 0f;
            StopVerticalMotion();
            State = grazeArea.GrazeType == GrazeType.Wall ? CharacterState.WallRun : CharacterState.CeilingRun;
        }

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

        private void Boost(Vector2 direction, float verticalImpulse)
        {
            if (IsInGrazeArea)
            {
                activeGrazeAreas.Clear();
                body.gravityScale = defaultGravityScale;
            }

            State = CharacterState.Jump;

            StopVerticalMotion();
            body.AddForce(new Vector2(boostForwardImpulse, direction.y * verticalImpulse), ForceMode2D.Impulse);
        }

        private void ApplyGrazeFloat()
        {
            body.gravityScale = 0f;
            var grazeArea = GetNearestGrazeArea();
            var velocity = body.linearVelocity;
            velocity.x = autoRunSpeed + moveSpeedBonus;
            velocity.y = 0f;

            if (grazeArea != null)
            {
                State = grazeArea.GrazeType == GrazeType.Wall ? CharacterState.WallRun : CharacterState.CeilingRun;
            }

            body.linearVelocity = velocity;
            AddGrazeScore();
        }

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

        private void StopVerticalMotion()
        {
            var velocity = body.linearVelocity;
            velocity.y = 0f;
            body.linearVelocity = velocity;
        }

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
