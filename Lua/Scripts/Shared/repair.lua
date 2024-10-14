

LuaUserData.MakeFieldAccessible(Descriptors["Barotrauma.Items.Components.Repairable"], "currentRepairItem")
Hook.Patch("Barotrauma.Items.Components.Repairable", "StartRepairing", function (instance, ptable)
	if not instance or instance.CurrentFixer == nil then return end
	
	local stunPrefab = AfflictionPrefab.Prefabs["stun"]
	local fixer = instance.CurrentFixer
	local repairTool = instance.currentRepairItem
	local selectedItem = instance.CurrentFixer.SelectedItem
	local randomDamage = math.random(1,10)
	local randomDamageForConsumables = math.random(40,90)
	
	repairTool.Condition = repairTool.Condition - randomDamage

	if selectedItem and selectedItem.HasTag("gun") then
		if not fixer.Inventory.FindItemByIdentifier("weaponparts") then
			fixer.SetStun(0.1)
		elseif fixer.Inventory.FindItemByIdentifier("weaponparts") then
			fixer.Inventory.FindItemByIdentifier("weaponparts").Condition = fixer.Inventory.FindItemByIdentifier("weaponparts").Condition - randomDamageForConsumables
		end
	elseif selectedItem and selectedItem.HasTag("pumpmotor") then
		if not fixer.Inventory.FindItemByIdentifier("mechanicalparts") then
			fixer.SetStun(0.1)
		elseif fixer.Inventory.FindItemByIdentifier("mechanicalparts") then
			fixer.Inventory.FindItemByIdentifier("mechanicalparts").Condition = fixer.Inventory.FindItemByIdentifier("mechanicalparts").Condition - randomDamageForConsumables
			--Entity.Spawner.AddEntityToRemoveQueue(fixer.Inventory.FindItemByIdentifier("mechanicalparts")) use xml to remove itself
		end
	elseif selectedItem and selectedItem.HasTag("RequireWireToFix") then --for now use mechanical parts to fix everything
		if not fixer.Inventory.FindItemByTag("mechanicalparts") then
			fixer.SetStun(0.1)
		elseif fixer.Inventory.FindItemByTag("mechanicalparts") then
			fixer.Inventory.FindItemByTag("mechanicalparts").Condition = fixer.Inventory.FindItemByTag("mechanicalparts").Condition - randomDamageForConsumables
			
		end
	end
	
end, Hook.HookMethodType.After)


