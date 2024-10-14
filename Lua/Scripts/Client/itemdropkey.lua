Hook.Add("keyUpdate", "itemdropkey", function (keyargs)
    if not PlayerInput.KeyDown(Keys.F) then return end
    if Character.DisableControls then return end
    if Character.Controlled == nil or Character.Controlled.Inventory == nil or Character.Controlled.LockHands then return end

    local rightHand = Character.Controlled.Inventory.GetItemInLimbSlot(InvSlotType.RightHand)
    local leftHand = Character.Controlled.Inventory.GetItemInLimbSlot(InvSlotType.LeftHand)

    if rightHand then
        rightHand.Drop(Character.Controlled)
    end

    if leftHand then
        leftHand.Drop(Character.Controlled)
    end
end)


















