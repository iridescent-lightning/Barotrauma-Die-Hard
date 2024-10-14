LuaUserData.MakeFieldAccessible(Descriptors["Barotrauma.Character"], "hudInfoVisible")
LuaUserData.MakeFieldAccessible(Descriptors["Barotrauma.AICharacter"], "hudInfoVisible")

Hook.HookMethod("Barotrauma.Character", "UpdateProjSpecific", function (instance)
    if instance.IsHuman or instance.IsHusk then
		if instance.IsUnconscious then
			instance.hudInfoVisible = false
		end
	end
end, Hook.HookMethodType.After)