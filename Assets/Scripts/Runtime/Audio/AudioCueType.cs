namespace ParkourShooter.Runtime.Audio
{
    /// <summary>
    /// AudioManager が再生する効果音の種類を表します。
    /// </summary>
    public enum AudioCueType
    {
        /// <summary>プレイヤー弾を発射した時の効果音です。</summary>
        PlayerShot,

        /// <summary>攻撃が対象に命中した時の効果音です。</summary>
        Hit,

        /// <summary>スキルを発動した時の効果音です。</summary>
        Skill,

        /// <summary>装備カードを獲得した時の効果音です。</summary>
        CardAcquired,

        /// <summary>操作キャラクターを切り替えた時の効果音です。</summary>
        CharacterSwitch,

        /// <summary>ボス弾を発射した時の効果音です。</summary>
        BossShot
    }
}
