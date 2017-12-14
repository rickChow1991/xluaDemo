using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
/// <summary>
/// 场景加载前的资源加载
/// </summary>
public class LoadMgr : UnitySingleton<LoadMgr>
{
    void Awake() { 
		this.enabled = false; 
	}

    public void ReplaceScene(string sceneName, Action func = null)
    {
        this.enabled = true;
    }

    public Queue<Callback> requestQueue;
    int allIndex;
    int curIndex;
    public Callback curRequest;


    float timer = 1f;

    void Update() 
    {
        ///强制性加点延迟
        if ((timer -= Time.deltaTime) < 0f)
        {
            timer = 1f;

            if (curIndex < allIndex)
            {
                this.curRequest = requestQueue.Dequeue();
                this.curRequest();
                this.curIndex++;
                //UpdateProgree();
            }
            else
            {
                OnProgressFinish();
            }
        }
    }

    ///进度条显示当前的情况
    void OnGUI()
    {
        GUI.color = Color.red;
        GUI.Label(new Rect(10, 10, 1500, 100), "当前比例 " + (float)curIndex / (float)allIndex + "");
    }
    
    Action finish_func;
    void OnProgressFinish() 
    {
        Debug.Log(" OnProgressFinish ");
		finish_func();
        finish_func = null;
        this.enabled = false;
    }

    void OnDestroy()
    {
        if (finish_func != null) finish_func = null;
    }
}
