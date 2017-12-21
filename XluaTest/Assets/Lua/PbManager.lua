protobuf = require 'protobuf'
function RegisterPbs(pbPaths)
    for i = 0, pbPaths.Length - 1 do
        local p = pbPaths[i]
        if string.len(p) > 0 then
            protobuf.register(CS.UnityEngine.Resources.Load(p).bytes)
        end
    end
    --protobuf.register(CS.UnityEngine.Resources.Load('proto/tcpproto/commstruct.pb').bytes)
    --protobuf.register(CS.UnityEngine.Resources.Load('proto/tcpproto/lobby.pb').bytes)
    --protobuf.register(CS.UnityEngine.Resources.Load('proto/tcpproto/srviner.pb').bytes)
end

function EncodeBuffer(type,data)
    --序列化
    local encode = protobuf.encode(type, data)
    -- 反序列化
    local user_decode = protobuf.decode(type, encode)
    return user_decode
end