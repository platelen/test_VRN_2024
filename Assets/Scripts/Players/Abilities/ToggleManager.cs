using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ToggleManager : MonoBehaviour
{
    public List<AbilityBase> Abilities;
    private AbilityBase lastAbility;

    void Start()
    {
        foreach (AbilityBase ability in Abilities)
        {
            ability.ToggleAbility.onValueChanged.AddListener(delegate
            {
                OnToggleValueChanged(ability);
            });
        }
    }
    private void Update()
    {
        bool allTogglesOff = Abilities.All(ability => !ability.ToggleAbility.isOn);

        if (lastAbility != null && allTogglesOff)
        {
            // ��� ToggleAbility ���������, ������� ������������� lastAbility � null
            lastAbility = null;
        }
    }

    void OnToggleValueChanged(AbilityBase ability)
    {
        if (ability.ToggleAbility.isOn)
        {
            // ���� ������� ������� �����, �� � ���� ��� ��������� ���� (target)
            // �������� ��� � ������ ��������� ���������� �����

            if (lastAbility == null)
            {
                lastAbility = ability;
                return;
            }
            else if (lastAbility != null && lastAbility.TargetParent == null)
            {
                lastAbility.ToggleAbility.isOn = false;
                lastAbility = ability;
            }
            else if (lastAbility != null && lastAbility.TargetParent != null)
            {
                lastAbility = ability;
            }
        }
    }
}

