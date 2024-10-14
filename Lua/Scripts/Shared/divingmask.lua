--[[ Makes bots try to get an anechoic suit before repairing leaks when in dangerous range of an active sonar.
Hook.Patch(
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

Hook.Patch(
    "Barotrauma.Character",
    "Speak",
    function(instance, ptable)

    if tostring(ptable["identifier"]) == "getdivinggear" and DH.insidetheHull(instance) then
        ptable["message"] = TextManager.Get("Botai_getdivingmask").Value
	end
end, Hook.HookMethodType.Before)



function DH.insidetheHull(character)
	local inSide = true
    if not character.AnimController.currentHull then
        inSide = false
	end
	return inSide

end--]]





