using System;
using UnityEngine;

namespace Players.Abilities.Test_Player
{
    public class StunCast : AbilityBase
    {
        [Header("Ability properties")] [SerializeField]
        private GameObject ManaCost;

        [Header("Stun properties")] [SerializeField]
        private float stunDuration = 2.0f;

        [SerializeField] private float _radiusCast = 5f;


        protected override KeyCode ActivationKey => KeyCode.Alpha1;
        private bool _isAbilityActivated;
        private bool _isKeyActivated;

        public delegate void FirstAbilityHandler(float value);

        public event FirstAbilityHandler FirstAbilityEvent;


        private void Update()
        {
            CheckActivationKey();

            if (_isKeyActivated)
            {
                CheckPosMouse();
            }
        }

        private void CheckActivationKey()
        {
            if (Input.GetKeyDown(ActivationKey))
            {
                ToggleActivation();
            }
        }

        private void ToggleActivation()
        {
            _isKeyActivated = !_isKeyActivated;
            _isAbilityActivated = _isKeyActivated;
        }

        private void CheckPosMouse()
        {
            Vector2 mousePosition =
                Camera.main.ScreenToWorldPoint(Input.mousePosition);
            float distanceToMouse = Vector2.Distance(transform.position, mousePosition);

            if (distanceToMouse <= _radiusCast)
            {
                Debug.Log("Указатель мыши находится в заданном радиусе");
            }
            else
            {
                Debug.Log("Указатель мыши не находится в заданном радиусе");
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, _radiusCast);
        }


        public override void ChangeBoolAndValues()
        {
            throw new NotImplementedException();
        }

        public override void OnLeftDoubleClick()
        {
            throw new NotImplementedException();
        }

        public override void OnRightDoubleClick()
        {
            throw new NotImplementedException();
        }

        public override void HandleDealDamageOrHeal()
        {
            throw new NotImplementedException();
        }
    }
}