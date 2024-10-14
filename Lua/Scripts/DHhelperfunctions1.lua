
-- This file contains a bunch of useful functions that see heavy use in the other scripts.


-- Mechtrauma exclusive functions:

-- none yet!


DH.HF = {} -- Helperfunctions (using HF instead of DH.HF might conflict with neurotraumas and mechtrauma use of the term)

-- most of these are redundant if neurotrauma is running,
-- because neurotrauma already has a set of helperfunctions defined.
-- users might want to run mechtrauma without neurotrauma, so we do it here regardless.

-- general mathy functions:





------------------------

function DH.HF.Lerp(a, b, t)
	return a + (b - a) * t
end

function DH.HF.Round(num, numDecimalPlaces)
    local mult = 10^(numDecimalPlaces or 0)
    return math.floor(num * mult + 0.5) / mult
end

function DH.HF.Clamp(num, min, max)
    if(num<min) then
        num = min 
    elseif(num>max) then 
        num = max 
    end
    return num
end

-- returns num if num > min, else defaultvalue
function DH.HF.Minimum(num, min, defaultvalue)
    if(num<min) then num=(defaultvalue or 0) end
    return num
end

-- /// affliction magic ///
-- i'm sure you'll have some use for these
------------------------------
function DH.HF.GetAfflictionStrength(character,identifier,defaultvalue)
    if character==nil or character.CharacterHealth==nil then return defaultvalue end

    local aff = character.CharacterHealth.GetAffliction(identifier)
    local res = defaultvalue or 0
    if(aff~=nil) then
        res = aff.Strength
    end
    return res
end

function DH.HF.GetAfflictionStrengthLimb(character,limbtype,identifier,defaultvalue)
    if character==nil or character.CharacterHealth==nil or character.AnimController==nil then return defaultvalue end
    local limb = character.AnimController.GetLimb(limbtype)
    if limb==nil then return defaultvalue end
    
    local aff = character.CharacterHealth.GetAffliction(identifier,limb)
    local res = defaultvalue or 0
    if(aff~=nil) then
        res = aff.Strength
    end
    return res
end

function DH.HF.HasAffliction(character,identifier,minamount)
    if character==nil or character.CharacterHealth==nil then return false end

    local aff = character.CharacterHealth.GetAffliction(identifier)
    local res = false
    if(aff~=nil) then
        res = aff.Strength >= (minamount or 0.5)
    end
    return res
end

function DH.HF.HasAfflictionLimb(character,identifier,limbtype,minamount)
    local limb = character.AnimController.GetLimb(limbtype)
    if limb==nil then return false end
    local aff = character.CharacterHealth.GetAffliction(identifier,limb)
    local res = false
    if(aff~=nil) then
        res = aff.Strength >= (minamount or 0.5)
    end
    return res
end

function DH.HF.SetAffliction(character,identifier,strength,aggressor,prevstrength)
    DH.HF.SetAfflictionLimb(character,identifier,LimbType.Torso,strength,aggressor,prevstrength)
end

-- the main "mess with afflictions" function
function DH.HF.SetAfflictionLimb(character,identifier,limbtype,strength,aggressor,prevstrength)
    local prefab = AfflictionPrefab.Prefabs[identifier]
    local resistance = character.CharacterHealth.GetResistance(prefab)
    if resistance >= 1 then return end

    local strength = strength*character.CharacterHealth.MaxVitality/100/(1-resistance)
    local affliction = prefab.Instantiate(
        strength
        ,aggressor)

    character.CharacterHealth.ApplyAffliction(character.AnimController.GetLimb(limbtype),affliction,false)

    -- turn target aggressive if damaging
--    if(aggressor ~= nil and character~=aggressor) then 
--        if prevstrength == nil then prevstrength = 0 end
--
--        local dmg = affliction.GetVitalityDecrease(character.CharacterHealth,strength-prevstrength)
--
--        if (dmg ~= nil and dmg > 0) then
--            MakeAggressive(aggressor,character,dmg)
--        end
--    end

    
end


function DH.HF.AddAffliction(character,identifier,strength,aggressor)
    local prevstrength = DH.HF.GetAfflictionStrength(character,identifier,0)
    DH.HF.SetAffliction(character,identifier,
    strength+prevstrength,
    aggressor,prevstrength)
end

function DH.HF.AddAfflictionResisted(character,identifier,strength,aggressor)
    local prevstrength = DH.HF.GetAfflictionStrength(character,identifier,0)
    strength = strength * (1-DH.HF.GetResistance(character,identifier))
    DH.HF.SetAffliction(character,identifier,
    strength+prevstrength,
    aggressor,prevstrength)
end

function DH.HF.GetResistance(character,identifier)
    local prefab = AfflictionPrefab.Prefabs[identifier]
    if character == nil or character.CharacterHealth == nil or prefab==nil then return 0 end
    return character.CharacterHealth.GetResistance(prefab)
end

-- /// misc ///

