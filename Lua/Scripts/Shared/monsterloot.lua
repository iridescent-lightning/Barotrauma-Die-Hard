
	




--[[Hook.Add("Barotrauma.Item", "Interact", function(item, characterPicker, ignoreRequiredItemsBool, forceSelectKeyBool, forceActionKeyBool)
	if Item.Interact then
	print("working")


end)--]]


--[[Hook.Add("Terminal.OnUse", "powerConsumptionReport", function(effect, deltaTime, item, targets,worldPosition, client)
 
  local terminal = item.GetComponentString("Terminal")
  local hull
  local totalPowerConsumption = 0
  --print(item.GetComponent.Powered())
  
	--if CentralComputerOnline then
		
		terminal.ShowMessage = "-"
	for k, item in pairs(Item.ItemList) do   
		if item.FindHull() ~= nil then 
			hull = item.FindHull().DisplayName.Value 
		else 
			hull = "EXTERIOR"  
		end 
		
		if item.GetComponentString("Powered") ~= nil and item.GetComponentString("Powered").CurrPowerConsumption >0.5 then
				totalPowerConsumption = totalPowerConsumption + item.GetComponentString("Powered").CurrPowerConsumption           
				terminal.ShowMessage = "[Power: " .. item.GetComponentString("Powered").CurrPowerConsumption .. "| Fixture: " .. item.name .. "Hull: " .. hull .. "]"      
		end 
		
	
		terminal.ShowMessage = "Estimated Power Consumption:" .. totalPowerConsumption
	end
	--else
    --terminal.ShowMessage = "**************NO CONNECTION**************"
	--end

	if SERVER then
    terminal.SyncHistory()
	end

end)--]]

