
-- This file contains a bunch of useful functions that see heavy use in the other scripts.


-- Neurotrauma functions

function DH.DislocateLimb(character,limbtype,strength)
    strength = strength or 1
    local limbtoaffliction = {}
    limbtoaffliction[LimbType.RightLeg] = "dislocation1"
    limbtoaffliction[LimbType.LeftLeg] = "dislocation2"
    limbtoaffliction[LimbType.RightArm] = "dislocation3"
    limbtoaffliction[LimbType.LeftArm] = "dislocation4"
    if limbtoaffliction[limbtype] == nil then return end
    HF.AddAffliction(character,limbtoaffliction[limbtype],strength)
end
function DH.BreakLimb(character,limbtype,strength)
    strength = strength or 5
    local limbtoaffliction = {}
    limbtoaffliction[LimbType.RightLeg] = "rl_fracture"
    limbtoaffliction[LimbType.LeftLeg] = "ll_fracture"
    limbtoaffliction[LimbType.RightArm] = "ra_fracture"
    limbtoaffliction[LimbType.LeftArm] = "la_fracture"
    limbtoaffliction[LimbType.Head] = "h_fracture"
    limbtoaffliction[LimbType.Torso] = "t_fracture"
    if limbtoaffliction[limbtype] == nil then return end
    HF.AddAffliction(character,limbtoaffliction[limbtype],strength)

    if strength > 0 and DH.Config.fracturesRemoveCasts then
        HF.SetAfflictionLimb(character,"gypsumcast",limbtype,0)
    end
end
function DH.SurgicallyAmputateLimb(character,limbtype,strength,traumampstrength)
    strength = strength or 100
    traumampstrength = traumampstrength or 0
    
    limbtype = HF.NormalizeLimbType(limbtype)
    local limbtoaffliction = {}
    limbtoaffliction[LimbType.RightLeg] = "srl_amputation"
    limbtoaffliction[LimbType.LeftLeg] = "sll_amputation"
    limbtoaffliction[LimbType.RightArm] = "sra_amputation"
    limbtoaffliction[LimbType.LeftArm] = "sla_amputation"
    limbtoaffliction[LimbType.Head] = "sh_amputation"
    if limbtoaffliction[limbtype] == nil then return end
    HF.SetAffliction(character,limbtoaffliction[limbtype],strength)

    limbtoaffliction[LimbType.RightLeg] = "trl_amputation"
    limbtoaffliction[LimbType.LeftLeg] = "tll_amputation"
    limbtoaffliction[LimbType.RightArm] = "tra_amputation"
    limbtoaffliction[LimbType.LeftArm] = "tla_amputation"
    limbtoaffliction[LimbType.Head] = "th_amputation"
    HF.SetAffliction(character,limbtoaffliction[limbtype],traumampstrength)
end
function DH.TraumamputateLimb(character,limbtype)
    local limbtoaffliction = {}
    limbtoaffliction[LimbType.RightLeg] = "gate_ta_rl"
    limbtoaffliction[LimbType.LeftLeg] = "gate_ta_ll"
    limbtoaffliction[LimbType.RightArm] = "gate_ta_ra"
    limbtoaffliction[LimbType.LeftArm] = "gate_ta_la"
    limbtoaffliction[LimbType.Head] = "gate_ta_h"
    if limbtoaffliction[limbtype] == nil then return end
    HF.AddAfflictionLimb(character,limbtoaffliction[limbtype],limbtype,10)
end
function DH.TraumamputateLimbMinusItem(character,limbtype)
    local limbtoaffliction = {}
    limbtoaffliction[LimbType.RightLeg] = "gate_ta_rl_2"
    limbtoaffliction[LimbType.LeftLeg] = "gate_ta_ll_2"
    limbtoaffliction[LimbType.RightArm] = "gate_ta_ra_2"
    limbtoaffliction[LimbType.LeftArm] = "gate_ta_la_2"
    limbtoaffliction[LimbType.Head] = "gate_ta_h"
    if limbtoaffliction[limbtype] == nil then return end
    HF.AddAfflictionLimb(character,limbtoaffliction[limbtype],limbtype,10)
