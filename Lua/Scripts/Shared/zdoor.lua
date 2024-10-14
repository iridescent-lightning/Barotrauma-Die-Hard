if CLIENT and Game.IsMultiplayer then return end

local delay = {}

local sounds = {
    enterable_door1 = "sfx_doorsounda",
    enterable_door2 = "sfx_doorsoundb",
}

local function HandleReceive(signal, connection)
    if signal.sender == nil then return end

    local minDelay = 0

    if connection.Item.HasTag("smalldelay") then
        minDelay = 0.2
    elseif connection.Item.HasTag("mediumdelay") then
        minDelay = 0.5
    elseif connection.Item.HasTag("bigdelay") then
        minDelay = 1
    elseif connection.Item.HasTag("hugedelay") then
        minDelay = 2
    end

    if delay[signal.sender] and Timer.GetTime() - delay[signal.sender] < minDelay then
        return
    end

    signal.sender.TeleportTo(connection.Item.WorldPosition)
	
    if signal.sender.SelectedCharacter then
        signal.sender.SelectedCharacter.TeleportTo(connection.Item.WorldPosition)
    end

    local sound = sounds[connection.Item.Prefab.Identifier.Value]
    if sound then
        Entity.Spawner.AddItemToSpawnQueue(ItemPrefab.GetItemPrefab(sound), connection.Item.WorldPosition)
    end

    delay[signal.sender] = Timer.GetTime()
end

Hook.Add("signalReceived.enterable_doora", "ZDoors", HandleReceive)
Hook.Add("signalReceived.enterable_doorb", "ZDoors", HandleReceive)
Hook.Add("signalReceived.enterable_doorc", "ZDoors", HandleReceive)


-- Now use a better link method to teleport
Hook.Add("zdoor", "zdoor", function(effect, deltaTime, item, targets, worldPosition)

    -- Check if targets exist
    if targets == nil or #targets == 0 then return end

    -- Get the first target, which should be the user
    local user = targets[1]

    -- Ensure there are linked items to teleport to
    if item.linkedTo == nil then return end

    -- Loop through linked items
    for _, linkedItem in pairs(item.linkedTo) do
        -- Check if the linked item has a valid WorldPosition
        if linkedItem.WorldPosition then
            -- Teleport the user (target) to the linked item's position
            
            user.TeleportTo(linkedItem.WorldPosition)
            break -- Teleport to the first linked item and stop (optional)
        end
    end
end)


