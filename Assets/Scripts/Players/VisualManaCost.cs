using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisualManaCost : MonoBehaviour
{
    public Transform ManaBarSprite; 

    private void Start()
    {
        gameObject.SetActive(false);
    }

    public void CheckManaCost()
    {
        if (ManaBarSprite != null)
        {
            // �������� ������� �������
            SpriteRenderer spriteRenderer = ManaBarSprite.GetComponent<SpriteRenderer>();
            Bounds spriteBounds = spriteRenderer.bounds;

            // ������������ �������� ����� �� X
            float endX = spriteBounds.max.x;

            // ������������� ������� ������� � ����� ������� ���� �� X
            Vector3 newPosition = new Vector3(endX, transform.position.y, transform.position.z);
            transform.position = newPosition;
        }
    }
}
