--[[
local arrestedNotice = "You are put into the prision! 1000 Marks will be deducted from your bank!"
local arrestedNoticeColor = Color(233, 27, 27, 255)

Hook.Add("roundStart","collectspawnpoint",function()
if Level.IsLoadedOutpost then
for _, waypoint in pairs(Level.Loaded.StartOutpost.GetWaypoints(true)) do
	for tag in waypoint.Tags do
		if tag == 'jail' then
			print("insert")
			table.insert(jailSpawnPoints, waypoint)
		end
	end
end
end
end)
Hook.Add("goToPrision", "goto", function(effect, deltaTime, item, targets, worldPosition, user)

    for key, value in pairs(targets) do
        if value.IsHuman and #jailSpawnPoints ~= 0 and value.IsArrested and not jailedCharacter[value.ID] then
			value.TeleportTo(jailSpawnPoints[math.random(1,#jailSpawnPoints)].worldPosition)
			Game.GameSession.Campaign.Bank.Give(-1000)
			jailedCharacter[value.ID] = true
			if SERVER then
                    for _, client in pairs(Client.ClientList) do
                        local chatMessage = ChatMessage.Create("", string.format(arrestedNotice)
                            , ChatMessageType.Default, nil)
                        chatMessage.Color = arrestedNoticeColor

                        Game.SendDirectChatMessage(chatMessage, client)
                    end
                else
                    local chatMessage = ChatMessage.Create("", string.format(arrestedNotice),
                        ChatMessageType.Default, nil)
                    chatMessage.Color = arrestedNoticeColor

                    Game.ChatBox.AddMessage(chatMessage)
                end
		elseif value.IsHuman and #jailSpawnPoints ~= 0 and not value.IsArrested then
			jailedCharacter[value.ID] = nil
		elseif value.IsHuman and #jailSpawnPoints == 0 and value.IsArrested then
		return end 
	end
end)

Hook.Add("roundEnd","removearrested",function()
	if not Level.IsLoadedOutpost then return end 
	if Level.Loaded.StartOutpost then
		for _,v in pairs(Character.CharacterList) do
			if v.IsArrested then
				v.Kill(CauseOfDeathType.Unknown)
			end
		end	
	end
end)
Hook.Add("roundEnd","clearthejailpoint",function()
	jailSpawnPoints = {}
	jailedCharacter = {}
end)--]]



local function RespawnCharacter(character)
	--print("Respawning " .. character.Name .. " as tank")
	Entity.Spawner.AddCharacterToSpawnQueue("Husk", character.WorldPosition, function(newCharacter)
		local client = nil
		for key, value in pairs(Client.ClientList) do
			if value.Character == character then
				client = value
			end
		end
		Entity.Spawner.AddEntityToRemoveQueue(character)
		if client == nil then
			return
		end
		newCharacter.TeamID = character.TeamID
		client.SetClientCharacter(newCharacter)
	end)
end

Hook.Add("convert","convert",function(effect, deltaTime, item, targets, worldPosition)

	RespawnCharacter(effect.user)
	
end)