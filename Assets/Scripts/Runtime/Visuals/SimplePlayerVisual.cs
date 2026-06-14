using ParkourShooter.Runtime.Movement;
using UnityEngine;

namespace ParkourShooter.Runtime.Visuals
{
    /// <summary>
    /// プレイヤーの元色を維持しつつ、切り替え演出の透明度だけを反映します。
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class SimplePlayerVisual : MonoBehaviour
    {
        /// <summary>状態参照用のプレイヤーモーターです。</summary>
        [SerializeField] private ParkourPlayerMotor2D motor;

        /// <summary>色を制御する SpriteRenderer です。未設定時は子も含めて検索します。</summary>
        [SerializeField] private SpriteRenderer spriteRenderer;

        /// <summary>色を制御する SpriteRenderer です。</summary>
        /// <summary>開始時の RGB 色です。</summary>
        private Color baseColor;

        /// <summary>
        /// SpriteRenderer と基準色を取得します。
        /// </summary>
        private void Awake()
        {
            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            }

            if (spriteRenderer == null)
            {
                return;
            }

            baseColor = spriteRenderer.color;
        }

        /// <summary>
        /// 毎フレーム、RGB は基準色へ戻し、現在のアルファだけ維持します。
        /// </summary>
        private void LateUpdate()
        {
            if (motor == null || spriteRenderer == null)
            {
                return;
            }

            var color = baseColor;
            color.a = spriteRenderer.color.a;
            spriteRenderer.color = color;
        }
    }
}