end
function DH.ArteryCutLimb(character,limbtype,strength)
    strength=strength or 5
    local limbtoaffliction = {}
    limbtoaffliction[LimbType.RightLeg] = "rl_arterialcut"
    limbtoaffliction[LimbType.LeftLeg] = "ll_arterialcut"
    limbtoaffliction[LimbType.RightArm] = "ra_arterialcut"
    limbtoaffliction[LimbType.LeftArm] = "la_arterialcut"
    limbtoaffliction[LimbType.Head] = "h_arterialcut"
    limbtoaffliction[LimbType.Torso] = "t_arterialcut"
    if limbtoaffliction[limbtype] == nil then return end
    HF.AddAffliction(character,limbtoaffliction[limbtype],strength)
end


function DH.LimbIsDislocated(character,limbtype)
    local limbtoaffliction = {}
    limbtoaffliction[LimbType.RightLeg] = "dislocation1"
    limbtoaffliction[LimbType.LeftLeg] = "dislocation2"
    limbtoaffliction[LimbType.RightArm] = "dislocation3"
    limbtoaffliction[LimbType.LeftArm] = "dislocation4"
    if limbtoaffliction[limbtype] == nil then return false end
    return HF.HasAffliction(character,limbtoaffliction[limbtype],1)
end
function DH.LimbIsBroken(character,limbtype)
    local limbtoaffliction = {}
    limbtoaffliction[LimbType.RightLeg] = "rl_fracture"
    limbtoaffliction[LimbType.LeftLeg] = "ll_fracture"
    limbtoaffliction[LimbType.RightArm] = "ra_fracture"
    limbtoaffliction[LimbType.LeftArm] = "la_fracture"
    if limbtoaffliction[limbtype] == nil then return false end
    return HF.HasAffliction(character,limbtoaffliction[limbtype],1)
end
function DH.LimbIsArterialCut(character,limbtype)
    local limbtoaffliction = {}
    limbtoaffliction[LimbType.RightLeg] = "rl_arterialcut"
    limbtoaffliction[LimbType.LeftLeg] = "ll_arterialcut"
    limbtoaffliction[LimbType.RightArm] = "ra_arterialcut"
    limbtoaffliction[LimbType.LeftArm] = "la_arterialcut"
    limbtoaffliction[LimbType.Head] = "h_arterialcut"
    limbtoaffliction[LimbType.Torso] = "t_arterialcut"
    if limbtoaffliction[limbtype] == nil then return false end
    return HF.HasAffliction(character,limbtoaffliction[limbtype],1)
end
function DH.LimbIsAmputated(character,limbtype)
    return 
        DH.LimbIsTraumaticallyAmputated(character,limbtype) or
        DH.LimbIsSurgicallyAmputated(character,limbtype)
end
function DH.LimbIsTraumaticallyAmputated(character,limbtype)
    local limbtoaffliction = {}
    limbtoaffliction[LimbType.RightLeg] = "trl_amputation"
    limbtoaffliction[LimbType.LeftLeg] = "tll_amputation"
    limbtoaffliction[LimbType.RightArm] = "tra_amputation"
    limbtoaffliction[LimbType.LeftArm] = "tla_amputation"
    limbtoaffliction[LimbType.Head] = "th_amputation"
    if limbtoaffliction[limbtype] == nil then return false end
    return HF.HasAffliction(character,limbtoaffliction[limbtype],0.1)
end
function DH.LimbIsSurgicallyAmputated(character,limbtype)
    local limbtoaffliction = {}
    limbtoaffliction[LimbType.RightLeg] = "srl_amputation"
    limbtoaffliction[LimbType.LeftLeg] = "sll_amputation"
    limbtoaffliction[LimbType.RightArm] = "sra_amputation"
    limbtoaffliction[LimbType.LeftArm] = "sla_amputation"
    limbtoaffliction[LimbType.Head] = "sh_amputation"
    if limbtoaffliction[limbtype] == nil then return false end
    return HF.HasAffliction(character,limbtoaffliction[limbtype],0.1)
end