function PrintChat(msg)
    if SERVER then
        -- use server method
        Game.SendMessage(msg, ChatMessageType.Server) 
    else
        -- use client method
        Game.ChatBox.AddMessage(ChatMessage.Create("", msg, ChatMessageType.Server, nil))
    end
    
end

function DH.HF.DMClient(client,msg,color)
    if SERVER then
        if(client==nil) then return end

        local chatMessage = ChatMessage.Create("", msg, ChatMessageType.Server, nil)
        if(color~=nil) then chatMessage.Color = color end
        Game.SendDirectChatMessage(chatMessage, client)
    else
        PrintChat(msg)
    end
end

function DH.HF.Chance(chance)
    return math.random() < chance
end

function DH.HF.BoolToNum(val,trueoutput)
    if(val) then return trueoutput or 1 end
    return 0
end

function DH.HF.GetSkillLevel(character,skilltype)
    return character.GetSkillLevel(Identifier(skilltype))
end

function DH.HF.GetBaseSkillLevel(character,skilltype)
    if character == nil or character.Info == nil or character.Info.Job == nil then return 0 end
    return character.Info.Job.GetSkillLevel(Identifier(skilltype))
end

function DH.HF.GetSkillRequirementMet(character,skilltype,requiredamount)
    local skilllevel = DH.HF.GetSkillLevel(character,skilltype)
    return DH.HF.Chance(DH.HF.Clamp(skilllevel/requiredamount,0,1))
end

function DH.HF.GiveSkill(character,skilltype,amount)
    if character ~= nil and character.Info ~= nil then
        character.Info.IncreaseSkillLevel(Identifier(skilltype), amount)
    end
end

function DH.HF.GiveItem(character,identifier)
    if SERVER then
        -- use server spawn method
        Game.SpawnItem(identifier,character.WorldPosition,true,character)
    else
        -- use client spawn method
        character.Inventory.TryPutItem(Item(ItemPrefab.GetItemPrefab(identifier), character.WorldPosition), nil, {InvSlotType.Any})
    end
end

function DH.HF.GiveItemAtCondition(character,identifier,condition)
    if SERVER then
        -- use server spawn method
        local prefab = ItemPrefab.GetItemPrefab(identifier)
        Entity.Spawner.AddItemToSpawnQueue(prefab, character.WorldPosition, nil, nil, function(item)
            item.Condition = condition
            character.Inventory.TryPutItem(item, nil, {InvSlotType.Any})
        end)
    else
        -- use client spawn method
        local item = Item(ItemPrefab.GetItemPrefab(identifier), character.WorldPosition)
        item.Condition = condition
        character.Inventory.TryPutItem(item, nil, {InvSlotType.Any})
    end
end

-- for use with items
function DH.HF.SpawnItemPlusFunction(identifier,func,params,inventory,targetslot,position)
    local prefab = ItemPrefab.GetItemPrefab(identifier)
    if params == nil then params = {} end
    
    if SERVER then
        Entity.Spawner.AddItemToSpawnQueue(prefab, position or inventory.Container.Item.WorldPosition, nil, nil, function(newitem)
            if inventory~=nil then
                inventory.TryPutItem(newitem, targetslot,true,true,nil)
            end
            params["item"]=newitem
            if func ~= nil then func(params) end
        end)
    else
        local newitem = Item(prefab, position or inventory.Container.Item.WorldPosition)
        if inventory~=nil then
            inventory.TryPutItem(newitem, targetslot,true,true,nil)
        end
        params["item"]=newitem
        if func ~= nil then func(params) end
    end
end

-- for use with characters
function DH.HF.GiveItemPlusFunction(identifier,func,params,character)
    local prefab = ItemPrefab.GetItemPrefab(identifier)
    if params == nil then params = {} end
    
    if SERVER then
        Entity.Spawner.AddItemToSpawnQueue(prefab, character.WorldPosition, nil, nil, function(newitem)
            if character.Inventory~=nil then
                character.Inventory.TryPutItem(newitem, nil, {InvSlotType.Any})
            end
            params["item"]=newitem
            func(params)
        end)
    else
        local newitem = Item(prefab, character.WorldPosition)
        if character.Inventory~=nil then
            character.Inventory.TryPutItem(newitem, nil, {InvSlotType.Any})
        end
        params["item"]=newitem
        func(params)
    end
end

function DH.HF.SpawnItemAt(identifier,position)
    if SERVER then
        -- use server spawn method
        Game.SpawnItem(identifier,position,false,nil)
    else
        -- use client spawn method
        Item(ItemPrefab.GetItemPrefab(identifier), position)
    end
end

function DH.HF.RemoveItem(item)
    if item == nil or item.Removed then return end
    
    if SERVER then
        -- use server remove method
        Entity.Spawner.AddEntityToRemoveQueue(item)
    else
        -- use client remove method
        item.Remove()
    end
end

function DH.HF.StartsWith(String,Start)
    return string.sub(String,1,string.len(Start))==Start
end

