using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeSystem : MonoBehaviour
{
    [SerializeField] private MeleeData _data;
    public MeleeEvents Events = new MeleeEvents();
    [SerializeField] private Animator _animator;
    [SerializeField] private Transform _castOrigin;
    [SerializeField] private MeleeHitbox _weaponHitbox;
    [SerializeField] private AudioClip _swingSound;
    [SerializeField] private AudioClip _hitSound;
    [SerializeField] private GameObject _hitVFXPrefab;

    private float _nextAttackTime;
    private bool _isSwinging;
    private bool _isCharging;
    private float _chargeStartTime;
    private float _damageBonus;
    private float _rangeBonus;
    private HashSet<Collider> _hitThisSwing = new HashSet<Collider>();

    private MeleeTraits Traits => _data.Traits;

    private float _currentCooldownNormalized;
    private float CurrentCooldownNormalized
    {
        get => _currentCooldownNormalized;
        set
        {
            _currentCooldownNormalized = Mathf.Clamp01(value);
            float remaining = _currentCooldownNormalized * Traits.AttackCooldown;
            Events.OnCooldownChangedEvent.Invoke(remaining);
            Events.OnCooldownChangedNormalizedEvent.Invoke(_currentCooldownNormalized);
        }
    }

    public float EffectiveDamage => Traits.BaseDamage + _damageBonus;
    public float EffectiveRange => Traits.AttackRange + _rangeBonus;
    public bool IsReady => !_isSwinging && Time.time >= _nextAttackTime;

    private void Awake()
    {
        if (_castOrigin == null)
            _castOrigin = GetComponentInChildren<Camera>()?.transform;
    }

    private void Update()
    {
        if (Time.time < _nextAttackTime)
            CurrentCooldownNormalized = MeleeUtils.CooldownNormalized(_nextAttackTime, Traits.AttackCooldown);
    }

    public void Attack()
    {
        if (_isSwinging)
        {
            Events.OnAttackDeniedBusyEvent.Invoke();
            return;
        }

        if (Time.time < _nextAttackTime)
        {
            Events.OnAttackDeniedCooldownEvent.Invoke();
            return;
        }

        StartCoroutine(SwingRoutine(EffectiveDamage));
    }

    public void BeginCharge()
    {
        if (_isSwinging || Time.time < _nextAttackTime)
            return;

        _isCharging = true;
        _chargeStartTime = Time.time;
        StartCoroutine(ChargeReadyRoutine());
    }

    public void ReleaseCharge()
    {
        if (!_isCharging) return;

        _isCharging = false;
        float chargeDuration = Time.time - _chargeStartTime;
        float chargeNormalized = Mathf.Clamp01(chargeDuration / Traits.AttackCooldown);
        Events.OnChargeReleasedEvent.Invoke(chargeNormalized);

        float scaledDamage = MeleeUtils.ScaleDamageByCharge(EffectiveDamage, chargeNormalized, 2f);
        StartCoroutine(SwingRoutine(scaledDamage));
    }

    public void CancelAttack()
    {
        if (!_isSwinging) return;

        StopAllCoroutines();
        _isSwinging = false;
        _isCharging = false;
        _hitThisSwing.Clear();

        if (_weaponHitbox != null)
            _weaponHitbox.SetActive(false);

        Events.OnSwingCancelledEvent.Invoke();
    }

    public void AddDamageBonus(float amount) => _damageBonus += amount;
    public void RemoveDamageBonus(float amount) => _damageBonus -= amount;
    public void AddRangeBonus(float amount) => _rangeBonus += amount;
    public void RemoveRangeBonus(float amount) => _rangeBonus -= amount;

    private IEnumerator SwingRoutine(float damage)
    {
        _isSwinging = true;
        _nextAttackTime = Time.time + Traits.AttackCooldown;
        _hitThisSwing.Clear();

        if (_animator != null && !string.IsNullOrEmpty(Traits.AttackTrigger))
            _animator.SetTrigger(Traits.AttackTrigger);

        if (_swingSound != null)
            AudioSource.PlayClipAtPoint(_swingSound, _castOrigin.position);

        Events.OnSwingStartEvent.Invoke();

        yield return new WaitForSeconds(Traits.HitWindowStart);

        Events.OnHitWindowOpenEvent.Invoke();

        if (Traits.DetectionMode == MeleeDetectionMode.TriggerCollider)
        {
            if (_weaponHitbox != null)
            {
                _weaponHitbox.OnContact += HandleHitboxContact;
                _weaponHitbox.SetActive(true);
            }
        }
        else
        {
            PerformCast(damage);
        }

        yield return new WaitForSeconds(Traits.HitWindowEnd - Traits.HitWindowStart);

        if (Traits.DetectionMode == MeleeDetectionMode.TriggerCollider && _weaponHitbox != null)
        {
            _weaponHitbox.SetActive(false);
            _weaponHitbox.OnContact -= HandleHitboxContact;
        }

        if (_hitThisSwing.Count == 0)
            Events.OnMissEvent.Invoke();

        _isSwinging = false;
    }

    private IEnumerator ChargeReadyRoutine()
    {
        yield return new WaitForSeconds(Traits.AttackCooldown);

        if (_isCharging)
            Events.OnChargeReadyEvent.Invoke();
    }

    private void PerformCast(float damage)
    {
        Vector3 origin = _castOrigin.position;
        Vector3 dir = AimDirection();
        float range = EffectiveRange;

        switch (Traits.DetectionMode)
        {
            case MeleeDetectionMode.Raycast:
                {
                    if (MeleeUtils.TryRaycast(origin, dir, range, Traits.HitLayers, out RaycastHit hit))
                    {
                        var (point, normal) = MeleeUtils.ContactFromHit(hit);
                        ResolveHit(hit.collider, point, normal, damage);
                    }
                    break;
                }
            case MeleeDetectionMode.SphereCast:
                {
                    if (MeleeUtils.TrySphereCast(origin, dir, Traits.AttackRadius, range, Traits.HitLayers, out RaycastHit hit))
                    {
                        var (point, normal) = MeleeUtils.ContactFromHit(hit);
                        ResolveHit(hit.collider, point, normal, damage);
                    }
                    break;
                }
            case MeleeDetectionMode.OverlapSphere:
                {
                    Collider[] cols = MeleeUtils.OverlapSphere(origin, dir, range, Traits.AttackRadius, Traits.HitLayers);
                    int count = Mathf.Min(cols.Length, Traits.MaxTargets);
                    for (int i = 0; i < count; i++)
                    {
                        var (point, normal) = MeleeUtils.ContactFromCollider(cols[i], origin);
                        ResolveHit(cols[i], point, normal, damage);
                    }
                    break;
                }
        }
    }

    private void HandleHitboxContact(Collider col, Vector3 contactPoint)
    {
        Vector3 normal = (_castOrigin.position - contactPoint).normalized;
        ResolveHit(col, contactPoint, normal, EffectiveDamage);
    }

    private void ResolveHit(Collider col, Vector3 point, Vector3 normal, float damage)
    {
        if (!_hitThisSwing.Add(col)) return;

        Vector3 direction = -normal.normalized;

        var info = new DamageInfo
        {
            Amount = damage,
            Point = point,
            Direction = direction,
            Instigator = gameObject,
            SourceTag = "Melee"
        };

        IDamageable damageable = col.GetComponentInParent<IDamageable>();
        damageable?.TakeDamage(info);

        IKnockbackReceiver knockback = col.GetComponentInParent<IKnockbackReceiver>();
        knockback?.ApplyKnockback(info, Traits.Knockback);

        bool wasKillingBlow = damageable != null && damageable.IsDead;

        var result = new MeleeHitResult(col, point, normal, damage, Traits.Knockback, wasKillingBlow);

        if (_animator != null && !string.IsNullOrEmpty(Traits.ImpactTrigger))
            _animator.SetTrigger(Traits.ImpactTrigger);

        if (_hitSound != null)
            AudioSource.PlayClipAtPoint(_hitSound, point);

        if (_hitVFXPrefab != null)
            Instantiate(_hitVFXPrefab, point, Quaternion.LookRotation(normal));

        if (_hitThisSwing.Count > 1)
            Events.OnMultiHitEvent.Invoke(result);
        else
            Events.OnHitEvent.Invoke(result);
    }

    private Vector3 AimDirection()
    {
        return _castOrigin.forward;
    }
}
