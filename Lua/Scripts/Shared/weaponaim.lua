
Hook.Add("RevolverAim", "RevolverAim", function(effect, deltaTime, item, targets, worldPosition)
	local crouchPrefab = AfflictionPrefab.Prefabs["crouch"]

	if effect.user then
		local character = effect.user
		local limb = character.AnimController.GetLimb(LimbType.Torso)
		
		if character.AnimController.IsAiming and not character.AnimController.Crouching and character.CurrentSpeed <= 1.5 then
			
			--print("standaim")
			character.CharacterHealth.ApplyAffliction(limb, crouchPrefab.Instantiate(2))
			
			
		elseif character.AnimController.Crouching and character.AnimController.IsAiming then
			
			--print("crouchaim")
			character.CharacterHealth.ApplyAffliction(limb, crouchPrefab.Instantiate(3))
			
		elseif character.CurrentSpeed > 1.5 and character.CurrentSpeed < 1.8 then
			character.CharacterHealth.ApplyAffliction(limb, crouchPrefab.Instantiate(-1))
			
			--print("walkaim")
			
		elseif character.AnimController.IsMovingFast then
			character.CharacterHealth.ApplyAffliction(limb, crouchPrefab.Instantiate(-60))
			
			--print("runaim")
			

		end
	end		
end)

Hook.Add("57Aim", "57Aim", function(effect, deltaTime, item, targets, worldPosition)
	local crouchPrefab = AfflictionPrefab.Prefabs["crouch"]

	if effect.user then
		local character = effect.user
		local limb = character.AnimController.GetLimb(LimbType.Torso)
		
		if character.AnimController.IsAiming and not character.AnimController.Crouching and character.CurrentSpeed <= 1.5 then
			
			--print("standaim")
			character.CharacterHealth.ApplyAffliction(limb, crouchPrefab.Instantiate(2.1))
			
			
		elseif character.AnimController.Crouching and character.AnimController.IsAiming then
			
			--print("crouchaim")
			character.CharacterHealth.ApplyAffliction(limb, crouchPrefab.Instantiate(2.9))
			
		elseif character.CurrentSpeed > 1.5 and character.CurrentSpeed < 1.8 then
			character.CharacterHealth.ApplyAffliction(limb, crouchPrefab.Instantiate(-1))
			
			--print("walkaim")
			
		elseif character.AnimController.IsMovingFast then
			character.CharacterHealth.ApplyAffliction(limb, crouchPrefab.Instantiate(-60))
			
			--print("runaim")
			

		end
	end		
end)

Hook.Add("G19Aim", "G19Aim", function(effect, deltaTime, item, targets, worldPosition)
	local crouchPrefab = AfflictionPrefab.Prefabs["crouch"]

	if effect.user then
		local character = effect.user
		local limb = character.AnimController.GetLimb(LimbType.Torso)
		
		if character.AnimController.IsAiming and not character.AnimController.Crouching and character.CurrentSpeed <= 1.5 then
			
			--print("standaim")
			character.CharacterHealth.ApplyAffliction(limb, crouchPrefab.Instantiate(2.2))
			
			
		elseif character.AnimController.Crouching and character.AnimController.IsAiming then
			
			--print("crouchaim")
			character.CharacterHealth.ApplyAffliction(limb, crouchPrefab.Instantiate(3.0))
			
		elseif character.CurrentSpeed > 1.5 and character.CurrentSpeed < 1.8 then
			character.CharacterHealth.ApplyAffliction(limb, crouchPrefab.Instantiate(-1))
			
			--print("walkaim")
			
		elseif character.AnimController.IsMovingFast then
			character.CharacterHealth.ApplyAffliction(limb, crouchPrefab.Instantiate(-60))
			
			--print("runaim")
			

		end
	end		
end)

