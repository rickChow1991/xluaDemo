require("PbManager")
local M = {}
M._moduleName = ...
M.__index = M
----- begin module -----
local myApp = require("MyApp")
--主入口函数。从这里开始lua逻辑
function Main()
  myApp:run()
end

--场景切换通知
function OnLevelWasLoaded(level)
	Time.timeSinceLevelLoad = 0
end

function TestProtobuf(msg)
  --local result = msg:split(";")  --分隔字符串
  RegisterPbs(msg)

  local playerData = { }
  playerData.userId = 138
  playerData.idseq = 1001
  playerData.nickname = 'abc'
  playerData.avatar = 'avatar'
  playerData.sex = 0
  playerData.gold = 998
  playerData.gem = 889
  playerData.score = 799
  playerData.roomcard = 699

  local tableInfo = {}
  tableInfo.userId = 138
  tableInfo.roomId = 1
  tableInfo.tableId = 2
  tableInfo.player = playerData

  local user_decode = EncodeBuffer('tcpproto.L2GSReqPlayerJoinCreatedTable',tableInfo)
  print('hello', user_decode.player.nickname)
end
--------------------------- Lua字符串分隔方法 -----------------------------------------  
--字符串分隔方法  
function string:split(sep)  
  local sep, fields = sep or ":", {}  
  local pattern = string.format("([^%s]+)", sep)  
  self:gsub(pattern, function (c) fields[#fields + 1] = c end)  
  return fields  
end  

----- end -----
return M