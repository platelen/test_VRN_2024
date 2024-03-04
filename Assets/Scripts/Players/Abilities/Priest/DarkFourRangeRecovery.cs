using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DarkFourRangeRecovery : AbilityBase
{
    [Header("Ability properties")]
    [SerializeField] private GameObject DamageDebaffPrefab;
    [SerializeField] private GameObject ManaCost;

    [HideInInspector] public GameObject Target;

    public delegate void DarkFourthAbilityHandler(float value);
    public event DarkFourthAbilityHandler DarkFourthAbilityEvent;

    protected override KeyCode ActivationKey => KeyCode.Alpha4;

    private GameObject _newPrefab;

    private void Start()
    {
        Distance = 6f * 1.9f;
        AttackType = AttackType.OneAttack;
        AbilityType = AbilityType.DamageAbility;
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

        if (Input.GetMouseButtonDown(0) && _player.GetComponent<PlayerMove>().IsSelect && Abilities.gameObject.activeSelf && ToggleAbility.enabled == true)
        {
            HandleLeftMouseButtonToggle();
        }

        if (Input.GetMouseButtonDown(1) && _player.GetComponent<PlayerMove>().IsSelect && Abilities.gameObject.activeSelf && ToggleAbility.enabled == true)
        {
            Vector2 cursorPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(cursorPosition, Vector2.zero);

            if (hit.collider != null && hit.collider.CompareTag("Allies") && hit.collider.gameObject != gameObject)
            {
                HandleRightMouseButtonToggle();
            }
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
                ManaCost.transform.localScale = new Vector2(0.4f, ManaCost.gameObject.transform.localScale.y);
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
    }

    public override void OnRightDoubleClick()
    {
        if (AbilityTypeManager.ActiveAbilityType == 1 && _player.GetComponent<PlayerMove>().IsSelect && Abilities.gameObject.activeSelf)
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

    public override void ChangeBoolAndValues()
    {
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

            if (NewAbilityPrefab != null)
            {
                Destroy(NewAbilityPrefab);
            }
            DrawCircle.Clear();
        }
    }

    public override void HandleDealDamageOrHeal()
    {
        if (_castCoroutine == null)
        {
            _castCoroutine = StartCoroutine(CastProtect(1.2f));
        }
    }

    private void Damage()
    {
        if (_newPrefab != null && Target != null && _newPrefab.GetComponentInChildren<BaffDebaffEffectPrefab>())
        {
            _newPrefab.GetComponent<Damage>().Timer = Time.time;
            _newPrefab.GetComponentInChildren<BaffDebaffEffectPrefab>().StartCountdown(12);
        }
        else
        {
            _newPrefab = Instantiate(DamageDebaffPrefab);
            _newPrefab.transform.SetParent(Target.transform);
            _newPrefab.GetComponentInChildren<BaffDebaffEffectPrefab>().StartCountdown(12);
            _newPrefab.GetComponent<Damage>().CastRecovery(12f, 6f, 3f);

        }
        _player.GetComponent<ManaPlayer>().UseMana(4f);

        DarkFourthAbilityEvent?.Invoke(0f);
        Recharge();
    }

    private IEnumerator CastProtect(float castTime)
    {
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
        Damage();
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
        Select.GetComponent<SelectObject>().CanSelect = true;
        ToggleAbility.isOn = false;
        TargetParent = null;
        return;
    }
}