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
    public event Action<Vector3> OnMouseClicked;//坐标需要Vector3
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

    //在射线触碰到不同物体的时候会有指针的变化
    void SetCursorTexture()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if(Physics.Raycast(ray, out hitInfo))
        {
            //切换鼠标贴图
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
        //0代表鼠标左键，当鼠标点击地图外会返回空值
        if(Input.GetMouseButtonDown(0) && hitInfo.collider != null)
        {
            if (hitInfo.collider.gameObject.CompareTag("Ground"))
                OnMouseClicked?.Invoke(hitInfo.point);//所有注册到OnMouseClicked的方法都会被执行
            if (hitInfo.collider.gameObject.CompareTag("Enemy"))
                OnEnemyClicked?.Invoke(hitInfo.collider.gameObject);
            if (hitInfo.collider.gameObject.CompareTag("Attackable"))
                OnEnemyClicked?.Invoke(hitInfo.collider.gameObject);
        }
    }
}