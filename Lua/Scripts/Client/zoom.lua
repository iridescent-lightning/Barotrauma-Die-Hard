--[[
gzsDefault=false
gzsNew=gzsStart
gzsMin=1.5
gzsMax=1.5
gzsUpd=false


LuaUserData.MakeFieldAccessible(Descriptors["Barotrauma.Camera"],"globalZoomScale")
LuaUserData.MakeMethodAccessible(Descriptors["Barotrauma.Camera"],"CreateMatrices")

Hook.HookMethod("Barotrauma.Camera","CreateMatrices",function(instance,ptable)
	gzsDefault=instance.globalZoomScale
	gzsMin=math.max(0.5,gzsDefault*0.5)
	gzsMax=math.min(1.5,gzsDefault*1.5)
	gzsNew=math.max(math.min(gzsMax,gzsDefault* 1.5),gzsMin)
	gzsUpd=true
end,Hook.HookMethodType.After)
local hasZoom = false
Hook.HookMethod("Barotrauma.Character","ControlLocalPlayer",function(instance,ptable)

		if not Character.DisableControls and Character.Controlled and not hasZoom then
		hasZoom = true
			ptable.cam.globalZoomScale = 1.5
	end
end,Hook.HookMethodType.After)--]]


--[[Hook.Patch("Barotrauma.Character","DrawFront",function(instance,ptable)
--instance.hudInfoHeight = 200
spriteBatch = ptable["spriteBatch"]
test = ptable["iconStyle"]
--instance.Draw(spriteBatch)
print(spriteBatch)
end)--]]

--[[Hook.Patch(
    "Barotrauma.AIObjectiveGetItem",
    ".ctor",
    {
        "Barotrauma.Character",
        "Barotrauma.Identifier",
        "Barotrauma.AIObjectiveManager",
        "System.Boolean",
        "System.Boolean",
        "System.Single",
        "System.Boolean",
    },
    function(instance, ptable)

    local objective = ptable["objectiveManager"].GetActiveObjective()
    if tostring(objective) == "Barotrauma.AIObjectiveFindDivingGear"  then 
        ptable["identifierOrTag"] = "Diving"
    end
end, Hook.HookMethodType.Before)
--]]


LuaUserData.MakeFieldAccessible(Descriptors["Barotrauma.CharacterInventory"], "limbSlotIcons")
local setOuterClothesSprite = false
Hook.Patch("Barotrauma.CharacterInventory","DrawOwn",function(instance,ptable)
	local sprites = CharacterInventory.limbSlotIcons
	if sprites and not setOuterClothesSprite then
	sprites[InvSlotType.OuterClothes] = Sprite(DH.Path.."/UI/dhicon.png", Rectangle(896, 896, 128, 128))
	CharacterInventory.limbSlotIcons = sprites
	setOuterClothesSprite = true
	end
end)


-- client-side
Networking.Receive("flip", function(message)
   local entity = message.ReadUInt16()
   local isFlipped = message.ReadBoolean()

   local submarine = Entity.FindEntityByID(entity)
   -- flip the submarine
   submarine.FlipX()
end)


