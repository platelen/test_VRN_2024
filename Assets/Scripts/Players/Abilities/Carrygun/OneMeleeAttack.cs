using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OneMeleeAttack : AbilityBase
{
    public delegate void FirstAbilityHandler(float value);
    public event FirstAbilityHandler FirstAbilityEvent;

    [HideInInspector] public GameObject Target;
    [HideInInspector] public float DamageRate = 1.2f;
    [HideInInspector] public bool IsGlobalCooldown = true;
    [HideInInspector] public bool TargetCanAvoidance = true;

    private Toggle _toggleSecondAbility;
    private bool _isOneChange;

    protected override KeyCode ActivationKey => KeyCode.Alpha1;

    private void Start()
    {
        Distance = 1.94f;
        AttackType = AttackType.Autoattack;
        AbilityType = AbilityType.DamageAbility;
    }

    private void Update()
    {
        HandleToggleAbility();
        Target = TargetParent;

        if (_toggleSecondAbility == null && _playerAbility != null)
        {
            _toggleSecondAbility = _playerAbility.GetComponent<TwoMeleeAttack>().ToggleAbility;
        }
    }

    protected override void HandleToggleAbility()
    {
        base.HandleToggleAbility();
        // ������� ��� � ������ Update
        if (_toggleSecondAbility != null &&!_toggleSecondAbility.isOn && !ToggleAbility.isOn)
        {
            TargetParent = null;
            _isOneChange = false;
        }

        if (_toggleSecondAbility != null && _toggleSecondAbility.isOn)
        {
            _isOneChange = false;
        }

        if (Input.GetMouseButtonDown(0) && _player.GetComponent<PlayerMove>().IsSelect && Abilities.gameObject.activeSelf)
        {
            HandleLeftMouseButtonToggle();
            if (AbilityTypeManager.ActiveAbilityType == 1 && _toggleSecondAbility.isOn == false)
            {
                if (_castCoroutine != null)
                {
                    ToggleAbility.isOn = false;
                    return;
                }
                else
                {
                    HandleAbilityType();
                }
            }
        }
    }

    protected override void HandleToggleAbilityOn()
    {
        // ���������� ToggleAbility
        base.HandleToggleAbilityOn();

        if (_playerAbility.GetComponent<TwoMeleeAttack>().Target != null)
        {
            TargetParent = _playerAbility.GetComponent<TwoMeleeAttack>().Target;
            if (_isOneChange == false)
            {
                ChangeBoolAndValues();
            }
        }

        if (TargetParent == null)
        {
            HandlePrefabVisibility();
            HandleTargetSelection();
        }

        if (TargetParent != null)
        {
            HandleDistanceToTarget();
        }
    }

    protected override void HandleToggleAbilityOff()
    {
        // ����������� ToggleAbility
        base.HandleToggleAbilityOff();

        CanDealDamageOrHeal = false;
        CanMakeDamage = false;
    }

    public override void OnLeftDoubleClick()
    {
        if (ShouldUseToggleTarget() || _isInputDoubleClick)
        {
            StartCoroutine(ToggleDoubleClick());
        }
        else if (AbilityTypeManager.ActiveAbilityType == 1 && _player.GetComponent<PlayerMove>().IsSelect && Abilities.gameObject.activeSelf)
        {
            StartCoroutine(DoNotDoubleClickAtTarget());
        }
    }

    public override void OnRightDoubleClick()
    {
    }

    public override void ChangeBoolAndValues()
    {
        _targetHealth = TargetParent.GetComponent<HealthPlayer>();
        CanMakeDamage = true;
        CanDealDamageOrHeal = true;
        _isOneChange = true;
        Destroy(NewAbilityPrefab);
    }

    private void HandleTargetSelection()
    {
        // ����� �����

        _targetPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(_targetPosition, Vector2.zero);
        if (hit.collider != null && hit.collider.CompareTag("Enemies") && hit.collider.gameObject != gameObject)
        {
            TargetParent = hit.collider.gameObject;
            ChangeBoolAndValues();

            if (NewAbilityPrefab != null)
            {
                Destroy(NewAbilityPrefab);
            }
        }
    }


    public override void HandleDealDamageOrHeal()
    {
        // ��������� �����
        _damageValue = 12;

        if (CanMakeDamage && _castCoroutine == null && CanUseAbility)
        {
            if (Abilities.GetComponent<GlobalCooldown>() && IsGlobalCooldown)
            {
                Abilities.GetComponent<GlobalCooldown>().StartGlobalCooldown();
            }
            FirstAbilityEvent?.Invoke(_damageValue);
            _castCoroutine = StartCoroutine(Damage(DamageRate));
        }
    }

    private void HandleActivePsionica()
    {
        //�������� ��� �������� ��������

        float activePsionica = _playerAbility.GetComponent<FiveConversion>().PsionicaActive;
        if (activePsionica > 0)
        {
            List<BaseEffect> buffEffects = new List<BaseEffect>();
            Component[] allEffects = TargetParent.GetComponents<Component>();

            foreach (Component effectComponent in allEffects)
            {
                if (effectComponent is BaseEffect effect && effect.Type == EffectType.Buff)
                {
                    buffEffects.Add(effect);
                }
            }

            if (buffEffects.Count > 0)
            {
                if (activePsionica >= 30)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        Destroy(buffEffects[i]);
                    }
                }
                else if (activePsionica >= 20 && activePsionica < 30)
                {
                    for (int i = 0; i < 2; i++)
                    {
                        Destroy(buffEffects[i]);
                    }
                }
                else if (activePsionica >= 10 && activePsionica < 20)
                {
                    for (int i = 0; i < 1; i++)
                    {
                        Destroy(buffEffects[i]);
                    }
                }
            }
            StartCoroutine(DamageCooldown(activePsionica));
            _playerAbility.GetComponent<FiveConversion>().UseActivePsionica(activePsionica, Target);
        }
    }

    private void MissAtDistance()
    {
        // ������ ��� ��������� ����� ��� �� 10% �� �������
        if (previousPosition == Vector3.zero)
        {
            previousPosition = TargetParent.transform.position;
        }

        float time = 0;
        time += Time.deltaTime;
        if (time < 0.6f)
        {
            Vector3 currentPosition = TargetParent.transform.position;

            if (Vector3.Distance(previousPosition, currentPosition) >= 0.19f)
            {
                _damageValue = 0;
            }

            previousPosition = currentPosition;
        }
    }

    private IEnumerator Damage(float damageRate)
    {
        if (TargetCanAvoidance)
        {
            MissAtDistance();
        }

        yield return new WaitForSeconds(damageRate / 2);

        _targetHealth.TakePhisicDamage(_damageValue);
        _player.GetComponent<PsionicaMelee>().MakePsionica(_damageValue);
        HandleActivePsionica();
        CanMakeDamage = false;

        yield return new WaitForSeconds(damageRate / 2);

        _castCoroutine = null;
        CanMakeDamage = true;
    }

    private IEnumerator DamageCooldown(float activePsionica)
    {
        yield return new WaitForSeconds(0.1f);
        _targetHealth.TakeMagicDamage(activePsionica);
    }
}


