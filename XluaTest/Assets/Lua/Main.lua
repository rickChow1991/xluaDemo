require "events"

--主入口函数。从这里开始lua逻辑
function Main() 
  print("hello world")
  local myApp = require("MyApp").new():run()
end

--场景切换通知
function OnLevelWasLoaded(level)
	Time.timeSinceLevelLoad = 0
end

function TestProtobuf(msg)
  print(msg)
  local tmp = string.byte(msg, 1)
  print(tmp)
  local protobuf = require 'protobuf'
  protobuf.register(CS.UnityEngine.Resources.Load('proto/commstruct.pb').bytes)
  protobuf.register(CS.UnityEngine.Resources.Load('proto/lobby.pb').bytes)
  protobuf.register(CS.UnityEngine.Resources.Load('proto/srviner.pb').bytes)
  protobuf.register(CS.UnityEngine.Resources.Load('proto/Person.pb').bytes)

  local playerData = { }
  playerData.userId = 138
  playerData.idseq = 1001
  playerData.nickname = 'flowery'
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

  local person = {}
  person.name = 'flowery'
  person.diamond = 998
  person.level = 99
  
  --序列化
  local encode = protobuf.encode('tcpproto.L2GSReqPlayerJoinCreatedTable', tableInfo)
  --local encode = protobuf.encode('test.Person', person)
  print("encodedata:"..encode)

  -- 反序列化
  local user_decode = protobuf.decode('tcpproto.L2GSReqPlayerJoinCreatedTable', msg)
  --local user_decode = protobuf.decode('test.Person', encode)

  assert(playerData.userId == user_decode.userId)
  --print('hello', user_decode.name)
  print('hello', user_decode.player.nickname)
end