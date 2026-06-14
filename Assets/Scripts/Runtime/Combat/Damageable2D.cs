namespace ParkourShooter.Runtime.Combat
{
    /// <summary>
    /// 弾や攻撃からダメージを受けられる 2D オブジェクトの共通インターフェースです。
    /// </summary>
    public interface Damageable2D
    {
        /// <summary>
        /// ダメージを受けられる生存状態かどうかを返します。
        /// </summary>
        bool IsAlive { get; }

        /// <summary>
        /// 指定されたダメージ量を現在の耐久値へ適用します。
        /// </summary>
        /// <param name="damage">適用するダメージ量です。</param>
        void ApplyDamage(int damage);
    }
}
