if SERVER then return end


local OuterHolsterKey=Keys.Q 
local OuterHolsterKeySeconde = Keys.LeftAlt


local foundonce2 = false
Hook.Add("keyUpdate", "holsterunholsterOuter", function (keyargs)

    if not (PlayerInput.KeyDown(OuterHolsterKey) and PlayerInput.KeyDown(OuterHolsterKeySeconde)) then foundonce2 = false return end
    if foundonce2 == true then return end
    if Character.DisableControls then return end
    if Character.Controlled == nil or Character.Controlled.Inventory == nil then return end

    local CharacterInv = Character.Controlled.Inventory

    local ItemInHand = CharacterInv.GetItemAt(5)
    local ItemInBelt = CharacterInv.GetItemInLimbSlot(InvSlotType.OuterClothes)
    local HandSlotIndex = CharacterInv.FindLimbSlot(InvSlotType.RightHand)
    local BagSlotIndex = CharacterInv.FindLimbSlot(InvSlotType.OuterClothes)

    if ItemInHand then
      if not Character.Controlled.Inventory.TryPutItem(ItemInHand, BagSlotIndex, 
         true, false, Character.Controlled, true, true) then goto point2 end
    
    goto point1 end
    
    ::point2::
    if ItemInBelt then
    Character.Controlled.Inventory.TryPutItem(ItemInBelt, HandSlotIndex, 
    true, false, Character.Controlled, true, true) goto point1 end
    
    ::point1::
    foundonce2 = true

end)

local OuterHolsterKey=Keys.Q 
local foundonce = false
Hook.Add("keyUpdate", "holsterunholsterBag", function (keyargs)

    if not PlayerInput.KeyDown(OuterHolsterKey) 
		then foundonce = false 
	return end
    if foundonce == true then return end
    if Character.DisableControls then return end
    if Character.Controlled == nil or Character.Controlled.Inventory == nil then return end

    local CharacterInv = Character.Controlled.Inventory

    local ItemInHand = CharacterInv.GetItemAt(5)
    local ItemInBelt = CharacterInv.GetItemInLimbSlot(InvSlotType.Bag)
    local HandSlotIndex = CharacterInv.FindLimbSlot(InvSlotType.RightHand)
    local BagSlotIndex = CharacterInv.FindLimbSlot(InvSlotType.Bag)

    if ItemInHand then
      if not Character.Controlled.Inventory.TryPutItem(ItemInHand, BagSlotIndex, 
         true, false, Character.Controlled, true, true) then goto point2 end
    
    goto point1 end
    
    ::point2::
    if ItemInBelt then
    Character.Controlled.Inventory.TryPutItem(ItemInBelt, HandSlotIndex, 
    true, false, Character.Controlled, true, true) goto point1 end
    
    ::point1::
    foundonce = true

end)