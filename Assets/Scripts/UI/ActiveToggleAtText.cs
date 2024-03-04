using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ActiveToggleAtText : MonoBehaviour
{
    private List<Toggle> toggles = new List<Toggle>(); 
    public TextMeshProUGUI textMeshPro;

    private void Start()
    {
        for (int i = 0; i < gameObject.transform.childCount; i++)
        {
            GameObject childObject = gameObject.transform.GetChild(i).gameObject;

            Toggle toggle = childObject.GetComponent<Toggle>();

            if (toggle != null)
            {
                toggles.Add(toggle);
            }
        }
        // ������������� ������
        UpdateToggleCount();

        // ������������� �� ������� ��������� ��������� �������
        for (int i = 0; i < toggles.Count; i++)
        {
            toggles[i].onValueChanged.AddListener((value) => UpdateToggleCount());
        }
    }

    private void UpdateToggleCount()
    {
        // ������������ ���������� ���������� �������
        int enabledToggles = 0;
        for (int i = 0; i < toggles.Count; i++)
        {
            if (toggles[i].isOn)
            {
                enabledToggles++;
            }
        }

        // ���������� ���������� ���������� ������� � ��������� ����
        textMeshPro.text = $"{enabledToggles}";
    }
}
