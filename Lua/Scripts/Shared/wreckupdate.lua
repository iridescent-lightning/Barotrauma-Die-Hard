local function MakeWrecksMovable()
    if Level.Loaded == nil then return end
  
    for key, value in pairs(Level.Loaded.Wrecks) do
        value.PhysicsBody.FarseerBody.BodyType = 2
        value.SetCrushDepth(math.max(Submarine.MainSub.RealWorldCrushDepth - 150, Level.DefaultRealWorldCrushDepth))
    end
  end
  
  Hook.Add("roundStart", "makeWrecksMovable", MakeWrecksMovable)
  
  Hook.HookMethod("Barotrauma.Submarine", "MakeWreck", function(submarine)
     submarine.PhysicsBody.FarseerBody.BodyType = 2
  end, Hook.HookMethodType.After)
  



  if Game.IsMultiplayer and CLIENT then return end

  local sellText = TextManager.Get("SellWreck")
  local sellTextColor = Color(255, 255, 100, 255)
  
  local function FindPriceTag(tags)
      local price = ""
  
      local startPos, endPos = tags:find("price:")
  
      if endPos == nil then return "" end
  
      for i = endPos + 1, #tags, 1 do
          local c = tags:sub(i, i)
  
          if c == "," then break end
          price = price .. c
      end
  
      return price
  end
  
  function GetSubmarinePrice(sub)
      for key, value in pairs(sub.GetItems(false)) do
          local price = FindPriceTag(value.Tags)
          if price ~= "" then
              return tonumber(price)
          end
      end
  
      return sub.CalculateBasePrice()
  end
  
  local wrecksMarkedForSelling = {}
  
  Hook.Add("think", "checkForWrecksSell", function()
      if Level.Loaded == nil then return end
      if Level.Loaded.Wrecks == nil then return end
  
      for key, value in pairs(Level.Loaded.Wrecks) do
          if Level.Loaded.IsCloseToEnd(value.WorldPosition, 6000) then
              if not wrecksMarkedForSelling[value] then
  
                  local price = GetSubmarinePrice(value)
                  local EXPmulti = math.floor(price * (Level.Loaded.Difficulty / 25))
  
                  if SERVER then
                      for _, client in pairs(Client.ClientList) do
                          local chatMessage = ChatMessage.Create("", string.format(sellText.Value, value.Info.Name, price, EXPmulti)
                              , ChatMessageType.Default, nil)
                          chatMessage.Color = sellTextColor
  
                          Game.SendDirectChatMessage(chatMessage, client)
                      end
                  else
                      local chatMessage = ChatMessage.Create("", string.format(sellText.Value, value.Info.Name, price, EXPmulti),
                          ChatMessageType.Default, nil)
                      chatMessage.Color = sellTextColor
  
                      Game.ChatBox.AddMessage(chatMessage)
                  end
  
                  wrecksMarkedForSelling[value] = price
              end
          end
      end
  end)
  
  local function GiveWreckEXP(amount)
      for k, v in pairs(Character.CharacterList) do
          if v.IsHuman and not v.IsDead and v.IsOnPlayerTeam or v.SpeciesName == "Mudraptor_player" then
              v.Info.GiveExperience(amount, true)
          end
      end
  end
  
  Hook.Add("roundEnd", "sellWrecks", function()
      if Game.GameSession.Campaign == nil then return end
  
      local toSell = wrecksMarkedForSelling
  
      Timer.Wait(function()
          for key, value in pairs(toSell) do
              Game.GameSession.Campaign.Bank.Give(value)
          end
      end, 5000)
  
      for key, value in pairs(toSell) do
          local EXPmulti = Level.Loaded.Difficulty / 75
          GiveWreckEXP(value * EXPmulti)
      end
  
      wrecksMarkedForSelling = {}
  end)