LuaUserData.MakeFieldAccessible(Descriptors["Barotrauma.Items.Components.Repairable"], "skillTextContainer")

Hook.Patch("Barotrauma.Items.Components.Repairable", "CreateGUI", function(instance, ptable)
	if instance.Item.HasTag("gun") then
   GUI.TextBlock(GUI.RectTransform(Vector2(1, 0), instance.skillTextContainer.RectTransform), "Needs weapon parts")
	elseif instance.Item.HasTag("pumpmotor") then
		GUI.TextBlock(GUI.RectTransform(Vector2(1, 0), instance.skillTextContainer.RectTransform), "Needs mechanical parts")
   end
end, Hook.HookMethodType.After)