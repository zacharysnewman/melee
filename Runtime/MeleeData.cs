using UnityEngine;

[CreateAssetMenu(menuName = "Melee/Melee Data")]
public class MeleeData : ScriptableObject
{
    [SerializeField] private MeleeTraits _traits;

    public MeleeTraits Traits => _traits;
}
