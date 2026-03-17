using UnityEngine;

[System.Serializable]
public class MeleeTraits
{
    [SerializeField] private MeleeDetectionMode _detectionMode = MeleeDetectionMode.Raycast;
    [SerializeField] private LayerMask _hitLayers;
    [SerializeField] private float _attackRange = 2f;
    [SerializeField] private float _attackRadius = 0.5f;
    [SerializeField] private int _maxTargets = 1;
    [SerializeField] private float _baseDamage = 10f;
    [SerializeField] private float _knockback = 5f;
    [SerializeField] private float _attackCooldown = 0.5f;
    [SerializeField] private float _hitWindowStart = 0.1f;
    [SerializeField] private float _hitWindowEnd = 0.4f;
    [SerializeField] private string _attackTrigger = "Attack";
    [SerializeField] private string _impactTrigger = "Impact";

    public MeleeDetectionMode DetectionMode => _detectionMode;
    public LayerMask HitLayers => _hitLayers;
    public float AttackRange => _attackRange;
    public float AttackRadius => _attackRadius;
    public int MaxTargets => _maxTargets;
    public float BaseDamage => _baseDamage;
    public float Knockback => _knockback;
    public float AttackCooldown => _attackCooldown;
    public float HitWindowStart => _hitWindowStart;
    public float HitWindowEnd => _hitWindowEnd;
    public string AttackTrigger => _attackTrigger;
    public string ImpactTrigger => _impactTrigger;
}
