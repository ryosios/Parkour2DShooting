using UnityEngine;

namespace ParkourShooter.Runtime.Combat
{
    /// <summary>
    /// 弾や簡易 VFX に使う白 1px スプライトをランタイム生成して共有します。
    /// </summary>
    public static class GeneratedCombatSprite
    {
        /// <summary>生成済みの共有スプライトです。</summary>
        private static Sprite sprite;

        /// <summary>
        /// 共有スプライトを取得します。未生成ならその場で作成します。
        /// </summary>
        /// <returns>白 1px の Sprite です。</returns>
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