function DH.Fibrillate(character,amount)

    -- tachycardia (increased heartrate) ->
    -- fibrillation (irregular heartbeat) ->
    -- cardiacarrest

    -- fetch values
    local tachycardia = HF.GetAfflictionStrength(character,"tachycardia",0)
    local fibrillation = HF.GetAfflictionStrength(character,"fibrillation",0)
    local cardiacarrest = HF.GetAfflictionStrength(character,"cardiacarrest",0)

    -- already in cardiac arrest? don't do anything
    if cardiacarrest > 0 then return end

    -- determine total amount of fibrillation, then determine afflictions from that
    local previousAmount = tachycardia/5
    if fibrillation > 0 then previousAmount = 20 + fibrillation end
    local newAmount = previousAmount + amount

    -- 0-20: 0-100% tachycardia
    -- 20-120: 0-100% fibrillation
    -- >120: cardiac arrest

    if newAmount < 20 then
        -- 0-20: 0-100% tachycardia
        tachycardia = newAmount*5
        fibrillation = 0
    elseif newAmount < 120 then
        -- 20-120: 0-100% fibrillation
        tachycardia = 0
        fibrillation = newAmount-20
    else
        -- >120: cardiac arrest
        tachycardia = 0
        fibrillation = 0
        HF.SetAffliction(character,"cardiacarrest",10)
    end

    HF.SetAffliction(character,"tachycardia",tachycardia)
    HF.SetAffliction(character,"fibrillation",fibrillation)
end

HF = {} -- Helperfunctions

function HF.Lerp(a, b, t)
	return a + (b - a) * t
end



-- /// affliction magic ///
------------------------------


-- this might be overkill, but a lot of people have reported dislocation fixing issues
function HF.HasAfflictionExtremity(character,identifier,limbtype,minamount)
    local aff = nil
    
    if(limbtype == LimbType.LeftArm or limbtype == LimbType.LeftForearm or limbtype == LimbType.LeftHand) then
        aff = character.CharacterHealth.GetAffliction(identifier,character.AnimController.GetLimb(LimbType.LeftArm))
        if(aff==nil) then aff = character.CharacterHealth.GetAffliction(identifier,character.AnimController.GetLimb(LimbType.LeftForearm)) end
        if(aff==nil) then aff = character.CharacterHealth.GetAffliction(identifier,character.AnimController.GetLimb(LimbType.LeftHand)) end
    elseif(limbtype == LimbType.RightArm or limbtype == LimbType.RightForearm or limbtype == LimbType.RightHand) then
        aff = character.CharacterHealth.GetAffliction(identifier,character.AnimController.GetLimb(LimbType.RightArm))
        if(aff==nil) then aff = character.CharacterHealth.GetAffliction(identifier,character.AnimController.GetLimb(LimbType.RightForearm)) end
        if(aff==nil) then aff = character.CharacterHealth.GetAffliction(identifier,character.AnimController.GetLimb(LimbType.RightHand)) end
    elseif(limbtype == LimbType.LeftLeg or limbtype == LimbType.LeftThigh or limbtype == LimbType.LeftFoot) then
        aff = character.CharacterHealth.GetAffliction(identifier,character.AnimController.GetLimb(LimbType.LeftLeg))
        if(aff==nil) then aff = character.CharacterHealth.GetAffliction(identifier,character.AnimController.GetLimb(LimbType.LeftThigh)) end
        if(aff==nil) then aff = character.CharacterHealth.GetAffliction(identifier,character.AnimController.GetLimb(LimbType.LeftFoot)) end
    elseif(limbtype == LimbType.RightLeg or limbtype == LimbType.RightThigh or limbtype == LimbType.RightFoot) then
        aff = character.CharacterHealth.GetAffliction(identifier,character.AnimController.GetLimb(LimbType.RightLeg))
        if(aff==nil) then aff = character.CharacterHealth.GetAffliction(identifier,character.AnimController.GetLimb(LimbType.RightThigh)) end
        if(aff==nil) then aff = character.CharacterHealth.GetAffliction(identifier,character.AnimController.GetLimb(LimbType.RightFoot)) end
    end
    
    local res = false
    if(aff~=nil) then
        res = aff.Strength >= (minamount or 0.5)
    end
    return res
end

function HF.SetAffliction(character,identifier,strength,aggressor,prevstrength)
    HF.SetAfflictionLimb(character,identifier,LimbType.Torso,strength,aggressor,prevstrength)
end