function DH.HF.SplitString (inputstr, sep)
    if sep == nil then
            sep = "%s"
    end
    local t={}
    for str in string.gmatch(inputstr, "([^"..sep.."]+)") do
            table.insert(t, str)
    end
    return t
end

function DH.HF.HasAbilityFlag(character,flagtype)
    return character.HasAbilityFlag(flagtype)
end

function DH.HF.CharacterToClient(character)

    if not SERVER then return nil end

    for key,client in pairs(Client.ClientList) do
        if client.Character == character then 
            return client
        end
    end

    return nil
end

function DH.HF.ClientFromName(name)

    if not SERVER then return nil end

    for key,client in pairs(Client.ClientList) do
        if client.Name == name then 
            return client
        end
    end

    return nil
end

function DH.HF.GameIsPaused()
    if SERVER then return false end

    return Game.Paused
end

function DH.HF.TableContains(table, value)
    for i, v in ipairs(table) do
        if v == value then
            return true
        end
    end

    return false
end

function DH.HF.PutItemInsideItem(container,identifier,index)
    if index==nil then index = 0 end

    local inv = container.OwnInventory
    if inv == nil then return end

    local previtem = inv.GetItemAt(index)
    if previtem ~= nil then
        inv.ForceRemoveFromSlot(previtem, index)
        previtem.Drop()
    end

    Timer.Wait(function() 
        if SERVER then
            -- use server spawn method
            local prefab = ItemPrefab.GetItemPrefab(identifier)
            Entity.Spawner.AddItemToSpawnQueue(prefab, container.WorldPosition, nil, nil, function(item)
                inv.TryPutItem(item, nil, {index}, true, true)
            end)
        else
            -- use client spawn method
            local item = Item(ItemPrefab.GetItemPrefab(identifier), container.WorldPosition)
            inv.TryPutItem(item, nil, {index}, true, true)
        end
    end,
    10)
end

-- returns an unrounded random number
function DH.HF.RandomRange(min,max) 
    return min+math.random()*(max-min)
end

function DH.HF.HasTalent(character,talentidentifier) 

    local talents = character.Info.UnlockedTalents

    for value in talents do
        if value.Value == talentidentifier then return true end
    end

    return false
end

function DH.HF.CharacterDistance(char1,char2) 
    return DH.HF.Distance(char1.WorldPosition,char2.WorldPosition)
end

function DH.HF.Distance(v1,v2)
    return Vector2.Distance(v1,v2)
end

function DH.HF.GetOuterWearIdentifier(character) 
    return DH.HF.GetCharacterInventorySlotIdentifer(character,4)
end
function DH.HF.GetInnerWearIdentifier(character) 
    return DH.HF.GetCharacterInventorySlotIdentifer(character,3)
end
function DH.HF.GetHeadWearIdentifier(character) 
    return DH.HF.GetCharacterInventorySlotIdentifer(character,2)
end

function DH.HF.GetCharacterInventorySlotIdentifer(character,slot) 
    local item = character.Inventory.GetItemAt(slot)
    if item==nil then return nil end
    return item.Prefab.Identifier.Value
end

function DH.HF.GetItemInRightHand(character) 
    return DH.HF.GetCharacterInventorySlot(character,6)
end
function DH.HF.GetItemInLeftHand(character) 
    return DH.HF.GetCharacterInventorySlot(character,5)
end
function DH.HF.GetOuterWear(character) 
    return DH.HF.GetCharacterInventorySlot(character,4)
end
function DH.HF.GetInnerWear(character) 
    return DH.HF.GetCharacterInventorySlot(character,3)
end
function DH.HF.GetHeadWear(character) 
    return DH.HF.GetCharacterInventorySlot(character,2)
end

function DH.HF.GetCharacterInventorySlot(character,slot) 
    return character.Inventory.GetItemAt(slot)
end

function DH.HF.ItemHasTag(item,tag)
    if item==nil then return false end
    return item.HasTag(tag)
end

function DH.HF.CombineArrays(arr1,arr2)
    local res = {}
    for _,v in ipairs(arr1) do
        table.insert(res, v) end
    for _,v in ipairs(arr2) do
        table.insert(res, v) end
    return res
end

function DH.HF.SendTextBox(header,msg,client)
    if SERVER then
        Game.SendDirectChatMessage(header, msg, nil, 7, client)
    else
        GUI.MessageBox(header, msg)
    end
end

function DH.HF.ReplaceString(original,find,replace)
    return string.gsub(original,find,replace)
end

function DH.HF.Explode(entity,range,force,damage,structureDamage,itemDamage,empStrength,ballastFloraStrength)
    
    range = range or 0
    force = force or 0
    damage = damage or 0
    structureDamage = structureDamage or 0
    itemDamage = itemDamage or 0
    empStrength = empStrength or 0
    ballastFloraStrength = ballastFloraStrength or 0

    local explosion = Explosion(range, force, damage, structureDamage,
        itemDamage, empStrength, ballastFloraStrength)
    explosion.Explode(entity.WorldPosition, nil);

    DH.HF.SpawnItemAt("ntvfx_explosion",entity.WorldPosition)
end