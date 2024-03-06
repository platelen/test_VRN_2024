using UnityEngine;

namespace Players.Abilities.Test_Player
{
    [CreateAssetMenu(menuName = "Create Stun Ability Data",fileName = "Stun Ability Data")]
    public class SoStunAbilityData:ScriptableObject
    {
        [SerializeField] private float _stunDuration = 2.0f;
        [SerializeField] private float _radiusCast = 5f;
        [SerializeField] private float _manaPrice = 20f;

        public float StunDuration => _stunDuration;

        public float RadiusCast => _radiusCast;

        public float ManaPrice => _manaPrice;
    }
}