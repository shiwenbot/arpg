using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
public class MouseManager : Singleton<MouseManager>
{   
    //Cursor texture
    public Texture2D point, doorway, attack, target, arrow;
    RaycastHit hitInfo;
    public event Action<Vector3> OnMouseClicked;//������ҪVector3
    public event Action<GameObject> OnEnemyClicked;

    protected override void Awake() {
        base.Awake();
        //DontDestroyOnLoad(this);
    }
    
    private void Update()
    {
        SetCursorTexture();
        MouseControl();
    }

    //�����ߴ�������ͬ�����ʱ�����ָ��ı仯
    void SetCursorTexture()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if(Physics.Raycast(ray, out hitInfo))
        {
            //�л������ͼ
            switch (hitInfo.collider.gameObject.tag)
            {
                case "Ground":
                    Cursor.SetCursor(target, new Vector2(16, 16), CursorMode.ForceSoftware  );
                    break;
                case "Enemy":
                    Cursor.SetCursor(attack, new Vector2(16, 16), CursorMode.ForceSoftware);
                    break;
            }
        }
    }

    void MouseControl()
    {
        //0���������������������ͼ��᷵�ؿ�ֵ
        if(Input.GetMouseButtonDown(0) && hitInfo.collider != null)
        {
            if (hitInfo.collider.gameObject.CompareTag("Ground"))
                OnMouseClicked?.Invoke(hitInfo.point);//����ע�ᵽOnMouseClicked�ķ������ᱻִ��
            if (hitInfo.collider.gameObject.CompareTag("Enemy"))
                OnEnemyClicked?.Invoke(hitInfo.collider.gameObject);
            if (hitInfo.collider.gameObject.CompareTag("Attackable"))
                OnEnemyClicked?.Invoke(hitInfo.collider.gameObject);
        }
    }
}