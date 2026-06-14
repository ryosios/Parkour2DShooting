namespace ParkourShooter.Runtime.Characters
{
    /// <summary>
    /// プレイヤーキャラクターの現在のアクション状態を表します。
    /// </summary>
    public enum CharacterState
    {
        /// <summary>地上をオートランしている状態です。</summary>
        GroundRun,

        /// <summary>壁グレイズ領域に入っている状態です。</summary>
        WallRun,

        /// <summary>天井グレイズ領域に入っている状態です。</summary>
        CeilingRun,

        /// <summary>ジャンプまたは落下している状態です。</summary>
        Jump,

        /// <summary>ダメージを受けている状態です。</summary>
        Damage,

        /// <summary>キャラクター切り替えで入場している状態です。</summary>
        SwitchIn,

        /// <summary>キャラクター切り替えで退場している状態です。</summary>
        SwitchOut,

        /// <summary>スキルを発動している状態です。</summary>
        Skill,

        /// <summary>戦闘不能の状態です。</summary>
        Dead
    }
}
