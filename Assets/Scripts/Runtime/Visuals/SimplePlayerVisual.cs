using ParkourShooter.Runtime.Movement;
using UnityEngine;

namespace ParkourShooter.Runtime.Visuals
{
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class SimplePlayerVisual : MonoBehaviour
    {
        [SerializeField] private ParkourPlayerMotor2D motor;

        private SpriteRenderer spriteRenderer;
        private Color baseColor;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            baseColor = spriteRenderer.color;
        }

        private void LateUpdate()
        {
            if (motor == null)
            {
                return;
            }

            var color = baseColor;
            color.a = spriteRenderer.color.a;
            spriteRenderer.color = color;
        }
    }
}