Hook.Add("G19Aim", "G19Aim", function(effect, deltaTime, item, targets, worldPosition)
	local crouchPrefab = AfflictionPrefab.Prefabs["crouch"]

	if effect.user then
		local character = effect.user
		local limb = character.AnimController.GetLimb(LimbType.Torso)
		
		if character.AnimController.IsAiming and not character.AnimController.Crouching and character.CurrentSpeed <= 1.5 then
			
			--print("standaim")
			character.CharacterHealth.ApplyAffliction(limb, crouchPrefab.Instantiate(2.2))
			
			
		elseif character.AnimController.Crouching and character.AnimController.IsAiming then
			
			--print("crouchaim")
			character.CharacterHealth.ApplyAffliction(limb, crouchPrefab.Instantiate(3.0))
			
		elseif character.CurrentSpeed > 1.5 and character.CurrentSpeed < 1.8 then
			character.CharacterHealth.ApplyAffliction(limb, crouchPrefab.Instantiate(-1))
			
			--print("walkaim")
			
		elseif character.AnimController.IsMovingFast then
			character.CharacterHealth.ApplyAffliction(limb, crouchPrefab.Instantiate(-60))
			
			--print("runaim")
			

		end
	end		
end)

Hook.Add("MPXAim", "MPXAim", function(effect, deltaTime, item, targets, worldPosition)
	local crouchPrefab = AfflictionPrefab.Prefabs["crouch"]

	if effect.user then
		local character = effect.user
		local limb = character.AnimController.GetLimb(LimbType.Torso)
		
		if character.AnimController.IsAiming and not character.AnimController.Crouching and character.CurrentSpeed <= 1.5 then
			
			--print("standaim")
			character.CharacterHealth.ApplyAffliction(limb, crouchPrefab.Instantiate(2.5))
			
			
		elseif character.AnimController.Crouching and character.AnimController.IsAiming then
			
			--print("crouchaim")
			character.CharacterHealth.ApplyAffliction(limb, crouchPrefab.Instantiate(2.5))
			
		elseif character.CurrentSpeed > 1.5 and character.CurrentSpeed < 1.8 then
			character.CharacterHealth.ApplyAffliction(limb, crouchPrefab.Instantiate(-0.5))
			
			--print("walkaim")
			
		elseif character.AnimController.IsMovingFast then
			character.CharacterHealth.ApplyAffliction(limb, crouchPrefab.Instantiate(-60))
			
			--print("runaim")
			

		end
	end		
end)

Hook.Add("MCXAim", "MCXAim", function(effect, deltaTime, item, targets, worldPosition)
	local crouchPrefab = AfflictionPrefab.Prefabs["CrouchLua"]

		for character in targets do
		local limb = character.AnimController.GetLimb(LimbType.Torso)
		
		if character.AnimController.IsAiming and not character.AnimController.Crouching and character.CurrentSpeed <= 1.5 then
			
			--print("standaim")
			character.CharacterHealth.ApplyAffliction(limb, crouchPrefab.Instantiate(0.9))
			
			
		elseif character.AnimController.Crouching and character.AnimController.IsAiming then
			
			--print("crouchaim")
			character.CharacterHealth.ApplyAffliction(limb, crouchPrefab.Instantiate(3.1))
			
		elseif character.CurrentSpeed > 1.5 and character.CurrentSpeed < 1.8 then
			character.CharacterHealth.ApplyAffliction(limb, crouchPrefab.Instantiate(-1))
			
			--print("walkaim")
			
		elseif character.AnimController.IsMovingFast then
			character.CharacterHealth.ApplyAffliction(limb, crouchPrefab.Instantiate(-60))
			
			--print("runaim")
			

		end
	end		
end)

