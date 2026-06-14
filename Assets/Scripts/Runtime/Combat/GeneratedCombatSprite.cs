using UnityEngine;

namespace ParkourShooter.Runtime.Combat
{
    public static class GeneratedCombatSprite
    {
        private static Sprite sprite;

        public static Sprite Get()
        {
            if (sprite != null)
            {
                return sprite;
            }

            var texture = new Texture2D(1, 1, TextureFormat.RGBA32, false)
            {
                name = "GeneratedCombatSpriteTexture",
                filterMode = FilterMode.Point
            };
            texture.SetPixel(0, 0, Color.white);
            texture.Apply();

            sprite = Sprite.Create(texture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);
            sprite.name = "GeneratedCombatSprite";
            return sprite;
        }
    }
}
