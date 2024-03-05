using System;
using Players.Abilities.Test_Player;
using UnityEngine;

public class TestStunAbilityEvents : MonoBehaviour, ICharacterEvents
{
    public delegate void TestStunAbilityHandler(float value);

    public event TestStunAbilityHandler TestStunAbilityEvent;
    public event Action<float> AbilitiesEvent;

    private void OnEnable()
    {
        GetComponent<StunCast>().FirstAbilityEvent += HandleEvent;
    }

    private void OnDisable()
    {
        GetComponent<StunCast>().FirstAbilityEvent -= HandleEvent;
    }

    private void HandleEvent(float value)
    {
        TestStunAbilityEvent?.Invoke(value);
        AbilitiesEvent?.Invoke(value);
    }
}