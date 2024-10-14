--obsolete. moved to c#
--[[Hook.Add("character.applyDamage", "bodyarmor.ApplyDamage", function (characterHealth, attackResult, hitLimb)
    if hitLimb.type ~= LimbType.Torso then return end

    local character = characterHealth.Character
    if character.Inventory == nil then return end
    local armor = character.Inventory.GetItemInLimbSlot(InvSlotType.OuterClothes)
    if armor == nil or armor.Prefab.Identifier ~= "bodyarmor" then return end

    local damage = 10
    for affliction in attackResult.Afflictions do
        if affliction.Identifier == "Armormarker" then
			armor.Condition = armor.Condition - damage
            break
        end
    end

    
    if armor.Condition <= 0 then
        Entity.Spawner.AddEntityToRemoveQueue(armor)
    end
end)



Hook.Add("character.applyDamage", "bodyarmorII.ApplyDamage", function (characterHealth, attackResult, hitLimb)
    if hitLimb.type ~= LimbType.Torso then return end

    local character = characterHealth.Character
    if character.Inventory == nil then return end
    local armor = character.Inventory.GetItemInLimbSlot(InvSlotType.OuterClothes)

    if armor == nil or armor.Prefab.Identifier ~= "bodyarmorII" then return end


    local damage = 15
    for affliction in attackResult.Afflictions do
        if affliction.Identifier == "Armormarker" then
            armor.Condition = armor.Condition - damage
            break
        end
    end


    if armor.Condition > 0 then
        return true
    else
        Entity.Spawner.AddEntityToRemoveQueue(armor)
    end
end)

Hook.Add("character.applyDamage", "bodyarmorIII.ApplyDamage", function (characterHealth, attackResult, hitLimb)
    if hitLimb.type ~= LimbType.Torso then return end

    local character = characterHealth.Character
    if character.Inventory == nil then return end
    local armor = character.Inventory.GetItemInLimbSlot(InvSlotType.OuterClothes)

    if armor == nil or armor.Prefab.Identifier ~= "bodyarmorIII" then return end

    local damage = 20
    for affliction in attackResult.Afflictions do
        if affliction.Identifier == "Armormarker" then
            armor.Condition = armor.Condition - damage
            break
        end
    end

    if armor.Condition > 0 then
        return true
    else
        Entity.Spawner.AddEntityToRemoveQueue(armor)
    end
end)

Hook.Add("character.applyDamage", "ballistichelmet1.ApplyDamage", function (characterHealth, attackResult, hitLimb)
    if hitLimb.type ~= LimbType.Head then return end

    local character = characterHealth.Character
    if character.Inventory == nil then return end
    local armor = character.Inventory.GetItemInLimbSlot(InvSlotType.Head)

    if armor == nil or armor.Prefab.Identifier ~= "ballistichelmet1" then return end

    local damage = 20
    for affliction in attackResult.Afflictions do
        if affliction.Identifier == "Armormarker" then
            armor.Condition = armor.Condition - damage
            break
        end
    end

    if armor.Condition > 0 then
        return true
    else
        Entity.Spawner.AddEntityToRemoveQueue(armor)
    end
end)

Hook.Add("character.applyDamage", "ballistichelmet2.ApplyDamage", function (characterHealth, attackResult, hitLimb)
    if hitLimb.type ~= LimbType.Head then return end

    local character = characterHealth.Character
    if character.Inventory == nil then return end
    local armor = character.Inventory.GetItemInLimbSlot(InvSlotType.Head)

    if armor == nil or armor.Prefab.Identifier ~= "ballistichelmet2" then return end

    local damage = 30
    for affliction in attackResult.Afflictions do
        if affliction.Identifier == "Armormarker" then
            armor.Condition = armor.Condition - damage
            break
        end
    end

    if armor.Condition > 0 then
        return true
    else
        Entity.Spawner.AddEntityToRemoveQueue(armor)
    end
end)

Hook.Add("character.applyDamage", "ballistichelmet3.ApplyDamage", function (characterHealth, attackResult, hitLimb)
    if hitLimb.type ~= LimbType.Head then return end

    local character = characterHealth.Character
    if character.Inventory == nil then return end
    local armor = character.Inventory.GetItemInLimbSlot(InvSlotType.Head)

    if armor == nil or armor.Prefab.Identifier ~= "ballistichelmet3" then return end

    local damage = 40
    for affliction in attackResult.Afflictions do
        if affliction.Identifier == "Armormarker" then
            armor.Condition = armor.Condition - damage
            break
        end
    end

    if armor.Condition > 0 then
        return true
    else
        Entity.Spawner.AddEntityToRemoveQueue(armor)
    end
end)





Hook.Add("character.applyDamage", "divingsuit.ApplyDamage", function (characterHealth, attackResult, hitLimb)
    

    local character = characterHealth.Character
    if character.Inventory == nil then return end
    local armor = character.Inventory.GetItemInLimbSlot(InvSlotType.InnerClothes)
    if armor == nil then return end
	if armor.Prefab.Identifier == "divingsuit" or armor.Prefab.Identifier == "abyssdivingsuit" or armor.Prefab.Identifier == "combatdivingsuit" or armor.Prefab.Identifier == "respawndivingsuit" then
	
		local damage = 5
		for affliction in attackResult.Afflictions do
			if affliction.Identifier == "lacerations" or affliction.Identifier == "gunshotwound" or affliction.Identifier == "bitwounds"  then
			armor.Condition = armor.Condition - damage - affliction.Strength
				break
			end
		end

    
end
end)


Hook.Add("character.applyDamage", "cloth.damage", function (characterHealth, attackResult, hitLimb)
    if hitLimb.type ~= LimbType.Torso then return end

    local character = characterHealth.Character
    if character.Inventory == nil then return end
    local armor = character.Inventory.GetItemInLimbSlot(InvSlotType.InnerClothes)
	local plate = character.Inventory.GetItemInLimbSlot(InvSlotType.OuterClothes)
    if armor == nil then return end
	if armor.HasTag("clothing") and not plate then
	
		local damage = 4
		for affliction in attackResult.Afflictions do
			if affliction.Identifier == "lacerations" or affliction.Identifier == "gunshotwound" or affliction.Identifier == "bitwounds" then
			armor.Condition = armor.Condition - damage
				break
			end
		end

    
    if armor.Condition <= 0 then
        Entity.Spawner.AddEntityToRemoveQueue(armor)
    end
end
end)--]]



