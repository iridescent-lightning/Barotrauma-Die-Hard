DH.signalReceivedMethods = {}
jailSpawnPoints = {}
jailedCharacter = {}



-- boosters



DH.omniVerticalBoosterSmall = {}
DH.signalReceivedMethods.verticalboostersmall = function(signal,connection)

    -- register verticalbooster
    if DH.omniVerticalBoosterSmall[connection.Item] == nil then
        DH.omniVerticalBoosterSmall[connection.Item] = connection.Item
    end

	if connection.Name == "set_Vertical_force" then
        local memComponents = DH.HF.EnumerableToTable(connection.Item.GetComponents(Components.MemoryComponent))
        if memComponents[1] ~= nil then
            memComponents[1].Value = signal.value
        end
    end
end

Hook.Add("think", "verticalboostersmall", function ()

    if HF.GameIsPaused() then return end

    for item in DH.omniVerticalBoosterSmall do
        if item ~= nil and item.Submarine ~= nil then
            
            local memComponents = DH.HF.EnumerableToTable(item.GetComponents(Components.MemoryComponent))
            local PoweredComponent = item.GetComponent(Components.Powered)

            local maxForce = math.max(tonumber(memComponents[2].Value) or 2000,0)
            local powerConsumption = math.max(tonumber(memComponents[3].Value) or 4000,0)

            local forceY = 100

            PoweredComponent.PowerConsumption = (math.abs(forceY)) / 200 * powerConsumption;
            PoweredComponent.IsActive = true
            
            local Voltage = PoweredComponent.Voltage
            local MinVoltage = 0.5

            if (Voltage < MinVoltage) then forceY = 0 end

            if (math.abs(forceY) > 1) then
            
                local voltageFactor = math.min(Voltage, 1)
                if MinVoltage <= 0 then voltageFactor= 1 end

                local currForceY = forceY * voltageFactor

                -- arbitrary multiplier that was added to changes in submarine mass without having to readjust all engines
                local forceMultiplier = 0.1

                currForceY = currForceY * maxForce * forceMultiplier

                local forceVector = Vector2(0, currForceY)
                item.Submarine.ApplyForce(forceVector)

            end
        end
    end
end)


function fetchVerticalBoosterSmall()--moved from the vertical part so i can init it here

    local itemsfound = {
        verticalboostersmall={}
    }
    DH.omniVerticalBoosterSmall = {}

    for item in Item.ItemList do
        for identifier,itemtable in pairs(itemsfound) do
            if identifier == item.Prefab.Identifier.Value then
                table.insert(itemtable,item)
                break
            end
        end 
    end

    for item in itemsfound.verticalboostersmall do DH.omniVerticalBoosterSmall[item] = item end
end



--[[Hook.Add("roundStart", "DH.RoundStart", function()
    fetchVerticalBoosterSmall()
end)--]]

--the fetching function moved to prefix and the roundstart was combined













DH.inhibitors = {}
DH.signalReceivedMethods.mb_inertiainhibitor = function(signal,connection)

    -- register inhibitor
    if DH.inhibitors[connection.Item] == nil then
        DH.inhibitors[connection.Item] = connection.Item
    end

    if connection.Name == "set_state" then
        local LightComponent = connection.Item.GetComponent(Components.LightComponent)
        LightComponent.IsOn = (tonumber(signal.value) or 0) > 0
    end
end

Hook.Add("think", "subfreezer", function ()
    if DH.HF.GameIsPaused() then return end

    for item in DH.inhibitors do
        if item ~= nil and item.Submarine ~= nil and item.Submarine.PhysicsBody ~= nil then
            
            local PoweredComponent = item.GetComponent(Components.Powered)
            local LightComponent = item.GetComponent(Components.LightComponent)

            PoweredComponent.IsActive = LightComponent.IsOn

            if (PoweredComponent.IsActive and PoweredComponent.Voltage >= PoweredComponent.MinVoltage) then
                item.Submarine.Velocity = Vector2(0,0)

                local memComponents = DH.HF.EnumerableToTable(item.GetComponents(Components.MemoryComponent))
                local desiredposX = tonumber(memComponents[1].Value)
                local desiredposY = tonumber(memComponents[2].Value)
                if desiredposX~=nil and desiredposY~=nil then
                    -- determine distance to desired position
                    local xDiff = math.abs(item.Submarine.PhysicsBody.FarseerBody.Position.X-desiredposX)
                    local yDiff = math.abs(item.Submarine.PhysicsBody.FarseerBody.Position.Y-desiredposY)
                    local distance = math.sqrt(xDiff^2 + yDiff^2)

                    if distance < 15 then
                        -- snap submarine to the desired position
                        -- this *could* cause some phasing bullshit to happen with things that are outside
                        item.Submarine.PhysicsBody.FarseerBody.Position = Vector2(desiredposX,desiredposY)
                    else
                        -- we're too far away to snap back, set current position to the desired one
                        memComponents[1].Value = tostring(item.Submarine.PhysicsBody.FarseerBody.Position.X)
                        memComponents[2].Value = tostring(item.Submarine.PhysicsBody.FarseerBody.Position.Y)
                    end
                else
                    -- no valid position in memory, assign the current one
                    memComponents[1].Value = tostring(item.Submarine.PhysicsBody.FarseerBody.Position.X)
                    memComponents[2].Value = tostring(item.Submarine.PhysicsBody.FarseerBody.Position.Y)
                end
            end
        end
    end
end)

