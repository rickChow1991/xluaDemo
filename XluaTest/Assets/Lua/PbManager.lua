protobuf = require 'protobuf'
function RegisterPbs()
    protobuf.register(CS.UnityEngine.Resources.Load('proto/commstruct.pb').bytes)
    protobuf.register(CS.UnityEngine.Resources.Load('proto/lobby.pb').bytes)
    protobuf.register(CS.UnityEngine.Resources.Load('proto/srviner.pb').bytes)
end

function EncodeBuffer(type,data)
    --序列化
    local encode = protobuf.encode(type, data)
    -- 反序列化
    local user_decode = protobuf.decode(type, encode)
    return user_decode
end