Hook.Add("MP9Aim", "MP9Aim", function(effect, deltaTime, item, targets, worldPosition)
	local crouchPrefab = AfflictionPrefab.Prefabs["crouch"]

	if effect.user then
		local character = effect.user
		local limb = character.AnimController.GetLimb(LimbType.Torso)
		
		if character.AnimController.IsAiming and not character.AnimController.Crouching and character.CurrentSpeed <= 1.5 then
			
			--print("standaim")
			character.CharacterHealth.ApplyAffliction(limb, crouchPrefab.Instantiate(3.2))
			
			
		elseif character.AnimController.Crouching and character.AnimController.IsAiming then
			
			--print("crouchaim")
			character.CharacterHealth.ApplyAffliction(limb, crouchPrefab.Instantiate(3.7))
			
		elseif character.CurrentSpeed > 1.5 and character.CurrentSpeed < 1.8 then
			character.CharacterHealth.ApplyAffliction(limb, crouchPrefab.Instantiate(-0.1))
			
			--print("walkaim")
			
		elseif character.AnimController.IsMovingFast then
			character.CharacterHealth.ApplyAffliction(limb, crouchPrefab.Instantiate(-60))
			
			--print("runaim")
			

		end
	end		
end)

Hook.Add("MP9Aim", "MP9Aim", function(effect, deltaTime, item, targets, worldPosition)
	local crouchPrefab = AfflictionPrefab.Prefabs["crouch"]

	if effect.user then
		local character = effect.user
		local limb = character.AnimController.GetLimb(LimbType.Torso)
		
		if character.AnimController.IsAiming and not character.AnimController.Crouching and character.CurrentSpeed <= 1.5 then
			
			--print("standaim")
			character.CharacterHealth.ApplyAffliction(limb, crouchPrefab.Instantiate(3.2))
			
			
		elseif character.AnimController.Crouching and character.AnimController.IsAiming then
			
			--print("crouchaim")
			character.CharacterHealth.ApplyAffliction(limb, crouchPrefab.Instantiate(3.7))
			
		elseif character.CurrentSpeed > 1.5 and character.CurrentSpeed < 1.8 then
			character.CharacterHealth.ApplyAffliction(limb, crouchPrefab.Instantiate(-0.1))
			
			--print("walkaim")
			
		elseif character.AnimController.IsMovingFast then
			character.CharacterHealth.ApplyAffliction(limb, crouchPrefab.Instantiate(-60))
			
			--print("runaim")
			

		end
	end		
end)

Hook.Add("P90Aim", "P90Aim", function(effect, deltaTime, item, targets, worldPosition)
	local crouchPrefab = AfflictionPrefab.Prefabs["crouch"]

	if effect.user then
		local character = effect.user
		local limb = character.AnimController.GetLimb(LimbType.Torso)
		
		if character.AnimController.IsAiming and not character.AnimController.Crouching and character.CurrentSpeed <= 1.5 then
			
			--print("standaim")
			character.CharacterHealth.ApplyAffliction(limb, crouchPrefab.Instantiate(2.4))
			
			
		elseif character.AnimController.Crouching and character.AnimController.IsAiming then
			
			--print("crouchaim")
			character.CharacterHealth.ApplyAffliction(limb, crouchPrefab.Instantiate(2.9))
			
		elseif character.CurrentSpeed > 1.5 and character.CurrentSpeed < 1.8 then
			character.CharacterHealth.ApplyAffliction(limb, crouchPrefab.Instantiate(-0.1))
			
			--print("walkaim")
			
		elseif character.AnimController.IsMovingFast then
			character.CharacterHealth.ApplyAffliction(limb, crouchPrefab.Instantiate(-60))
			
			--print("runaim")
			

		end
	end		
end)

Hook.Add("P90Aim", "P90Aim", function(effect, deltaTime, item, targets, worldPosition)
	local crouchPrefab = AfflictionPrefab.Prefabs["crouch"]

	if effect.user then
		local character = effect.user
		local limb = character.AnimController.GetLimb(LimbType.Torso)
		
		if character.AnimController.IsAiming and not character.AnimController.Crouching and character.CurrentSpeed <= 1.5 then
			
			--print("standaim")
			character.CharacterHealth.ApplyAffliction(limb, crouchPrefab.Instantiate(2.4))
			
			
		elseif character.AnimController.Crouching and character.AnimController.IsAiming then
			
			--print("crouchaim")
			character.CharacterHealth.ApplyAffliction(limb, crouchPrefab.Instantiate(2.9))
			
		elseif character.CurrentSpeed > 1.5 and character.CurrentSpeed < 1.8 then
			character.CharacterHealth.ApplyAffliction(limb, crouchPrefab.Instantiate(-0.1))
			
			--print("walkaim")
			
		elseif character.AnimController.IsMovingFast then
			character.CharacterHealth.ApplyAffliction(limb, crouchPrefab.Instantiate(-60))
			
			--print("runaim")
			

		end
	end		
end)

