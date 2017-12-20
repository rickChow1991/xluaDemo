using UnityEngine;
using System;
using System.Runtime.InteropServices;

public class GameManager : Singleton<GameManager>
{
    //玩家数据
    public struct PlayerData
    {
        public int userId;     //玩家ID--系统内唯一角色ID
        public int idseq;      //玩家展示ID
        public string nickname;   //昵称
        public string avatar;     //头像
        public int sex;         //性别
        public int gold;       //金币
        public int gem;        //钻石
        public int score;      //积分
        public int roomcard;   //房卡
    }

    //玩家请求加入创建的房间
    public struct L2GSReqPlayerJoinCreatedTable
    {
        //玩家ID
        public int userId;
        //玩家要加入的创建桌子的的room
        public int roomId;
        //玩家要加入的创建桌子的ID
        public int tableId;
        //玩家数据
        public PlayerData player;
    }
    public void Start()
    {
        StartLuaLogic();
    }

    void StartLuaLogic()
    {
        LuaManager.I.luaenv.DoString("require 'Main'");
        Action main = LuaManager.I.luaenv.Global.Get<Action>("Main");
        main();

        var userInfo = new PlayerData();
        userInfo.userId = 1001;
        userInfo.idseq = 100001;
        userInfo.nickname = "Flowery_from_c#";
        userInfo.avatar = "avatar_001";
        userInfo.sex = 0;
        userInfo.gold = 998;
        userInfo.gem = 100;
        userInfo.score = 99;
        userInfo.roomcard = 100;

        var user = new L2GSReqPlayerJoinCreatedTable();
        user.userId = 1001;
        user.roomId = 101;
        user.tableId = 1;
        user.player = userInfo;
        Action<byte[]> testProtobuf = LuaManager.I.luaenv.Global.Get<Action<byte[]>>("TestProtobuf");
        testProtobuf(StrutsToBytesArray(user));
    }

    public static byte[] StrutsToBytesArray(object structObj)
    {
        //得到结构体的大小  
        int size = Marshal.SizeOf(structObj);
        //创建byte数组  
        byte[] bytes = new byte[size];
        //分配结构体大小的内存空间  
        IntPtr structPtr = Marshal.AllocHGlobal(size);
        //将结构体拷到分配好的内存空间  
        Marshal.StructureToPtr(structObj, structPtr, false);
        //从内存空间拷到byte数组  
        Marshal.Copy(structPtr, bytes, 0, size);
        //释放内存空间  
        Marshal.FreeHGlobal(structPtr);
        //返回byte数组  
        return bytes;
    }

    public void Update() { }
    public void Release() { }
}


