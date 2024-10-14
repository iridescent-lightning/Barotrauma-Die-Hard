Hook.Add("revolver.OnUse", "DH.weaponrecoil", function(item, usingCharacter, targetCharacter, limb)
	print("use")
--[[local crouchPrefab = AfflictionPrefab.Prefabs["crouch"]
	local crouchPrefab = AfflictionPrefab.Prefabs["recoil"]
	
        for key, value in pairs(usingCharacter) do
		
		local limb = value.AnimController.GetLimb(LimbType.Torso)
		
			if value.IsHuman and value.AnimController.IsAiming and not value.AnimController.Crouching then
			
			print("standaim")
			value.CharacterHealth.ApplyAffliction(limb, crouchPrefab.Instantiate(0.5))
			
			elseif value.IsHuman and value.AnimController.Crouching and value.AnimController.IsAiming then
			
			print("crouching")
			value.CharacterHealth.ApplyAffliction(limb, crouchPrefab.Instantiate(3))
			
			elseif value.IsHuman and value.CurrentSpeed > 1.8 and not value.AnimController.Crouching then
			value.CharacterHealth.ApplyAffliction(limb, crouchPrefab.Instantiate(0))
			
			print("moving")
			end
		end--]]
			


end)