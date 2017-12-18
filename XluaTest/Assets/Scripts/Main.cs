using UnityEngine;
using System;
using System.Collections;
using XLua;

public class Main : MonoBehaviour {
    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        LuaManager.I.Start();
        GameManager.I.Start();
    }

    void Update() 
    {
        GameManager.I.Update();
        LuaManager.I.Update();
    }

    void OnDestroy()
    {
        GameManager.I.Release();
        LuaManager.I.Release();
    }
}
