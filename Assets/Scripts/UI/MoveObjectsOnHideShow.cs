using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveObjectsOnHideShow : MonoBehaviour
{
    public Transform objectToMove;  // ������, ������� ����� ����������
    public Transform showPosition;  // �������, ���� ���������� ������ ��� ����������� ��������
    public Transform originalPosition; // �������� ������� �������

    private bool isHidden = false;     // ����, �����������, ������ �� ��������

    private void Start()
    {
        // ����� ����������� �������
        objectToMove.position = showPosition.position;
    }

    public void ToggleImageVisibility()
    {
        isHidden = !isHidden; // ����������� ���� ��������� ��������

        if (isHidden)
        {
            // ���� �������� ����������, ���������� ������ �� �������� �������
            objectToMove.position = originalPosition.position;
        }
        else
        {
            // ���� �������� c�����, ���������� ������ �� ������� 
            objectToMove.position = showPosition.position;
        }
    }
}
