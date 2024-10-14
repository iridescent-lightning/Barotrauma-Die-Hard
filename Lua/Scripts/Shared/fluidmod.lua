--mainsubHulls = {} -- seems like fm runs before the prefix so table was nil, so move table here.
gas = dofile(DH.Path.."/Lua/Scripts/Shared/gasses.lua") --now i make it global so the watch can display temperature
local fluidSimulation = dofile(DH.Path.."/Lua/Scripts/Shared/fluid_simulation.lua")
local mathUtils = dofile(DH.Path.."/Lua/Scripts/Shared/math_utils.lua")

gas.DefineGas("co2")
gas.DefineGas("Chlorine")

fluidSimulation.SetGasses(gas)

local burnPrefab = AfflictionPrefab.Prefabs["Burn"]
local pressureDeathPrefab = AfflictionPrefab.Prefabs["Burn"]
local highPressurePrefab = AfflictionPrefab.Prefabs["Burn"]
local hypothermiaPrefab = AfflictionPrefab.Prefabs["coldwater"]
local chlorinePrefab = AfflictionPrefab.Prefabs["chlorine_poisoning"]
local copoisionPrefab = AfflictionPrefab.Prefabs["co_poisoning"]
--[[for k, v in pairs(AfflictionPrefab.ListArray) do
    if v.name == "Burn" then
       burnPrefab = v
    end

    if v.name == "High Pressure" then
        highPressurePrefab = v
    end

    if v.name == "Barotrauma" then
        pressureDeathPrefab = v
    end

    if v.name == "Hypothermia" then
        hypothermiaPrefab = v
    end
end--]]


local oxygenUpdateTimer = 0
local afflictionUpdateTimer = 0
Hook.Add("think", "fluidmod", function()

if DH.HF.GameIsPaused() then return end

    if Timer.GetTime() > oxygenUpdateTimer then

        for k, v in pairs(Gap.GapList) do
            if v.open > 0 then
                local linkedHulls = v.linkedTo
                if linkedHulls[1] ~= nil and linkedHulls[2] ~= nil and linkedHulls[1] ~= linkedHulls[2] then
                    fluidSimulation.Simulate(v, linkedHulls[1], linkedHulls[2])
                end
            end
        end
		
		
		for hull, hullInfo in pairs(mainsubHulls) do
			local sizeHull = hullInfo.Rsize
			local hulltemperature = gas.GetTemperature(hull)
			local numberOfFire = #(hull.FireSources)--FireSource(worldPosition, yourHull, true)
			local ChlorineAmount = gas.GetGas(hull, 'Chlorine')
			
			
			
			if numberOfFire > 0 and hull.WaterPercentage < 40 then
				gas.AddTemperature(hull, 30*numberOfFire/sizeHull)
				gas.AddGas(hull, "co2", 0.5)
			elseif hull.WaterPercentage >= 40 and hulltemperature >= 274.15 then
				gas.AddTemperature(hull, -0.1)
			end

			if hulltemperature >= 274.15 then
				gas.AddTemperature(hull, -0.01)
				
			else
				gas.AddTemperature(hull, 0.01)
			end
			
			
		end
		
        oxygenUpdateTimer = Timer.GetTime() + 0.5 -- updates run at 10 times a second
    end

    if Timer.GetTime() > afflictionUpdateTimer then
        for k, char in pairs(Character.CharacterList) do
            if char.CurrentHull and char.CharacterHealth and mainsubHulls[char.CurrentHull] then--check if char in our hulls
                local temp = gas.GetTemperature(char.CurrentHull)
				local CharacterInv = char.Inventory
				local pMask = CharacterInv.GetItemAt(2)
				local pDivingSuit = CharacterInv.GetItemAt(3)
				local chlorineAmountChar = gas.GetGas(char.CurrentHull, 'Chlorine')
				local co2AmountChar = gas.GetGas(char.CurrentHull, 'co2')
				
				
                local amountHypothermia = char.CharacterHealth.GetAffliction("coldwater")
				
				gas.AddGas(char.CurrentHull, "co2", 0.6/mainsubHulls[char.CurrentHull].Rsize)
				
                if amountHypothermia ~= nil then amountHypothermia = amountHypothermia.Strength 
                else amountHypothermia = 0 end

                if temp > gas.maxNormalTemperature then
                    local damage = (temp - (gas.maxNormalTemperature)) / 100
                    for k, limb in pairs(char.AnimController.Limbs) do--why only work on client?nononoahahahahahahha. no cheat code only heat up hull on client. thats why
					
                        char.CharacterHealth.ApplyAffliction(limb, burnPrefab.Instantiate(damage))
						
                    end
                    char.CharacterHealth.ApplyAffliction(char.AnimController.MainLimb, hypothermiaPrefab.Instantiate(-1))
                elseif temp < gas.minNormalTemperature then
                    char.CharacterHealth.ApplyAffliction(char.AnimController.MainLimb, hypothermiaPrefab.Instantiate(0.5))
                elseif chlorineAmountChar > 12 and char.UseHullOxygen then
					char.CharacterHealth.ApplyAffliction(char.AnimController.MainLimb, chlorinePrefab.Instantiate(0.5))
				elseif co2AmountChar  > 8 and char.UseHullOxygen then
					char.CharacterHealth.ApplyAffliction(char.AnimController.MainLimb, copoisionPrefab.Instantiate(1))
                end

            end 
        end 

        afflictionUpdateTimer = Timer.GetTime() + 0.5
    end
end)

