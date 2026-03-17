using System;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class MeleeHitbox : MonoBehaviour
{
    public event Action<Collider, Vector3> OnContact;

    private bool _active;

    public void SetActive(bool active)
    {
        _active = active;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!_active) return;
        Vector3 closestPoint = other.ClosestPoint(transform.position);
        OnContact?.Invoke(other, closestPoint);
    }
}
