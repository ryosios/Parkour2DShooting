namespace ParkourShooter.Runtime.Cards
{
    /// <summary>
    /// 装備カードが付与する強化効果の種類を表します。
    /// </summary>
    public enum CardEffectType
    {
        /// <summary>攻撃ダメージを加算します。</summary>
        AttackUp,

        /// <summary>オート移動速度を加算します。</summary>
        MoveSpeedUp,

        /// <summary>同時に発射する弾数を加算します。</summary>
        ProjectileCountUp,

        /// <summary>グレイズ中のスコア倍率を加算します。</summary>
        GrazeBonus
    }
}
