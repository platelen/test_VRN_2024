using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public enum AttackType
{
    OneAttack,
    Autoattack
}

public enum AbilityType
{
    DamageAbility,
    HealAbility
}

public abstract class AbilityBase : MonoBehaviour
{
    [Header("General properties of abilities")]
    [SerializeField] protected GameObject IconAbility;
    [SerializeField] protected GameObject AbilityPrefab;
    [SerializeField] protected GameObject Select;
    [SerializeField] protected GameObject Abilities;
    [SerializeField] protected LayerMask ObstacleLayerMask;
    [SerializeField] protected GameObject CastPrefab;
    public Toggle ToggleAbility;
    public DrawCircle DrawCircle;

    [Header("Autoattack")]
    [SerializeField] protected GameObject IconAbilityAutoattack;
    [SerializeField] protected GameObject CircleSelect;

    [HideInInspector] public AttackType AttackType;
    [HideInInspector] public AbilityType AbilityType;
    [HideInInspector] public GameObject TargetParent;
    [HideInInspector] public bool CanMakeDamage = false;
    [HideInInspector] public bool CanDealDamageOrHeal = false;
    [HideInInspector] public bool CanUseAbility = true;
    [HideInInspector] public GameObject NewAbilityPrefab;
    [HideInInspector] public bool CanDoAbility;
    [HideInInspector] public bool CanDrawCircle = true;
    [HideInInspector] public float Distance;
    [HideInInspector] public LastAbility lastAbility;
    [HideInInspector] public bool IsActiveAbility;

    public abstract void ChangeBoolAndValues();
    public abstract void OnLeftDoubleClick();
    public abstract void OnRightDoubleClick();
    public abstract void HandleDealDamageOrHeal();

    protected abstract KeyCode ActivationKey { get; }

    protected bool _isPrefab;
    protected bool _isSelect = true;
    protected bool _cursorIsActive = true;
    protected bool _isInputDoubleClick;
    protected bool _isInputClick = false;
    protected bool _cast;
    protected bool _addAbilityManager;
    protected bool _isLastAbility;
    protected bool _enemiesCirclesSelectActive;
    protected float _damageValue;
    protected float _distanceToTarget;
    protected float _timer;
    protected float elapsedTime = 0;
    protected float radius = 2.9f;
    protected float doubleClickTime = 1f;
    protected float lastClickTime;
    protected float _abilityCooldownTime = 1.0f;
    protected Vector2 _targetPosition;
    protected Vector2 _abilityPosition;
    protected Vector3 previousPosition = Vector3.zero;
    protected GameObject _newCastPrefab;
    protected HealthPlayer _targetHealth;
    protected Coroutine _castCoroutine;
    protected Coroutine _blinkCoroutine;
    private AbilityManager abilityManager;
    protected GameObject _targetCircle;
    protected GameObject _player;
    protected GameObject _playerAbility; 
    protected List <GameObject> _enemies = new List<GameObject>();
    protected List<GameObject> enemiesToRemove = new List<GameObject>();

    protected virtual void HandleToggleAbility()
    {
        // ������� ��� � ������ Update
        if (_player == null)
        {
            _player = transform.parent.gameObject;
        }

        if (_playerAbility == null)
        {
            _playerAbility = _player.transform.Find("Abilities").gameObject;
        }

        if (ToggleAbility.isOn == true)
        {
            if (TargetParent != null && _addAbilityManager == false)
            {
                abilityManager.AddAbilityToQueue(this);
                _addAbilityManager = true;
            }

            IconAbility.GetComponent<SpriteRenderer>().enabled = true;
            Color newColor = IconAbility.GetComponent<SpriteRenderer>().color;
            newColor.a = 1f;
            IconAbility.GetComponent<SpriteRenderer>().color = newColor;

            HandleToggleAbilityOn();
        }
        else if (ToggleAbility.isOn == false)
        {
            HandleToggleAbilityOff();
        }

        if (abilityManager == null)
        {
            abilityManager = _player.transform.Find("AbilityManager").GetComponent<AbilityManager>();
        }

        if(lastAbility == null)
        {
            lastAbility = _player.transform.Find("AbilityManager").GetComponent<LastAbility>();
        }

        if (AttackType == AttackType.Autoattack && _blinkCoroutine == null && CircleSelect.activeSelf)
        {
            Color newSelectColor = CircleSelect.GetComponent<SpriteRenderer>().color;
            newSelectColor.a = 0.7f;
            CircleSelect.GetComponent<SpriteRenderer>().color = newSelectColor;
        }

        if (Input.GetKeyDown(ActivationKey) && ToggleAbility.gameObject.activeSelf  && Abilities.gameObject.activeSelf && ToggleAbility.enabled && _player.GetComponent<PlayerMove>().IsSelect)
        {
            if (lastAbility != null && lastAbility.LastUseAbility == this && lastAbility.OneClick)
            {
                lastAbility.TwoClick = true;
                return;
            }
            else
            {
                HandleInputToggle();
                if (AbilityTypeManager.ActiveAbilityType == 0)
                {
                    HandleAbilityType();
                }
            }
        }

    }

