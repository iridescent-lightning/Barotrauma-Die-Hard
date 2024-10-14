
--[[local function lerp(a,b,t)
	return a* (1- t) + b * t
end

Hook.Patch("Barotrauma.Character", "ControlLocalPlayer", function(instance, ptable)
	local character = instance
	
	
	if not character then return end
	
	if PlayerInput.SecondaryMouseButtonHeld() then
	Screen.Selected.Cam.OffsetAmount = lerp(Screen.Selected.Cam.OffsetAmount, 500, 0.5)
	end
end, Hook.HookMethodType.After)--]]


