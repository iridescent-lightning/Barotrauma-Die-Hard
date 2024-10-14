
--[[Hook.Patch("Barotrauma.Items.Components.OxygenGenerator","Update",function(instance,ptable)
	local frame = GUI.Frame(GUI.RectTransform(Vector2(0.25, 0.25)), nil)
		frame.CanBeFocused = false
		
	local button = GUI.Button(GUI.RectTransform(Vector2(0.25, 0.25), frame.RectTransform, GUI.Anchor.TopRight), nil, GUI.Alignment.Center, "PowerButton")
	if not hasUpdate and instance.Item.Use then
		frame.AddToGUIUpdateList()
		hasUpdate = true
	end
end)--]]

Hook.Add("OxygenGeneratorButton","OxygenGeneratorButton",function(effect, deltaTime, item, targets, worldPosition)
	local frame = GUI.Frame(GUI.RectTransform(Vector2(0.3, 0.3)), nil)
		frame.CanBeFocused = false
		
	local button = GUI.Button(GUI.RectTransform(Vector2(0.3, 0.3), frame.RectTransform, GUI.Anchor.TopRight), nil, GUI.Alignment.Center, "PowerButton")
	
		frame.AddToGUIUpdateList()

end)