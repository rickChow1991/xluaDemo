local M = {}
M._moduleName = ...
M.__index = M
----- begin module -----
function M:run()
  --[[
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
  --]]
  self:OpenUI("UIFrame") 
end

function M:OpenUI(SceneName)
  print("ui name:"..SceneName)
  local uiPath = "UI/"..SceneName
  local uiPrefab = CS.UnityEngine.Resources.Load(uiPath);
  local position = CS.UnityEngine.Vector3.zero
  local rotation = CS.UnityEngine.Quaternion.identity
  local parent = CS.UnityEngine.GameObject.Find("Canvas").transform
  local ui = CS.UnityEngine.GameObject.Instantiate(uiPrefab,position,rotation,parent)
  CS.XLuaExtension.LuaInjector.Inject(ui,self)
  self.txt_Label.text = "test"
  self.sc_Content.onGetElementCount = 
  function()
    return 100
  end
  self.sc_Content.onUpdateScrollView = 
  function(idx,tran)
    print("idx:"..idx)
    print("tranname:"..tran.name)
  end
  print("count:"..self.sc_Content.onGetElementCount())
  self.sc_Content:Show()
end

----- end -----
return M