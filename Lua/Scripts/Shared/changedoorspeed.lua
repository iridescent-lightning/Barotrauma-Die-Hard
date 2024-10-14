LuaUserData.MakeMethodAccessible(Descriptors["Barotrauma.Items.Components.Door"], "set_ClosingSpeed")
LuaUserData.MakeMethodAccessible(Descriptors["Barotrauma.Items.Components.Door"], "set_OpeningSpeed")
--[[Hook.Add("roundStart", "wdoorspeed_doorwithbuttononpenspeed", function()
	for _, item in pairs(Item.ItemList) do
        if item.HasTag('windoweddoorwbuttons') or item.HasTag('doorwbuttons') then
			local theComponent = item.GetComponentString('Door')
			if theComponent then
			theComponent.set_OpeningSpeed(0.8)
			theComponent.set_ClosingSpeed(0.8)
			end
		end
	end
end)--]]
