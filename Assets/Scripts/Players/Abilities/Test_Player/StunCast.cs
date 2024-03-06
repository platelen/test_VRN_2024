using System;
using System.Collections;
using UnityEngine;

namespace Players.Abilities.Test_Player
{
    public class StunCast : AbilityBase
    {
        [Header("Ability properties")] [SerializeField]
        private GameObject ManaCost;

        [Header("Stun properties")] [SerializeField]
        private SoStunAbilityData _soStunAbilityData;

        protected override KeyCode ActivationKey => KeyCode.Alpha1;
        private bool _isAbilityActivated;
        private bool _isKeyActivated;
        private bool _isPreparingStun;


        private void Update()
        {
            CheckActivationKey();

            if (_isKeyActivated)
            {
                DrawCircle.Draw(_soStunAbilityData.RadiusCast);
                CheckPosMouse();
            }
            else
            {
                DrawCircle.Clear();
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

            if (distanceToMouse <= _soStunAbilityData.RadiusCast)
            {
                HandleTargetSelection();
            }
            else
            {
                Debug.Log("Указатель мыши не находится в заданном радиусе");
            }
        }

        private void HandleTargetSelection()
        {
            // Выбор врага
            _targetPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(_targetPosition, Vector2.zero);

            int playerLayer = LayerMask.NameToLayer("Player");

            if (hit.collider != null && hit.collider.CompareTag("Enemies") &&
                hit.collider.gameObject.layer != playerLayer)
            {
                StartCoroutine(ApplyStunToEnemy(hit.collider.gameObject));
                DrawCircle.Clear();
                ToggleActivation();

                Debug.Log($"Тэг обжекта: {hit.collider.gameObject.tag} Имя обжекта: {hit.collider.gameObject.name}");
            }
            else if (hit.collider != null && hit.collider.CompareTag("Allies"))
            {
                Debug.Log($"Тэг обжекта: {hit.collider.gameObject.tag} Имя обжекта: {hit.collider.gameObject.name}");
            }
        }

        private IEnumerator PreparingStunCast()
        {
            _isPreparingStun = true;
            Debug.Log("Подготовка к касту стана");
            yield return new WaitForSeconds(_soStunAbilityData.PreparingStun);
            _isPreparingStun = false;
        }

        //Метод стана.
        private IEnumerator ApplyStunToEnemy(GameObject enemy)
        {
            PlayerMove enemyMoveComponent = enemy.GetComponent<PlayerMove>();

            if (gameObject.transform.parent != null)
            {
                gameObject.transform.parent.GetComponent<ManaPlayer>().UseMana(_soStunAbilityData.ManaPrice);
                StartCoroutine(PreparingStunCast());
                
                // Дожидаемся окончания подготовки стана
                while (_isPreparingStun)
                {
                    yield return null;
                }
            }


            if (enemyMoveComponent != null)
            {
                enemyMoveComponent.Stun(_soStunAbilityData.StunDuration);
                enemyMoveComponent.GetComponent<HealthPlayer>().TakeMagicDamage(_soStunAbilityData.DamageStun);
                _isPreparingStun = true;
            }

            yield return new WaitForSeconds(_soStunAbilityData.StunDuration);

            if (enemyMoveComponent != null)
            {
                enemyMoveComponent.EndStun();
                StopAllCoroutines();
            }
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