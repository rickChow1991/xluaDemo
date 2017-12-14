using UnityEngine;
using System;
using System.Collections;
using XLua;

public class Main : MonoBehaviour {
    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    // Use this for initialization
    void Start()
    {
        LuaManager.I.Start();
        ///here start gamelogic, so need start at last
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
