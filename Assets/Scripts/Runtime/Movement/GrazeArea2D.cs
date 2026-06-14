using UnityEngine;

namespace ParkourShooter.Runtime.Movement
{
    /// <summary>
    /// プレイヤーが素通りできるグレイズ領域を表し、領域種別を提供します。
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public sealed class GrazeArea2D : MonoBehaviour
    {
        /// <summary>このグレイズ領域の種類です。</summary>
        [SerializeField] private GrazeType grazeType = GrazeType.Wall;

        /// <summary>将来の吸着補助用に保持している強度値です。</summary>
        [SerializeField] private float attractionStrength = 14f;

        /// <summary>このグレイズ領域の種類です。</summary>
        public GrazeType GrazeType => grazeType;

        /// <summary>吸着補助に使用する強度値です。</summary>
        public float AttractionStrength => attractionStrength;

        /// <summary>領域の中心座標です。</summary>
        public Vector2 Center => transform.position;

        /// <summary>
        /// 追加直後に Collider2D をトリガー化します。
        /// </summary>
        private void Reset()
        {
            var areaCollider = GetComponent<Collider2D>();
            areaCollider.isTrigger = true;
        }

        /// <summary>
        /// Inspector 編集時も物理衝突しないトリガー状態を維持します。
        /// </summary>
        private void OnValidate()
        {
            var areaCollider = GetComponent<Collider2D>();
            if (areaCollider != null)
            {
                areaCollider.isTrigger = true;
            }
        }
    }
}