function HF.ApplyAfflictionChange(character,identifier,strength,prevstrength,minstrength,maxstrength)
    strength = HF.Clamp(strength,minstrength,maxstrength)
    prevstrength = HF.Clamp(prevstrength,minstrength,maxstrength)
    if (prevstrength~=strength) then
        HF.SetAffliction(character,identifier,strength)
    end
end

function HF.ApplyAfflictionChangeLimb(character,limbtype,identifier,strength,prevstrength,minstrength,maxstrength)
    strength = HF.Clamp(strength,minstrength,maxstrength)
    prevstrength = HF.Clamp(prevstrength,minstrength,maxstrength)
    if (prevstrength~=strength) then
        HF.SetAfflictionLimb(character,identifier,limbtype,strength)
    end
end

function HF.ApplySymptom(character,identifier,hassymptom,removeifnot)    
    if(not hassymptom and not removeifnot) then return end
    
    local strength = 0
    if(hassymptom) then strength = 100 end
    
    if(removeifnot or hassymptom) then 
        HF.SetAffliction(character,identifier,strength)
    end
end

function HF.ApplySymptomLimb(character,limbtype,identifier,hassymptom,removeifnot)    
    if(not hassymptom and not removeifnot) then return end
    
    local strength = 0
    if(hassymptom) then strength = 100 end
    
    if(removeifnot or hassymptom) then 
        HF.SetAfflictionLimb(character,identifier,limbtype,strength)
    end
end

function HF.AddAfflictionLimb(character,identifier,limbtype,strength,aggressor)
    local prevstrength = HF.GetAfflictionStrengthLimb(character,limbtype,identifier,0)
    HF.SetAfflictionLimb(character,identifier,limbtype,
    strength+prevstrength,
    aggressor,prevstrength)
end

function HF.AddAffliction(character,identifier,strength,aggressor)
    local prevstrength = HF.GetAfflictionStrength(character,identifier,0)
    HF.SetAffliction(character,identifier,
    strength+prevstrength,
    aggressor,prevstrength)
end

function HF.AddAfflictionResisted(character,identifier,strength,aggressor)
    local prevstrength = HF.GetAfflictionStrength(character,identifier,0)
    strength = strength * (1-HF.GetResistance(character,identifier))
    HF.SetAffliction(character,identifier,
    strength+prevstrength,
    aggressor,prevstrength)
end

function HF.GetResistance(character,identifier)
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



function HF.GetSkillRequirementMet(character,skilltype,requiredamount)
    local skilllevel = HF.GetSkillLevel(character,skilltype)
    return HF.Chance(HF.Clamp(skilllevel/requiredamount,0,1))
end

function HF.GetSurgerySkillRequirementMet(character,requiredamount)
    local skilllevel = HF.GetSurgerySkill(character)
    return HF.Chance(HF.Clamp(skilllevel/requiredamount,0,1))
end

function HF.GetSurgerySkill(character)
    if NTSP ~= nil and DH.Config.NTSPenableSurgerySkill then 
        return math.max(
            5,
            HF.GetSkillLevel(character,"surgery"),
            HF.GetSkillLevel(character,"medical")/4)
     end

    return HF.GetSkillLevel(character,"medical")
end

function HF.GiveSkill(character,skilltype,amount)
    if character ~= nil and character.Info ~= nil then
        character.Info.IncreaseSkillLevel(Identifier(skilltype), amount)
    end
end

-- amount = vitality healed
function HF.GiveSkillScaled(character,skilltype,amount)
    if character ~= nil and character.Info ~= nil then
        HF.GiveSkill(character,skilltype,amount*0.001/math.max(HF.GetSkillLevel(character,skilltype),1))
    end
end



function HF.ForceArmLock(character,identifier)

    local handindex = 6
    if(identifier=="armlock2") then handindex = 5 end

    if SERVER then
        -- drop previously held item
        local previtem = character.Inventory.GetItemAt(handindex)
        if(previtem ~= nil) then 
            previtem.Drop(character,true)
        end

        HF.GiveItem(character,identifier)
    else
        local item = Item(ItemPrefab.GetItemPrefab(identifier), character.WorldPosition)

        -- drop previously held item
        local previtem = character.Inventory.GetItemAt(handindex)
        if(previtem ~= nil) then 
            previtem.Drop(character,true)
        end

        character.Inventory.ForceToSlot(item,handindex)
    end