Game.DisableSpamFilter(true)
Hook.Add("chatMessage", "chatMessageDebug", function (msg, client)
    if msg == "!info" then
        local mess = ""

        mess = mess .. "Volume: " .. Character.Controlled.CurrentHull.Volume
        mess = mess .. "\nTemperature: " .. gas.GetTemperature(Character.Controlled.CurrentHull) .. "K"
		mess = mess.. "\nTemperature: " .. string.format("%.1f", tonumber(gas.GetTemperature(Character.Controlled.CurrentHull)- 273.15)) .. "°C"
		
        for _, gasName in pairs(gas.listGasses) do
            mess = mess .. "\n" .. gasName .. ": " .. gas.GetGas(Character.Controlled.CurrentHull, gasName)
        end
        --Game.SendMessage(mess, 1)
		PrintChat(mess)
		--print(mess)
    end

    if msg == "!add" then
        gas.AddGas(Character.Controlled.CurrentHull, "Chlorine", 2000)
    end

    if msg == "!ad" then
        gas.AddGas(Character.Controlled.CurrentHull, "Chlorine", 50)
    end

    if msg == "!hot" then
		if Client then
        gas.AddTemperature(Character.Controlled.CurrentHull, 50)
		elseif Server then
		gas.AddTemperature(client.Character.CurrentHull, 50)
		end
    end

    if msg == "!cold" then
        gas.AddTemperature(Character.Controlled.CurrentHull, -50)
    end

    if msg == "!pressure" then
        client.Character.CurrentHull.LethalPressure = 1000000
    end

    if msg == "!pressure1" then
        Game.SendMessage(client.Character.CurrentHull.LethalPressure, 1)
    end

    if msg == "!removeoxygen" then
        client.Character.CurrentHull.Oxygen = 0
    end
	
	if msg == "!size" then
		--print(Character.Controlled.CurrentHull.Size)
		local inputString = tostring(Character.Controlled.CurrentHull.Size)
		local x, y = inputString:match("X:(%d+) Y:(%d+)")
		x = tonumber(x)
		y = tonumber(y)
		print(x.."--"..y.."---"..x*y/10000)
	end
end)

--[[Hook.Add("changeFallDamage", "testFallDamage", function (amount, character, impactpos, velocity)
    local damage = velocity.Length() * 15
    return damage
end)--]]
