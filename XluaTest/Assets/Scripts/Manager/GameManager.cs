using UnityEngine;
using System;

public class GameManager : Singleton<GameManager>
{
    public void Start()
    {
        StartLuaLogic();
    }

    void StartLuaLogic()
    {
        LuaManager.I.luaenv.DoString("require 'Main'");
        Action main = LuaManager.I.luaenv.Global.Get<Action>("Main");
        main();
    }

    public void Update() { }
    public void Release() { }
}