Hook.Add("ScarAim", "ScarAim", function(effect, deltaTime, item, targets, worldPosition)
	local crouchPrefab = AfflictionPrefab.Prefabs["crouch"]

	if effect.user then
		local character = effect.user
		local limb = character.AnimController.GetLimb(LimbType.Torso)
		
		if character.AnimController.IsAiming and not character.AnimController.Crouching and character.CurrentSpeed <= 1.5 then
			
			--print("standaim")
			character.CharacterHealth.ApplyAffliction(limb, crouchPrefab.Instantiate(3.0))
			
			
		elseif character.AnimController.Crouching and character.AnimController.IsAiming then
			
			--print("crouchaim")
			character.CharacterHealth.ApplyAffliction(limb, crouchPrefab.Instantiate(3.5))
			
		elseif character.CurrentSpeed > 1.5 and character.CurrentSpeed < 1.8 then
			character.CharacterHealth.ApplyAffliction(limb, crouchPrefab.Instantiate(-1))
			
			--print("walkaim")
			
		elseif character.AnimController.IsMovingFast then
			character.CharacterHealth.ApplyAffliction(limb, crouchPrefab.Instantiate(-60))
			
			--print("runaim")
			

		end
	end		
end)

Hook.Add("ScarAim", "ScarAim", function(effect, deltaTime, item, targets, worldPosition)
	local crouchPrefab = AfflictionPrefab.Prefabs["crouch"]

	if effect.user then
		local character = effect.user
		local limb = character.AnimController.GetLimb(LimbType.Torso)
		
		if character.AnimController.IsAiming and not character.AnimController.Crouching and character.CurrentSpeed <= 1.5 then
			
			--print("standaim")
			character.CharacterHealth.ApplyAffliction(limb, crouchPrefab.Instantiate(3.0))
			
			
		elseif character.AnimController.Crouching and character.AnimController.IsAiming then
			
			--print("crouchaim")
			character.CharacterHealth.ApplyAffliction(limb, crouchPrefab.Instantiate(3.5))
			
		elseif character.CurrentSpeed > 1.5 and character.CurrentSpeed < 1.8 then
			character.CharacterHealth.ApplyAffliction(limb, crouchPrefab.Instantiate(-1))
			
			--print("walkaim")
			
		elseif character.AnimController.IsMovingFast then
			character.CharacterHealth.ApplyAffliction(limb, crouchPrefab.Instantiate(-60))
			
			--print("runaim")
			

		end
	end		
end)

Hook.Add("AKAim", "AKAim", function(effect, deltaTime, item, targets, worldPosition)

	for character in targets do

		
		if character.AnimController.IsAiming and not character.AnimController.Crouching and character.CurrentSpeed <= 1.5 then
			
			--print("standaim")
			character.ChangeStat(StatTypes.RangedSpreadReduction, 0.01)
			
			
		elseif character.AnimController.Crouching and character.AnimController.IsAiming then
			
			--print("crouchaim")
			character.ChangeStat(StatTypes.RangedSpreadReduction, 0.05)
			
		elseif character.CurrentSpeed > 1.5 and character.CurrentSpeed < 1.8 then
			character.ChangeStat(StatTypes.RangedSpreadReduction, -1)
			
			--print("walkaim")
			
		elseif character.AnimController.IsMovingFast then
			character.ChangeStat(StatTypes.RangedSpreadReduction, -10)
			
			--print("runaim")
			

		end
	if character.GetStatValue(StatTypes.RangedSpreadReduction, true) < 0 then 
			character.Info.ClearSavedStatValues(StatTypes.RangedSpreadReduction)
		end
		 print(character.GetStatValue(StatTypes.RangedSpreadReduction, true))
	end		
end)