end



-- this function is dumb and you shouldn't use it
function HF.TableSize(table)
    return #table
end

function HF.HasAbilityFlag(character,flagtype)
    return character.HasAbilityFlag(flagtype)
end

function HF.MakeAggressive(aggressor,target,damage)
    
    -- this shit isnt working!!!!
    -- why is this shit not working!?!?!?!
    
--    if(aggressor==nil) then print("no aggressor") else print("aggressor: "..aggressor.Name) end
--    if(target==nil) then print("no target") else print("target: "..target.Name) end
--    if(damage==nil) then print("no damage") else print("damage: "..damage) end
--    
--    if ((damage ~= nil and damage <= 0.5) or target==nil or aggressor==nil or target.AIController==nil or aggressor==target) then return end
--    
--    if damage == nil then damage = 5 end
--        
--    local AttackResult = LuaUserData.CreateStatic("Barotrauma.AttackResult")
--    local res = AttackResult.__new(Round(damage),nil)
--    target.AIController.OnAttacked(aggressor,res)
 
end

function HF.CharacterToClient(character)

    if not SERVER then return nil end

    for key,client in pairs(Client.ClientList) do
        if client.Character == character then 
            return client
        end
    end

    return nil
end



function HF.LimbTypeToString(type)
    if(type==LimbType.Torso) then return "Torso" end
    if(type==LimbType.Head) then return "Head" end
    if(type==LimbType.LeftArm or type==LimbType.LeftForearm or type==LimbType.LeftHand) then return "Left Arm" end
    if(type==LimbType.RightArm or type==LimbType.RightForearm or type==LimbType.RightHand) then return "Right Arm" end
    if(type==LimbType.LeftLeg or type==LimbType.LeftThigh or type==LimbType.LeftFoot) then return "Left Leg" end
    if(type==LimbType.RightLeg or type==LimbType.RightThigh or type==LimbType.RightFoot) then return "Right Leg" end
    return "???"
end

function HF.GameIsPaused()
    if SERVER then return false end

    return Game.Paused
end

function HF.TableContains(table, value)
    for i, v in ipairs(table) do
        if v == value then
            return true
        end
    end

    return false
end

function HF.PutItemInsideItem(container,identifier,index)
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

function HF.CanPerformSurgeryOn(character)
    return HF.HasAffliction(character,"analgesia",1) or HF.HasAffliction(character,"sym_unconsciousness",0.1)
end

-- converts thighs, feet, forearms and hands into legs and arms
function HF.NormalizeLimbType(limbtype) 
    if 
        limbtype == LimbType.Head or 
        limbtype == LimbType.Torso or 
        limbtype == LimbType.RightArm or
        limbtype == LimbType.LeftArm or
        limbtype == LimbType.RightLeg or
        limbtype == LimbType.LeftLeg then 
        return limbtype end

    if limbtype == LimbType.LeftForearm or limbtype==LimbType.LeftHand then return LimbType.LeftArm end
    if limbtype == LimbType.RightForearm or limbtype==LimbType.RightHand then return LimbType.RightArm end

    if limbtype == LimbType.LeftThigh or limbtype==LimbType.LeftFoot then return LimbType.LeftLeg end
    if limbtype == LimbType.RightThigh or limbtype==LimbType.RightFoot then return LimbType.RightLeg end

    if limbtype == LimbType.Waist then return LimbType.Torso end

    return limbtype
end

-- returns an unrounded random number
function HF.RandomRange(min,max) 
    return min+math.random()*(max-min)
end

function HF.LimbIsExtremity(limbtype) 
    return limbtype~=LimbType.Torso and limbtype~=LimbType.Head
end



function HF.CharacterDistance(char1,char2) 
    return HF.Distance(char1.WorldPosition,char2.WorldPosition)
end

function HF.Distance(v1,v2)
    return Vector2.Distance(v1,v2)
end

function HF.GetOuterWearIdentifier(character) 
    return HF.GetCharacterInventorySlotIdentifer(character,4)
end
function HF.GetInnerWearIdentifier(character) 
    return HF.GetCharacterInventorySlotIdentifer(character,3)
end
function HF.GetHeadWearIdentifier(character) 
    return HF.GetCharacterInventorySlotIdentifer(character,2)