    protected virtual void HandleToggleAbilityOn()
    {
        // ���������� ToggleAbility
        if (_player.GetComponent<PlayerMove>().IsSelect == false)
        {
            if (TargetParent == null)
            {
                ToggleAbility.isOn = false;
                return;
            }
            DrawCircle.Clear();
            CanDrawCircle = true;
        }

        if (CanDrawCircle && _player.GetComponent<PlayerMove>().IsSelect)
        {
            DrawCircle.Draw(Distance - (1.9f / 2f));
            CanDrawCircle = false;
        }

        if (_player.GetComponent<PlayerMove>().IsSelect && Input.GetMouseButtonDown(1) && _cast == false)
        {
            Vector2 cursorPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(cursorPosition, Vector2.zero);

            if (AbilityTypeManager.ActiveAbilityType == 1 && hit.collider != null && !hit.collider.CompareTag("Enemies") ||
                AbilityTypeManager.ActiveAbilityType == 1 && hit.collider == null ||
                AbilityTypeManager.ActiveAbilityType == 0)
            {
                AbilityBase[] abilities = _playerAbility.GetComponentsInChildren<AbilityBase>();
                List <AbilityBase> abilitiesCancel = new List <AbilityBase>();

                foreach (AbilityBase ability in abilities)
                {
                    if(ability.ToggleAbility.isOn && ability.TargetParent == null)
                    {
                        abilitiesCancel.Add(ability);   
                    }
                }

                if (abilitiesCancel.Count > 0)
                {
                    for (int i = 0; i < abilitiesCancel.Count; i++)
                    {
                        abilitiesCancel[i].CancelAbilityOnClick();
                    }
                }
                else
                {
                    CancelAbilityOnClick();
                }
            }
        }

        if (AttackType == AttackType.Autoattack && !CircleSelect.activeSelf && _blinkCoroutine != null)
        {
            _blinkCoroutine = null;
        }

        if (NewAbilityPrefab != null && _cursorIsActive == true)
        {
            Cursor.visible = false;
            _cursorIsActive = false;
        }

        if (NewAbilityPrefab == null && _cursorIsActive == false)
        {
            Cursor.visible = true;
            _cursorIsActive = true;
        }

        if(TargetParent == null)
{
            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, Distance);
            string targetTag = (AbilityType == AbilityType.DamageAbility) ? "Enemies" : "Allies";

            // ��������, ��� ������ ����� � �������������
            if (_enemies == null)
            {
                _enemies = new List<GameObject>();
            }

            // ������� ������ ��� ������ �����
            _enemies.Clear();

            foreach (Collider2D collider in colliders)
            {
                if (collider.CompareTag(targetTag))
                {
                    collider.GetComponent<PlayerMove>().CircleSelect.SetActive(true);
                    _enemies.Add(collider.gameObject);
                }
            }

            _enemiesCirclesSelectActive = true;

            // �������� ������ ��� ������, ������� ����� �������
            List<GameObject> enemiesToRemove = new List<GameObject>();

            foreach (GameObject enemy in _enemies)
            {
                float distanceToCollider = Vector2.Distance(transform.position, enemy.transform.position);

                if (distanceToCollider > Distance)
                {
                    enemiesToRemove.Add(enemy);
                }
            }

            // �������� ������ �� ������
            foreach (GameObject enemyToRemove in enemiesToRemove)
            {
                enemyToRemove.GetComponent<PlayerMove>().CircleSelect.SetActive(false);
                _enemies.Remove(enemyToRemove);
            }
        }

