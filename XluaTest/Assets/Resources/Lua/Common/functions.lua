
--增加监听--
--function AddClick(btn, func)
--  listener = UIEventListener.Get(btn)
--  if listener.onClick then
--      listener.onClick = listener.onClick + func                      
--  else
--      listener.onClick = func
--  end                
--end

--function RemoveClick(btn, func)
--  listener = UIEventListener.Get(btn)
--  if listener.onClick then
--      listener.onClick = listener.onClick - func      
--  else
--      print('empty delegate')
--  end
--end

----输出日志--
function log(str)
  print(str);
end

----错误日志--
function logError(str) 
  print(str);
end

----警告日志--
function logWarn(str) 
	print(str);
end

----查找对象--
--function find(str)
--	return GameObject.Find(str);
--end

--function destroy(obj)
--	GameObject.Destroy(obj);
--end

--function newObject(prefab)
--	return GameObject.Instantiate(prefab);
--end

----创建面板--
--function createPanel(name)
--	panelMgr:CreatePanel(name);
--end

--function child(str)
--	return transform:FindChild(str);
--end

--function subGet(childNode, typeName)		
--	return child(childNode):GetComponent(typeName);
--end

--function findPanel(str) 
--	local obj = find(str);
--	if obj == nil then
--		error(str.." is null");
--		return nil;
--	end
--	return obj:GetComponent("BaseLua");
--end