end

function HF.GetCharacterInventorySlotIdentifer(character,slot) 
    local item = character.Inventory.GetItemAt(slot)
    if item==nil then return nil end
    return item.Prefab.Identifier.Value
end

function HF.GetItemInRightHand(character) 
    return HF.GetCharacterInventorySlot(character,6)
end
function HF.GetItemInLeftHand(character) 
    return HF.GetCharacterInventorySlot(character,5)
end
function HF.GetOuterWear(character) 
    return HF.GetCharacterInventorySlot(character,4)
end
function HF.GetInnerWear(character) 
    return HF.GetCharacterInventorySlot(character,3)
end
function HF.GetHeadWear(character) 
    return HF.GetCharacterInventorySlot(character,2)
end

function HF.GetCharacterInventorySlot(character,slot) 
    return character.Inventory.GetItemAt(slot)
end

function HF.ItemHasTag(item,tag)
    if item==nil then return false end
    return item.HasTag(tag)
end

function HF.CauseOfDeathToString(cod) 
    local res = nil

    if cod.Affliction ~= nil and -- from affliction
    cod.Affliction.CauseOfDeathDescription ~= nil then
        res = cod.Affliction.CauseOfDeathDescription
    else -- from type
        res = tostring(cod.Type)
    end

    return res or ""
end

function HF.CombineArrays(arr1,arr2)
    local res = {}
    for _,v in ipairs(arr1) do
        table.insert(res, v) end
    for _,v in ipairs(arr2) do
        table.insert(res, v) end
    return res
end

HF.EndocrineTalents =
{"aggressiveengineering","crisismanagement","cannedheat",
"doubleduty","firemanscarry","fieldmedic","multitasker",
"aceofalltrades","stillkicking","drunkensailor","trustedcaptain",
"downwiththeship","physicalconditioning","beatcop","commando",
"justascratch","intheflow","collegeathletics"}
function HF.ApplyEndocrineBoost(character,talentlist)
    talentlist=talentlist or HF.EndocrineTalents
        
    -- gee i sure do love translating c# into lua
    local targetCharacter = character
    if (targetCharacter.Info == nil) then return end
    local talentTree = TalentTree.JobTalentTrees[character.Info.Job.Prefab.Identifier.Value] if talentTree == nil then return end
    -- for the sake of technical simplicity, for now do not allow talents to be given if the character could unlock them in their talent tree as well
    local disallowedTalents = {}
    for subtree in talentTree.TalentSubTrees do for stage in subtree.TalentOptionStages do for talent in stage.Talents do
        table.insert(disallowedTalents,talent.Identifier.Value)
    end end end

    local characterTalents = {}
    for talent in targetCharacter.Info.UnlockedTalents do
        table.insert(characterTalents,talent.Value)
    end

    local viableTalents = {}
    for talent in talentlist do
        if not HF.TableContains(disallowedTalents,talent) and not HF.TableContains(characterTalents,talent) then
            table.insert(viableTalents,talent)
        end
    end
    
    if #viableTalents <= 0 then return end

    local talent = viableTalents[math.random(#viableTalents)]

    targetCharacter.GiveTalent(Identifier(talent), true);

end

function HF.JobMemberCount(jobidentifier)
    local res = 0
    for _, character in pairs(Character.CharacterList) do
        if (character.IsHuman and not character.IsDead and character.Info.Job ~= nil) then
            if character.Info.Job.Prefab.Identifier.Value == jobidentifier then
                res = res + 1
            end
        end
    end
    return res
end

function HF.SendTextBox(header,msg,client)
    if SERVER then
        Game.SendDirectChatMessage(header, msg, nil, 7, client)
    else
        GUI.MessageBox(header, msg)
    end
end

function HF.ReplaceString(original,find,replace)
    return string.gsub(original,find,replace)
end


function DH.HF.EnumerableToTable(enum)
    local res = {}
    for entry in enum do
        table.insert(res,entry)
    end
    return res
end


pullMode = {}

--Define a helper function to get the pull mode for an item
function getPullMode(item)
    return pullMode[item] or 0  --return 0 if the pull mode is not set yet
end

--Define a helper function to set the pull mode for an item
function setPullMode(item, mode)
    pullMode[item] = mode
end


