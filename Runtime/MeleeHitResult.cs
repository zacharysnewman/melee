using UnityEngine;

public readonly struct MeleeHitResult
{
    public readonly Collider Target;
    public readonly Vector3 WorldPoint;
    public readonly Vector3 WorldNormal;
    public readonly float DamageDealt;
    public readonly float KnockbackApplied;
    public readonly bool WasKillingBlow;

    public MeleeHitResult(Collider target, Vector3 worldPoint, Vector3 worldNormal,
        float damageDealt, float knockbackApplied, bool wasKillingBlow)
    {
        Target = target;
        WorldPoint = worldPoint;
        WorldNormal = worldNormal;
        DamageDealt = damageDealt;
        KnockbackApplied = knockbackApplied;
        WasKillingBlow = wasKillingBlow;
    }
}