Hook.Add("PKMAim", "PKMAim", function(effect, deltaTime, item, targets, worldPosition)
	local crouchPrefab = AfflictionPrefab.Prefabs["crouch"]

	if effect.user then
		local character = effect.user
		local limb = character.AnimController.GetLimb(LimbType.Torso)
		
		if character.AnimController.IsAiming and not character.AnimController.Crouching and character.CurrentSpeed <= 1.5 then
			
			--print("standaim")
			character.CharacterHealth.ApplyAffliction(limb, crouchPrefab.Instantiate(2.5))
			
			
		elseif character.AnimController.Crouching and character.AnimController.IsAiming then
			
			--print("crouchaim")
			character.CharacterHealth.ApplyAffliction(limb, crouchPrefab.Instantiate(3.4))
			
		elseif character.CurrentSpeed > 1.5 and character.CurrentSpeed < 1.8 then
			character.CharacterHealth.ApplyAffliction(limb, crouchPrefab.Instantiate(-10))
			
			--print("walkaim")
			
		elseif character.AnimController.IsMovingFast then
			character.CharacterHealth.ApplyAffliction(limb, crouchPrefab.Instantiate(-60))
			
			--print("runaim")
			

		end
	end		
end)


Hook.Add("FALAim", "FALAim", function(effect, deltaTime, item, targets, worldPosition)
	local crouchPrefab = AfflictionPrefab.Prefabs["crouch"]

	if effect.user then
		local character = effect.user
		local limb = character.AnimController.GetLimb(LimbType.Torso)
		
		if character.AnimController.IsAiming and not character.AnimController.Crouching and character.CurrentSpeed <= 1.5 then
			
			--print("standaim")
			character.CharacterHealth.ApplyAffliction(limb, crouchPrefab.Instantiate(2.5))
			
			
		elseif character.AnimController.Crouching and character.AnimController.IsAiming then
			
			--print("crouchaim")
			character.CharacterHealth.ApplyAffliction(limb, crouchPrefab.Instantiate(2.7))
			
		elseif character.CurrentSpeed > 1.5 and character.CurrentSpeed < 1.8 then
			character.CharacterHealth.ApplyAffliction(limb, crouchPrefab.Instantiate(-10))
			
			--print("walkaim")
			
		elseif character.AnimController.IsMovingFast then
			character.CharacterHealth.ApplyAffliction(limb, crouchPrefab.Instantiate(-60))
			
			--print("runaim")
			

		end
	end		
end)

Hook.Add("APSAim", "APSAim", function(effect, deltaTime, item, targets, worldPosition)
	local crouchPrefab = AfflictionPrefab.Prefabs["crouch"]

	if effect.user then
		local character = effect.user
		local limb = character.AnimController.GetLimb(LimbType.Torso)
		
		if character.AnimController.IsAiming and not character.AnimController.Crouching and character.CurrentSpeed <= 1.5 then
			
			--print("standaim")
			character.CharacterHealth.ApplyAffliction(limb, crouchPrefab.Instantiate(3.1))
			
			
		elseif character.AnimController.Crouching and character.AnimController.IsAiming then
			
			--print("crouchaim")
			character.CharacterHealth.ApplyAffliction(limb, crouchPrefab.Instantiate(3.7))
			
		elseif character.CurrentSpeed > 1.5 and character.CurrentSpeed < 1.8 then
			character.CharacterHealth.ApplyAffliction(limb, crouchPrefab.Instantiate(-10))
			
			--print("walkaim")
			
		elseif character.AnimController.IsMovingFast then
			character.CharacterHealth.ApplyAffliction(limb, crouchPrefab.Instantiate(-60))
			
			--print("runaim")
			

		end
	end		
end)