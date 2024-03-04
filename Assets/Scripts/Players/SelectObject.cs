using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using UnityEngine;

public class SelectObject : MonoBehaviour
{
    [HideInInspector] public GameObject SelectedObject;
    [HideInInspector] public bool CanSelect = true;
    public GameObject FirstPlayer;
    public GameObject SecondPlayer;
    public GameObject ThirstPlayer;

    private void Start()
    {
        if(SelectedObject != null)
        {
            SelectedObject.layer = LayerMask.NameToLayer("Player");
        }
    }
    private void Update()
    {
        if (CanSelect)
        {
            if(Input.GetKeyDown(KeyCode.F1))
            {
                SelectedObject.layer = LayerMask.NameToLayer("OtherPlayers");

                SelectedObject = FirstPlayer;
                SelectedObject.layer = LayerMask.NameToLayer("Player");
            }
            if (Input.GetKeyDown(KeyCode.F2))
            {
                SelectedObject.layer = LayerMask.NameToLayer("OtherPlayers");

                SelectedObject = SecondPlayer;
                SelectedObject.layer = LayerMask.NameToLayer("Player");

            }
            if (Input.GetKeyDown(KeyCode.F3))
            {
                SelectedObject.layer = LayerMask.NameToLayer("OtherPlayers");

                SelectedObject = ThirstPlayer;
                SelectedObject.layer = LayerMask.NameToLayer("Player");

            }
        }
    }
}
