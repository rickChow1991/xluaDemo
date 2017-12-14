using UnityEngine;
using System;
using System.Collections;
using XLua;

public class GameManager : Singleton<GameManager>
{

    [CSharpCallLua]
    public delegate Action MainAction();

    public void Start()
    {
        StartLuaLogic();
    }

    void StartLuaLogic()
    {
        UnityEngine.Debug.Log(" Start Lua : Logic");
        LuaManager.I.luaenv.DoString("require 'Main'");

        MainAction main = LuaManager.I.luaenv.Global.Get<MainAction>("Main");
        //映射到一个delgate，要求delegate加到生成列表，否则返回null，建议用法
        main();
    }

    public void Update() { }

    public void Release() { }
}


