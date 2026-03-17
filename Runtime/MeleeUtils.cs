using UnityEngine;

public static class MeleeUtils
{
    public static bool TryRaycast(Vector3 origin, Vector3 dir, float range, LayerMask layers, out RaycastHit hit)
    {
        return Physics.Raycast(origin, dir, out hit, range, layers);
    }

    public static bool TrySphereCast(Vector3 origin, Vector3 dir, float radius, float range, LayerMask layers, out RaycastHit hit)
    {
        return Physics.SphereCast(origin, radius, dir, out hit, range, layers);
    }

    public static Collider[] OverlapSphere(Vector3 origin, Vector3 dir, float range, float radius, LayerMask layers)
    {
        Vector3 center = origin + dir * range;
        return Physics.OverlapSphere(center, radius, layers);
    }

    public static (Vector3 point, Vector3 normal) ContactFromHit(RaycastHit hit)
    {
        return (hit.point, hit.normal);
    }

    public static (Vector3 point, Vector3 normal) ContactFromCollider(Collider col, Vector3 sampleOrigin)
    {
        Vector3 point = col.ClosestPoint(sampleOrigin);
        Vector3 normal = (sampleOrigin - point).normalized;
        return (point, normal);
    }

    public static float ScaleDamageByCharge(float baseDamage, float chargeNormalized, float maxMultiplier)
    {
        return baseDamage * Mathf.Lerp(1f, maxMultiplier, chargeNormalized);
    }

    public static float CooldownNormalized(float nextAttackTime, float cooldown)
    {
        if (cooldown <= 0f) return 0f;
        float remaining = nextAttackTime - Time.time;
        return Mathf.Clamp01(remaining / cooldown);
    }
}
