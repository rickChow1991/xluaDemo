require "Common/define"
require "Common/functions"
require "framework/init"
class("MyApp")
--- @class MyApp
function MyApp:ctor()
  self.curScene = nil;
end

function MyApp:run()
  --self:enterScene("LoginScene")

  local rapidjson = require 'rapidjson' 
  local t = rapidjson.decode('{"a":123}')
  print(t.a)
  t.a = 456
  local s = rapidjson.encode(t)
  print('json', s)
  ------------------------------------
  local lpeg = require 'lpeg'
  print('lpeg');
  print(lpeg.match(lpeg.R '09','123'))
  ------------------------------------
  local protobuf = require 'protobuf'
  protobuf.register(CS.UnityEngine.Resources.Load('proto/UserInfo.pb').bytes)
  protobuf.register(CS.UnityEngine.Resources.Load('proto/User.pb').bytes)

  local userInfo = {}
  userInfo.name = 'FLOWER'
  userInfo.diamond = 998
  userInfo.level = 100

  local user = { }
  user.id = 1
  user.status = { 1,0,2,4}
  user.pwdMd5 = 'md5'
  user.regTime = '2017-03-29 12:00:00'
  user.info = userInfo

  --序列化
  local encode = protobuf.encode('User', user)

  -- 反序列化
  local user_decode = protobuf.decode('User', encode)

  assert(user.id == user_decode.id and user.info.diamond == user_decode.info.diamond)
  print('hello', user_decode.info.name)

end

function MyApp:enterScene(SceneName)
  print("curScene name:"..SceneName)
  if self.curScene then
    self.curScene:OnExit()
  end

  self:test_pblua_func()

  --self.curScene = require("scenes/"..SceneName).new()
  --CS.UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(1);
  --loadMgr:ReplaceScene(self.curScene.ablist, self:OnLoadFinish())
end

function MyApp:OnLoadFinish()
  self.curScene:OnEnter()
end

--测试pblua--
function MyApp:test_pblua_func()
  local man = Protol.man_pb.Men();
  man.id = 2000;
  man.name = 'game';
  man.email = 'jarjin@163.com';
  
  local msg = man:SerializeToString();
  print(msg)
  self:OnPbluaCall(msg)
end

--pblua callback--
function MyApp:OnPbluaCall(data)
  local msg = Protol.man_pb.Men();
  msg:ParseFromString(data);
  print(msg);
  print(msg.id..' '..msg.name);
end

return MyApp