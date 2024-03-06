using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerMove : MonoBehaviour
{
    private Rigidbody2D _rigidbody;
    [HideInInspector] public bool CanMove = true;
    [HideInInspector] public bool IsMoving;
    [HideInInspector] public bool IsSelect;
    [HideInInspector] public Vector2 DirectionOfMovement;
    public float MoveSpeed = 5.0f;
    public GameObject SelectObject;
    public GameObject CircleSelect;
    public GameObject MarkersSelect;
    public GameObject AbilityPanel;
    public List<Toggle> AbilitiesOnTargetToggles;

    [Header("Move By Mouse Click")] [SerializeField]
    private float _distanceToTargetPosition = 0.1f;

    private Vector3 _targetPosition;

    private void Start()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _rigidbody.isKinematic = true;

        _targetPosition = transform.position; //Чтобы персонаж, не двигался на старте игры.
        Deselect();
    }

    void Update()
    {
        if (SelectObject.GetComponent<SelectObject>().SelectedObject == gameObject && IsSelect == false)
        {
            Select();
        }
        else if (SelectObject.GetComponent<SelectObject>().SelectedObject != gameObject && IsSelect == true)
        {
            Deselect();
        }

        if (Input.GetMouseButtonDown(1) && IsSelect && CanMove)
        {
            _targetPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            _targetPosition.z = 0;

            //Debug.Log("Clicked at: " + _targetPosition); //Место клика.
        }

        IsMoving = _rigidbody.velocity != Vector2.zero;

        if (IsMoving)
        {
            DirectionOfMovement = _rigidbody.velocity.normalized;
        }
    }

    void FixedUpdate()
    {
        if (IsSelect && CanMove)
        {
            if (Vector2.Distance(transform.position, _targetPosition) > _distanceToTargetPosition)
            {
                MoveToPosition(_targetPosition);
            }
            else
            {
                _rigidbody.velocity = Vector2.zero;
                _rigidbody.isKinematic = true;
            }
        }
    }

    // Новые методы для управления станом
    public void Stun(float duration)
    {
        CanMove = false;
        _rigidbody.velocity = Vector2.zero;
        Debug.Log($"Персонаж {gameObject.name} в стане");
    }

    public void EndStun()
    {
        CanMove = true;
        Debug.Log($"Персонаж {gameObject.name} вышел из стана");
    }


    private void Select()
    {
        IsSelect = true;
        CircleSelect.SetActive(true);
        AbilityPanel.SetActive(true);
        MarkersSelect.SetActive(true);
    }

    private void Deselect()
    {
        IsSelect = false;
        CircleSelect.SetActive(false);
        AbilityPanel.SetActive(false);
        MarkersSelect.SetActive(false);
    }

    //Метод перемещения к позиции щелчка мышки.
    private void MoveToPosition(Vector3 targetPosition)
    {
        _rigidbody.isKinematic = false;
        Vector2 direction = (targetPosition - transform.position).normalized;
        _rigidbody.velocity = direction * MoveSpeed * Time.deltaTime * 100;
    }
}