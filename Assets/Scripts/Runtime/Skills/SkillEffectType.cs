namespace ParkourShooter.Runtime.Skills
{
    /// <summary>
    /// スキル発動時に適用する効果の種類を表します。
    /// </summary>
    public enum SkillEffectType
    {
        /// <summary>一定時間、攻撃性能を強化します。</summary>
        AttackBoost,

        /// <summary>画面上のボス弾を消去します。</summary>
        BulletClear,

        /// <summary>攻撃強化とボス弾消去を同時に行います。</summary>
        AttackBoostAndBulletClear
    }
}
