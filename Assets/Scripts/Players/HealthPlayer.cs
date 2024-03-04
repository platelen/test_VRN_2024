using System;
using System.Collections.Generic;
using System.Diagnostics;
using TMPro;
using UnityEngine;
using static HealthPlayer;

public class HealthPlayer : MonoBehaviour
{
    public float Health;
    public float MaxHealth;
    public GameObject HealthBar;
    public TextMeshPro HealthBarText;
    public Transform DamageSpawn;
    public TextMeshPro PrefabText;

    public struct DamageInfo
    {
        public float OriginalDamage;
        public float ModifiedDamage;
    }

    public Action<DamageInfo> OnTakePhisicDamage;
    public Action<DamageInfo> OnTakeMagicDamage;

    public delegate void MakeMagicDamageHandler(float value, Type callerType);
    public event MakeMagicDamageHandler MakeMagicDamageEvent;

    public delegate void MakePhisicDamageHandler(float value, Type callerType);
    public event MakePhisicDamageHandler MakePhisicDamageEvent;

    public struct HealInfo
    {
        public float OriginalHeal;
        public float ModifiedHeal;
    }

    public Func<HealInfo, HealInfo> AddHealth;

    private void Start()
    {
        UpdateHealthBar();
    }

    public void TakePhisicDamage(float damageValue)
    {

        HandleAbsorptionOrRepeat(ref damageValue);

        if (damageValue > 0)
        {
            DamageInfo damageInfo;
            damageInfo.OriginalDamage = damageValue;

            damageInfo.ModifiedDamage = damageInfo.OriginalDamage;

            OnTakePhisicDamage?.Invoke(damageInfo);

            float modifiedDamage = damageInfo.ModifiedDamage;
            Health -= modifiedDamage;
            if (Health <= 0)
            {
                Health = 0;
                Die();
            }
            ShowDamagePrefab(-modifiedDamage, new Color(1, 0, 0, 1), new Color(1, 0, 0, 0.5f));
            UpdateHealthBar();
            UpdateHealthBarText();
        }
    }

    public void TakeMagicDamage(float damageValue)
    {
        HandleAbsorptionOrRepeat(ref damageValue);
        if (damageValue > 0)
        {
            DamageInfo damageInfo;
            damageInfo.OriginalDamage = damageValue;

            damageInfo.ModifiedDamage = damageInfo.OriginalDamage;

            OnTakeMagicDamage?.Invoke(damageInfo);

            float modifiedDamage = damageInfo.ModifiedDamage;
            Health -= modifiedDamage;
            if (Health <= 0)
            {
                Health = 0;
                Die();
            }
            ShowDamagePrefab(-modifiedDamage, new Color(1, 0, 0, 1), new Color(1, 0, 0, 0.5f));
            UpdateHealthBar();
            UpdateHealthBarText();
        }
    }

    public void MakePhisicDamage(float damageValue, GameObject target)
    {
        StackTrace stackTrace = new StackTrace();
        StackFrame callerFrame = stackTrace.GetFrame(1);

        // Получаем тип вызывающего объекта
        Type _callerType = callerFrame.GetMethod().DeclaringType;

        target.GetComponent<HealthPlayer>().TakePhisicDamage(damageValue);

        MakePhisicDamageEvent?.Invoke(damageValue, _callerType);
    }

    public void MakeMagicDamage(float damageValue, GameObject target)
    {
        StackTrace stackTrace = new StackTrace();
        StackFrame callerFrame = stackTrace.GetFrame(1);

        // Получаем тип вызывающего объекта
        Type _callerType = callerFrame.GetMethod().DeclaringType;

        target.GetComponent<HealthPlayer>().TakeMagicDamage(damageValue);

        MakeMagicDamageEvent?.Invoke(damageValue, _callerType);
    }

    public void AddHeal(float healValue)
    {
        HealInfo healthInfo;
        healthInfo.OriginalHeal = healValue;

        healthInfo.ModifiedHeal = healthInfo.OriginalHeal;
        if (AddHealth != null)
        {
            healthInfo = AddHealth(healthInfo);
        }

        float modifiedHeal = healthInfo.ModifiedHeal;

        Health += modifiedHeal;
        if (Health >= MaxHealth)
        {
            Health = MaxHealth;
        }
        ShowDamagePrefab(modifiedHeal, new Color(0, 0.8f, 0, 1), new Color(0, 0.8f, 0, 0.5f));
        UpdateHealthBar();
        UpdateHealthBarText();

    }

    private void HandleAbsorptionOrRepeat(ref float modifiedValue)
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);

            DamageAbsorption damageAbsorption = child.GetComponent<DamageAbsorption>();
            if (damageAbsorption != null)
            {
                damageAbsorption.Absorption(ref modifiedValue);
            }

            RepeatedDamage repeatedDamage = child.GetComponent<RepeatedDamage>();
            if (repeatedDamage != null && !repeatedDamage.IsRepeat)
            {
                repeatedDamage.RepeatDamage(ref modifiedValue);
            }
        }
        PsionicaMelee psionicaMelee = GetComponent<PsionicaMelee>();
        if (psionicaMelee != null)
        {
            psionicaMelee.PsionicaAbsorption(ref modifiedValue);

        }
    }

    private void ShowDamagePrefab(float value, Color startColor, Color endColor)
    {
        if(value > 0 && value < 1)
        {
            value = 1;
        }
        value = (int)value;
        PrefabText.text = (value > 0 ? "+" : "") + value.ToString();
        PrefabText.GetComponent<DamagePrefab>().StartColor = startColor;
        PrefabText.GetComponent<DamagePrefab>().EndColor = endColor;
        TextMeshPro newPrefab = Instantiate(PrefabText, DamageSpawn.position, Quaternion.identity);
        newPrefab.transform.SetParent(transform);
    }

    private void UpdateHealthBar()
    {
        float newScaleX = Health / MaxHealth;
        HealthBar.transform.localScale = new Vector3(newScaleX, 1.0f, 1.0f);
    }
    private void UpdateHealthBarText()
    {
        float healthValue = (int)Health;
        HealthBarText.text = healthValue.ToString();
    }
    private void Die()
    {
        // Логика при смерти
    }
}
