--[[local UpdateCooldown = 0
local UpdateInterval = 120
local UpdateDelta = UpdateInterval / 60 

Hook.Add("Think","DH.Update",function()

	if DH.HF.GameIsPaused() then 
	return 
	end

	UpdateCooldown = UpdateCooldown - 1
	if (UpdateCooldown <= 0) and not DH.HF.GameIsPaused() then 
		UpdateCooldown = UpdateInterval
		updates()
	end
end)

function updates()
	local characterToUpdate = {}
	for key, v in pairs(Character.CharacterList) do
		if not v.IsDead then
			table.insert(characterToUpdate, v)
		end
	end


	local amountCharacter = #characterToUpdate
	for i,value in pairs(characterToUpdate) do 
		 if (value ~= nil and not value.Removed and value.IsHuman and not value.IsDead) then
		Timer.Wait(function()
			if (value ~= nil and not value.Removed and value.IsHuman and not value.IsDead) then
			DHupdate(value) end
			end,((i + 1) / amountCharacter) * UpdateDelta * 1000)
		end
	end

end


		
function DHupdate(instance)
	--DH.BorrowTime(instance)
	RemoveMonsterLoot(instance)
end

function DH.Coldwater(character)
	if not character.IsHuman then return end
	if character.InWater then 
		local coldwaterPrefab = AfflictionPrefab.Prefabs["coldwater"]
		local limb = character.AnimController.GetLimb(LimbType.Torso)
		character.CharacterHealth.ApplyAffliction(limb, coldwaterPrefab.Instantiate(1.9))
	end
end

function DH.Fatigue(character)
	if not character.IsHuman then return end
	local fatiguePrefab = AfflictionPrefab.Prefabs["fatigue"]
	local limb = character.AnimController.GetLimb(LimbType.Torso)
	if character.IsHuman and character.CurrentSpeed > 1.8 and not character.InWater and not character.IsClimbing then
	character.CharacterHealth.ApplyAffliction(limb, fatiguePrefab.Instantiate(20))
	end
end
local rollOnce = {}
function DH.BorrowTime(character)
	if not character.IsHuman and character.IsUnconscious then return end
	if rollOnce[character] then return end
	local deathDice = math.random(1000)
	local delayRandom = math.random(8000, 20000)
	local deathCall = math.random(2000,6000)

	if deathDice > 990 and not rollOnce[character] and not character.IsDead and not character.Removed and character.IsUnconscious then
	print("check")
	Timer.Wait(function()
				if not character.IsDead and not character.Removed then
				character.CharacterHealth.ReduceAfflictionOnAllLimbs("gunshotwound", 80)
				character.CharacterHealth.ReduceAfflictionOnAllLimbs("bleeding", 25)
				end
				end, delayRandom) 
		rollOnce[character] = true
		print("borrowtime")
	end
end

--[[Hook.Add("Character.Death", "removeloot", function(character)
	if character.IsHuman or character.IsHusk or character.IsPet then return end--without this the game will lag

		if not character.IsHuman and not character.IsHusk then

				if character.Inventory == nil then return end
				for item in character.Inventory.AllItemsMod do

				Entity.Spawner.AddEntityToRemoveQueue(item)
				--item.Remove() 
				end
		end
end)--]]

--[[function RemoveMonsterLoot(character)
	if character.IsHuman or character.IsHusk or character.IsPet then return end
	if not character.IsHuman and not character.IsHusk then

				if character.Inventory == nil then return end
				for item in character.Inventory.AllItemsMod do

				Entity.Spawner.AddEntityToRemoveQueue(item)
				--item.Remove() 
				end
		end
end--]]



--[[Hook.Patch("Barotrauma.Character", "UpdateDespawn", function (instance, ptable)
if not instance.IsHuman and instance.IsDead then 
return end

    if instance.IsHuman and  then
		return true
	else 
		return end

end)--]]


--now function moved to c# characterHealth
--[[Hook.Add("character.applyDamage", "forcedrop", function (characterHealth, attackResult, hitLimb)
	
	local character = characterHealth.Character
	
	if character.Inventory == nil then return end
	if not character.IsHuman then return end
	
	if hitLimb.type == LimbType.LeftArm or hitLimb.type == LimbType.RightArm then 

	
		local rightHand = characterHealth.Character.Inventory.GetItemInLimbSlot(InvSlotType.RightHand)
		local leftHand = characterHealth.Character.Inventory.GetItemInLimbSlot(InvSlotType.LeftHand)
	
    for affliction in attackResult.Afflictions do
        if affliction.Identifier == "blunttrauma" then
			if rightHand then
			rightHand.Drop(character)
			elseif leftHand then
			leftHand.Drop(character)
			end
		end
    end
end

end)
--now function moved to c# characterHealth
Hook.Add("character.applyDamage", "dropWhenUnconscious", function (characterHealth, attackResult, hitLimb)
	
	local humans = characterHealth.Character
	if not humans.IsHuman or hitLimb.type == nil then return end
	
	
	if humans.IsUnconscious then
	
	local Head = humans.Inventory.GetItemInLimbSlot(InvSlotType.Head)
	local rightHand = humans.Inventory.GetItemInLimbSlot(InvSlotType.RightHand)
    local leftHand = humans.Inventory.GetItemInLimbSlot(InvSlotType.LeftHand)
	local dice = math.random(10)
	
		if rightHand then
				rightHand.Drop()
			elseif leftHand then
				leftHand.Drop()
			elseif Head and dice > 5 and hitLimb.type == LimbType.Head then
				Head.Drop()
			end

	end
end)--]]

--[[Hook.Add("character.applyDamage", "explosionBreakJoints", function (characterHealth, attackResult, hitLimb)
	print("test")
	local character = characterHealth.Character
	if not character.IsHuman or not character.IsDead then return end
	
	for affliction in attackResult.Afflictions do
        if affliction.Identifier == "explosiondamage" then
			character.BreakJoints()
		end
	end
end)--]]
--switch to use LuaHook to reduce impact
--[[Hook.Add("HumanExplosionDeath","HumanExplosionDeath",function(effect, deltaTime, item, targets, worldPosition)
for target in targets do
	local limb = target.AnimController.GetLimb(LimbType.Torso)
	local affliction = target.CharacterHealth.GetAffliction("explosiondamage", limb)
	if affliction and target.IsDead then
	target.BreakJoints()
	end
end

end)--]]
--[[Hook.Add("character.applyDamage", "test", function (characterHealth, attackResult, hitLimb)
	
	local character = characterHealth.Character
print('work')
	local limb = character.AnimController.GetLimb(LimbType.Torso)
	limb.HideAndDisable(3000, true)


end)--]]
--[[Hook.Patch("Barotrauma.PhysicsBody", "ApplyWaterForces", function (instance, ptable)
    print(instance.ApplyWaterForces)
	
end)--]]

--[[Just a short-work solution for now. Perhaps bots will get drown more often. Or some other bugs? Wait for devs to handle the issue. 2023/11/23, this feature is no longer needed. But I have forgoten its logic.--]]
--[[function DH.FindDivingsuitInSlot(character)
local inner = character.Inventory.GetItemInLimbSlot(InvSlotType.InnerClothes)

local head = character.Inventory.GetItemInLimbSlot(InvSlotType.Head)
	if inner  then
	
	if inner.HasTag('deepdiving') then
		local oxygenTank = inner.OwnInventory.GetItemAt(0)
		if oxygenTank and oxygenTank.Condition ~= 0 then
			return true
		end
	end
	return false
	end
end

Hook.Patch("Barotrauma.AIObjectiveFindDivingGear","CheckObjectiveSpecific",function(instance,ptable)
if DH.FindDivingsuitInSlot(instance.character) then

	return true
else

	return false
end
end,Hook.HookMethodType.After)--]]

--[[Hook.Patch("Barotrauma.AIObjectiveFindDivingGear","Act",function(instance,ptable)

print("acting")
end)--]]

--[[LuaUserData.MakeFieldAccessible(Descriptors["Barotrauma.AIObjectiveGoTo"], "getDivingGearIfNeeded")
Hook.Patch("Barotrauma.AIObjectiveGoTo","Act",function(instance,ptable)
if DH.FindDivingsuitInSlot(instance.character) then
instance.getDivingGearIfNeeded = false
else
instance.getDivingGearIfNeeded = true
end
end)
--]]



	
	