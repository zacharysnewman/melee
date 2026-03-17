using UnityEngine.Events;

[System.Serializable]
public class MeleeHitResultEvent : UnityEvent<MeleeHitResult> { }

[System.Serializable]
public class MeleeFloatEvent : UnityEvent<float> { }

[System.Serializable]
public class MeleeEvents
{
    // Value events
    public MeleeFloatEvent OnCooldownChangedEvent = new MeleeFloatEvent();
    public MeleeFloatEvent OnCooldownChangedNormalizedEvent = new MeleeFloatEvent();

    // Semantic events
    public UnityEvent OnSwingStartEvent = new UnityEvent();
    public UnityEvent OnHitWindowOpenEvent = new UnityEvent();
    public UnityEvent OnMissEvent = new UnityEvent();
    public MeleeHitResultEvent OnHitEvent = new MeleeHitResultEvent();
    public MeleeHitResultEvent OnMultiHitEvent = new MeleeHitResultEvent();
    public UnityEvent OnAttackDeniedCooldownEvent = new UnityEvent();
    public UnityEvent OnAttackDeniedBusyEvent = new UnityEvent();
    public UnityEvent OnSwingCancelledEvent = new UnityEvent();
    public UnityEvent OnChargeReadyEvent = new UnityEvent();
    public MeleeFloatEvent OnChargeReleasedEvent = new MeleeFloatEvent();
}
