﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using XLua;
using System;

[System.Serializable]
public class Injection
{
    public string name;
    public GameObject value;
}

[LuaCallCSharp]
public class LuaBehaviour : MonoBehaviour
{
    public string LuaFilePath;
    public Injection[] injections;

    private Action<LuaTable> luaStart;
    private Action<LuaTable> luaUpdate;
    private Action<LuaTable> luaOnDestroy;

    private LuaTable _luaInstance;
    private static LuaCustomDelegate.StringReturnTable creatorFunc;

    public LuaTable luaInstance
    {
        get
        { return _luaInstance; }
    }

    void Awake()
    {
        if (creatorFunc == null)
        {
            // function in lua
            creatorFunc = XLuaComponent.instance.luaEnv.Global.Get<LuaCustomDelegate.StringReturnTable>("NewLuaInstanceByPath");
        }

        _luaInstance = creatorFunc(LuaFilePath);
        //_luaInstance.Set("self", this);
        //_luaInstance.Set("MonoBehaviour", this);
        _luaInstance.Set("gameObject", gameObject);
        _luaInstance.Set("transform", transform);

        foreach (var injection in injections)
        {
            _luaInstance.Set(injection.name, injection.value);
        }

        Action<LuaTable> luaAwake = _luaInstance.Get<Action<LuaTable>>("Awake");
        _luaInstance.Get("Start", out luaStart);
        _luaInstance.Get("Update", out luaUpdate);
        _luaInstance.Get("OnDestroy", out luaOnDestroy);

        if (luaAwake != null)
        {
            luaAwake(luaInstance);
        }
    }

    void Start()
    {
        if (luaStart != null)
        {
            luaStart(luaInstance);
        }
    }

    void Update()
    {
        if (luaUpdate != null)
        {
            luaUpdate(luaInstance);
        }
    }

    void OnDestroy()
    {
        if (luaOnDestroy != null)
        {
            luaOnDestroy(luaInstance);
        }
        luaOnDestroy = null;
        luaUpdate = null;
        luaStart = null;
        _luaInstance.Dispose();
        injections = null;
    }
}