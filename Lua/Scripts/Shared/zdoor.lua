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


--now a c# feature
--[[Hook.Add("ElectricalBugVent","ElectricalBugVent",function(effect, deltaTime, item, targets, worldPosition)
	local bugChance = math.random(1000)
	if item.Submarine == Submarine.MainSub then
		if bugChance > 990 then
			Entity.Spawner.AddCharacterToSpawnQueue('Electrical_bug',item.WorldPosition,onSpawn)
		elseif bugChance > 995 then
			Entity.Spawner.AddCharacterToSpawnQueue('Electrical_bug',item.WorldPosition,onSpawn)
			Entity.Spawner.AddCharacterToSpawnQueue('Electrical_bug',item.WorldPosition,onSpawn)
		end
	end
end)--]]


