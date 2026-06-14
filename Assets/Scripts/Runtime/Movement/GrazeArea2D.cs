using UnityEngine;

namespace ParkourShooter.Runtime.Movement
{
    [RequireComponent(typeof(Collider2D))]
    public sealed class GrazeArea2D : MonoBehaviour
    {
        [SerializeField] private GrazeType grazeType = GrazeType.Wall;
        [SerializeField] private float attractionStrength = 14f;

        public GrazeType GrazeType => grazeType;
        public float AttractionStrength => attractionStrength;
        public Vector2 Center => transform.position;

        private void Reset()
        {
            var areaCollider = GetComponent<Collider2D>();
            areaCollider.isTrigger = true;
        }

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