        if (TargetParent != null)
        {
            AddPsionicsForMoving();

            if (AttackType == AttackType.Autoattack)
            {
                if (_blinkCoroutine == null)
                {
                    _blinkCoroutine = StartCoroutine(Blink());
                }

                IconAbilityAutoattack.SetActive(true);
            }

            if (_enemiesCirclesSelectActive)
            {
                List<GameObject> enemiesToRemove = new List<GameObject>();

                foreach (GameObject enemy in _enemies)
                {
                    enemy.GetComponent<PlayerMove>().CircleSelect.SetActive(false);
                    enemiesToRemove.Add(enemy);
                }

                // �������� �������� �� _enemies
                foreach (GameObject enemyToRemove in enemiesToRemove)
                {
                    _enemies.Remove(enemyToRemove);
                }

                enemiesToRemove.Clear(); // ������� ������ ��������� ��������

                _enemiesCirclesSelectActive = false;
             }
        }
    }

    protected virtual void HandleToggleAbilityOff()
    {
        // ����������� ToggleAbility
        IsActiveAbility = false;
        _isPrefab = false;

        if (_targetCircle != null && TargetParent != Select.GetComponent<SelectObject>().SelectedObject)
        {
            _targetCircle.SetActive(false);
        }

        if (NewAbilityPrefab != null)
        {
            Destroy(NewAbilityPrefab);
        }

        if(AttackType == AttackType.Autoattack)
        {
            IconAbilityAutoattack.SetActive(false);
            if (_blinkCoroutine != null)
            {
                StopCoroutine(_blinkCoroutine);
                _blinkCoroutine = null;
            }
        }

        if (_enemies != null)
        {
            foreach (GameObject enemy in _enemies)
            {               
                    enemy.GetComponent<PlayerMove>().CircleSelect.SetActive(false);
            }

            _enemies.Clear();
            _enemiesCirclesSelectActive = false;
        }

        if (NewAbilityPrefab == null && _cursorIsActive == false)
        {
            Cursor.visible = true;
            _cursorIsActive = true;
        }

        if (DrawCircle.lineRenderer && DrawCircle.lineRenderer.positionCount > 0 && CanDrawCircle == false)
        {
            DrawCircle.Clear();
        }
        _targetCircle = null;
        CanDrawCircle = true;
        IconAbility.GetComponent<SpriteRenderer>().enabled = false;
        _addAbilityManager = false;
        CanDoAbility = false;
        _isLastAbility = false;
        return;
    }

    public void HandleInputToggle()
    {
        if (!_isInputClick)
        {
            //����� ����������� ��������
            float timeSinceLastClick = Time.time - lastClickTime;

            if (timeSinceLastClick <= doubleClickTime)
            {
                _isInputDoubleClick = true;
                OnLeftDoubleClick();
                StartCoroutine(AbilityCooldown());
            }
            else
            {
                ToggleAbility.isOn = !ToggleAbility.isOn;
            }

            lastClickTime = Time.time;
        }
    }

    public void HandleLeftMouseButtonToggle()
    {
        if (!_isInputClick)
        {
            float timeSinceLastClick = Time.time - lastClickTime;

            if (timeSinceLastClick <= doubleClickTime)
            {
                OnLeftDoubleClick();
                StartCoroutine(AbilityCooldown());
            }

            lastClickTime = Time.time;
        }
    }

    public void HandleRightMouseButtonToggle()
    {
        if (!_isInputClick)
        {
            float timeSinceLastClick = Time.time - lastClickTime;

            if (timeSinceLastClick <= doubleClickTime)
            {
                OnRightDoubleClick();
                StartCoroutine(AbilityCooldown());
            }

            lastClickTime = Time.time;
        }
    }

    public void HandleAbilityType()
    {
        Vector2 cursorPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(cursorPosition, Vector2.zero);
        string targetTag = (AbilityType == AbilityType.DamageAbility) ? "Enemies" : "Allies";

        if (hit.collider != null && hit.collider.CompareTag(targetTag) && hit.collider.gameObject != gameObject)
        {
            ToggleAbility.isOn = true;

            _distanceToTarget = (hit.collider.transform.position - transform.position).magnitude;
            if (_distanceToTarget <= Distance)
            {
                TargetParent = hit.collider.gameObject;
                ChangeBoolAndValues();
            }
        }
    }

    public bool ShouldUseToggleTarget()
    { 
        RectTransform rectTransform = ToggleAbility.GetComponent<RectTransform>();

        // ���������, ��������� �� ������ ��� toggleTarget
        return RectTransformUtility.RectangleContainsScreenPoint(rectTransform, Input.mousePosition, Camera.main);
    }

    public void CancelAbilityOnClick()
    {
        Destroy(NewAbilityPrefab);
        ToggleAbility.isOn = false;
        return;
    }

    public void HandlePrefabVisibility()
    {
        // �������� �������
        _abilityPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        if (NewAbilityPrefab == null && _isPrefab == false)
        {
            NewAbilityPrefab = Instantiate(AbilityPrefab, _abilityPosition, Quaternion.identity);
            _isPrefab = true;
        }
        else if (NewAbilityPrefab != null)
        {
            NewAbilityPrefab.transform.position = _abilityPosition;
        }
    }

    public void HandleDistanceToTarget()
    {
        // �������� ���������
        if (CanDoAbility)
        {
            _distanceToTarget = (TargetParent.transform.position - _player.transform.position).magnitude;

        if (_distanceToTarget <= Distance && AttackType == AttackType.Autoattack && CanDealDamageOrHeal ||
            _distanceToTarget <= Distance && AttackType == AttackType.OneAttack)
        {

            if (_targetCircle == null)
            {
                _targetCircle = TargetParent.GetComponent<PlayerMove>().CircleSelect;
            }

            if(!_targetCircle.activeSelf && _player == Select.GetComponent<SelectObject>().SelectedObject)
            {
                _targetCircle.SetActive(true);
            }

            if (_player != Select.GetComponent<SelectObject>().SelectedObject && _targetCircle.activeSelf && TargetParent != Select.GetComponent<SelectObject>().SelectedObject)
            {
               _targetCircle.SetActive(false);
            }


                if (!CheckObstacleBetween(_player.transform.position, TargetParent.transform.position))
                {
                    _player.GetComponent<Rigidbody2D>().velocity = Vector2.zero;

                    IsActiveAbility = true;

                    HandleDealDamageOrHeal();

                    if (_isLastAbility == false && lastAbility != null)
                    {
                        lastAbility.AddLastAbility(this);
                        _isLastAbility = true;
                    }
                }
            }
        }
        else
        {
            IsActiveAbility = false;

            if (_targetCircle == null)
            {
                _targetCircle = TargetParent.GetComponent<PlayerMove>().CircleSelect;
            }

            if (_targetCircle.activeSelf && TargetParent != Select.GetComponent<SelectObject>().SelectedObject) 
            {
               _targetCircle.SetActive(false);
            }
        }
    }

    public void AddPsionicsForMoving()
    {
        // ���������� �������� ��� ����������� �����
        if (previousPosition == Vector3.zero)
        {
            previousPosition = TargetParent.transform.position;
        }

        elapsedTime += Time.deltaTime;
        if (elapsedTime >= 0.1f)
        {
            Vector3 currentPosition = TargetParent.transform.position;

            if (Vector3.Distance(previousPosition, currentPosition) >= 0.19f && GetComponent<PsionicaMelee>())
            {
                GetComponent<PsionicaMelee>().MakePsionica(0.3f);
            }

            elapsedTime = 0f;
            previousPosition = currentPosition;
        }
    }

    public void CreateCastPrefab(float time)
    {
        Vector2 newVector = new Vector2(_player.transform.position.x, _player.transform.position.y - 1);
        _newCastPrefab = Instantiate(CastPrefab, newVector, Quaternion.identity);
        _newCastPrefab.transform.SetParent(_player.transform);
        Transform childObject = _newCastPrefab.transform.Find("GameObject");
        if (childObject != null)
        {
            StartCoroutine(ScaleObjectOverTime(childObject, 1f, time));
        }
    }

    private IEnumerator ScaleObjectOverTime(Transform targetTransform, float targetScaleX, float duration)
    {
        float elapsedTime = 0f;
        Vector3 initialScale = targetTransform.localScale;
        Vector3 targetScale = new Vector3(targetScaleX, targetTransform.localScale.y, targetTransform.localScale.z);
        _cast = true;
        while (elapsedTime < duration && targetTransform != null)
        {
            targetTransform.localScale = Vector3.MoveTowards(initialScale, targetScale, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        if (targetTransform != null)
        {
            targetTransform.localScale = targetScale;
            _cast = false;
        }
    }

    bool CheckObstacleBetween(Vector3 start, Vector3 end)
    {
        Vector2 direction = (end - start).normalized;
        float distance = Vector2.Distance(start, end);

        RaycastHit2D[] hits = Physics2D.BoxCastAll(start, new Vector2(1f, 1f), 0f, direction, distance, ObstacleLayerMask);

        foreach (RaycastHit2D hit in hits)
        {
            return true;
        }

        return false;
    }

    private IEnumerator AbilityCooldown()
    {
        _isInputClick = true;
        yield return new WaitForSeconds(_abilityCooldownTime);
        _isInputClick = false;
    }

    public IEnumerator ToggleDoubleClick()
    {
        yield return new WaitForSeconds(0.1f);
        ToggleAbility.isOn = true;

        Collider2D[] colliders = Physics2D.OverlapCircleAll(_player.transform.position, Distance);
        Collider2D closestCollider = null;
        float closestDistance = float.MaxValue;

        if (colliders.Length > 0)
        {
            foreach (Collider2D collider in colliders)
            {
                string targetTag = (AbilityType == AbilityType.DamageAbility) ? "Enemies" : "Allies";

                if (collider.CompareTag(targetTag) && collider.gameObject != gameObject)
                {
                    float currentDistance = Vector2.Distance(_player.transform.position, collider.bounds.center);

                    if (currentDistance < closestDistance)
                    {
                        closestCollider = collider;
                        closestDistance = currentDistance;
                    }
                }
            }
        }

        if (closestCollider != null)
        {

            TargetParent = closestCollider.gameObject;
            ChangeBoolAndValues();
        }
        _isInputDoubleClick = false;
    }

    public IEnumerator DoNotDoubleClickAtTarget()
    {
        yield return new WaitForSeconds(0.1f);

        ToggleAbility.isOn = false;
        DrawCircle.Clear();
    }

    private IEnumerator Blink()
    {
        while (true && _player == Select.GetComponent<SelectObject>().SelectedObject)
        {
            // ���������
            for (float t = 0f; t < 1; t += Time.deltaTime)
            {
                float normalizedTime = t / 1;
                float alpha = Mathf.Lerp(1f, 0f, normalizedTime);

                Color newColor = IconAbility.GetComponent<SpriteRenderer>().color;
                newColor.a = alpha;
                IconAbility.GetComponent<SpriteRenderer>().color = newColor;

                Color newAutoattackColor = IconAbilityAutoattack.GetComponent<SpriteRenderer>().color;
                newAutoattackColor.a = alpha;
                IconAbilityAutoattack.GetComponent<SpriteRenderer>().color = newAutoattackColor;

                Color newSelectColor = CircleSelect.GetComponent<SpriteRenderer>().color;
                newSelectColor.a = alpha;
                CircleSelect.GetComponent<SpriteRenderer>().color = newSelectColor;

                DrawCircle.lineRenderer.startColor = new Color(DrawCircle.lineColor.r, DrawCircle.lineColor.g, DrawCircle.lineColor.b, alpha);
                DrawCircle.lineRenderer.endColor = new Color(DrawCircle.lineColor.r, DrawCircle.lineColor.g, DrawCircle.lineColor.b, alpha);

                yield return null;
            }

            // ���������
            for (float t = 0f; t < 1; t += Time.deltaTime)
            {
                float normalizedTime = t / 1;
                float alpha = Mathf.Lerp(0f, 1f, normalizedTime);

                Color newColor = IconAbility.GetComponent<SpriteRenderer>().color;
                newColor.a = alpha;
                IconAbility.GetComponent<SpriteRenderer>().color = newColor;

                Color newAutoattackColor = IconAbilityAutoattack.GetComponent<SpriteRenderer>().color;
                newAutoattackColor.a = alpha;
                IconAbilityAutoattack.GetComponent<SpriteRenderer>().color = newAutoattackColor;

                Color newSelectColor = CircleSelect.GetComponent<SpriteRenderer>().color;
                newSelectColor.a = alpha;
                CircleSelect.GetComponent<SpriteRenderer>().color = newSelectColor;

                DrawCircle.lineRenderer.startColor = new Color(DrawCircle.lineColor.r, DrawCircle.lineColor.g, DrawCircle.lineColor.b, alpha);
                DrawCircle.lineRenderer.endColor = new Color(DrawCircle.lineColor.r, DrawCircle.lineColor.g, DrawCircle.lineColor.b, alpha);

                yield return null;
            }
        }
    }
}