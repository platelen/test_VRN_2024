using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ThreeRangeHeal : AbilityBase
{
    [Header("Ability properties")]
    [SerializeField] private GameObject ManaCost;

    [HideInInspector] public GameObject Target;

    public delegate void ThirdAbilityHandler(float value);
    public event ThirdAbilityHandler ThirdAbilityEvent;

    protected override KeyCode ActivationKey => KeyCode.Alpha3;

    private void Start()
    {
        Distance = 6f * 1.9f;
        AttackType = AttackType.OneAttack;
        AbilityType = AbilityType.HealAbility;
    }

    void Update()
    {
        HandleToggleAbility();
        Target = TargetParent;
    }


    protected override void HandleToggleAbility()
    {
        base.HandleToggleAbility();
        // ������� ��� � ������ Update

        if (Input.GetMouseButtonDown(0) && ToggleAbility.gameObject.activeSelf && ToggleAbility.enabled && _player.GetComponent<PlayerMove>().IsSelect)
        {
            HandleLeftMouseButtonToggle();
        }
    }

    protected override void HandleToggleAbilityOn()
    {
        // ���������� ToggleAbility
        base.HandleToggleAbilityOn();

        if (TargetParent == null)
        {
            if (ManaCost != null)
            {
                ManaCost.SetActive(true);
                ManaCost.GetComponent<VisualManaCost>().CheckManaCost();
                ManaCost.transform.localScale = new Vector2(3f, ManaCost.gameObject.transform.localScale.y);
            }
            HandlePrefabVisibility();
            HandleTargetSelection();
        }

        if (TargetParent != null)
        {
            if (ManaCost != null)
            {
                ManaCost.gameObject.SetActive(false);
            }

            HandleDistanceToTarget();
        }
    }

    protected override void HandleToggleAbilityOff()
    {
        // ����������� ToggleAbility
        base.HandleToggleAbilityOff();

        if (_isSelect == false)
        {
            ManaCost.gameObject.SetActive(false);
        }
        TargetParent = null;
        return;
    }

    public override void OnLeftDoubleClick()
    {
        if (ShouldUseToggleTarget() || _isInputDoubleClick)
        {
            StartCoroutine(ToggleDoubleClick());
        }

        else if (AbilityTypeManager.ActiveAbilityType == 1 && _player.GetComponent<PlayerMove>().IsSelect && Abilities.gameObject.activeSelf)
        {
            if (_castCoroutine != null)
            {
                ToggleAbility.isOn = false;
                return;
            }
            else
            {
                StartCoroutine(EnemiesDoubleClick());
            }
        }
    }

    public override void OnRightDoubleClick()
    {
    }

    public override void ChangeBoolAndValues()
    {
        Destroy(NewAbilityPrefab);
    }

    private void HandleTargetSelection()
    {
        // ����� �����
        _targetPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(_targetPosition, Vector2.zero);

        if (hit.collider != null && hit.collider.CompareTag("Allies") && hit.collider.gameObject != gameObject)
        {
            TargetParent = hit.collider.gameObject;

            if (NewAbilityPrefab != null)
            {
                Destroy(NewAbilityPrefab);
            }
            DrawCircle.Clear();
        }
        else if (hit.collider != null && hit.collider.CompareTag("Allies") && hit.collider.gameObject == gameObject)
        {
            TargetParent = gameObject;

            if (NewAbilityPrefab != null)
            {
                Destroy(NewAbilityPrefab);
            }
            DrawCircle.Clear();
        }
    }

    public override void HandleDealDamageOrHeal()
    {
        if(_castCoroutine == null)
        {
            _castCoroutine = StartCoroutine(CastProtect(1.8f));
        }
    }

        private void Heal()
    {
        float heal = 35f;

        if(_playerAbility.GetComponent<OneRangeAttack>().ScriptInstanceCount == 1)
        {
            heal = 37f;
        }
        else if (_playerAbility.GetComponent<OneRangeAttack>().ScriptInstanceCount == 2)
        {
            heal = 41f;
        }

        if(TargetParent != null)
        {
            TargetParent.GetComponent<HealthPlayer>().AddHeal(heal);
        }

        _player.GetComponent<ManaPlayer>().UseMana(30f);

        ThirdAbilityEvent?.Invoke(heal);
        Recharge();
    }

    private IEnumerator CastProtect(float castTime)
    {
        if (Abilities.GetComponent<GlobalCooldown>())
        {
            Abilities.GetComponent<GlobalCooldown>().StartGlobalCooldown();
        }

        for (int i = 0; i < Abilities.transform.childCount; i++)
        {
            GameObject childObject = Abilities.transform.GetChild(i).gameObject;

            Toggle toggle = childObject.GetComponent<Toggle>();

            if (toggle != null)
            {
                toggle.enabled = false;
            }
        }
        _player.GetComponent<PlayerMove>().CanMove = false;
        CreateCastPrefab(castTime);

        yield return new WaitForSeconds(castTime);

        _castCoroutine = null;
        _player.GetComponent<PlayerMove>().CanMove = true;
        Select.GetComponent<SelectObject>().CanSelect = true;

        Heal();
    }

    private IEnumerator EnemiesDoubleClick()
    {
        yield return new WaitForSeconds(0.1f);

        ToggleAbility.isOn = true;
        HandleAbilityType();
    }


    private void Recharge()
    {
        for (int i = 0; i < Abilities.transform.childCount; i++)
        {
            GameObject childObject = Abilities.transform.GetChild(i).gameObject;

            Toggle toggle = childObject.GetComponent<Toggle>();

            if (toggle != null)
            {
                toggle.enabled = true;
            }
        }
        ToggleAbility.isOn = false;
        TargetParent = null;
        return;
    }
}
