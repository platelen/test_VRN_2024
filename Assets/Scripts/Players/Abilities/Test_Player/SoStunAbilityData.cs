using UnityEngine;

namespace Players.Abilities.Test_Player
{
    [CreateAssetMenu(menuName = "Create Stun Ability Data",fileName = "Stun Ability Data")]
    public class SoStunAbilityData:ScriptableObject
    {
        [SerializeField] private float _preparingStun = 2f;
        [SerializeField] private float _stunDuration = 5f;
        [SerializeField] private float _radiusCast = 5f;
        [SerializeField] private float _manaPrice = 20f;
        [SerializeField] private int _damageStun = 50;

        public float StunDuration => _stunDuration;

        public float RadiusCast => _radiusCast;

        public float ManaPrice => _manaPrice;

        public float PreparingStun => _preparingStun;

        public int DamageStun => _damageStun;
    }
}