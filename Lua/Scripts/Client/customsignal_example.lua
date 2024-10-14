local function register(type, name)
    if name == nil then
        name = type:match('%.[^\n%.]*$'):sub(2, -1)
    end
    local nameType = 'Type_' .. name

    _G[nameType] = LuaUserData.RegisterType(type)
    _G[name] = LuaUserData.CreateStatic(type)
end

register("Barotrauma.Items.Components.Sonar")
-- register("Barotrauma.Items.Components.SonarTransducer")
Hook.HookMethod("Barotrauma.Items.Components.Steering", "Update", function(instance, ptable)
    if instance.item ~= nil and instance.item.Submarine ~= nil and instance.posToMaintain ~= nil then
        local deltaX = 0
        local deltaY = 0

        local sonar = instance.item.GetComponentString("Sonar")
        local sonarPosX = 0
        local sonarPosY = 0

        if sonar ~= nil then
            if sonar.UseTransducers and sonar.CenterOnTransducers then
                local count = 0
                for transducer in sonar.ConnectedTransducers do
                    count = count + 1
                    sonarPosX = sonarPosX + transducer.Item.WorldPosition.X
                    sonarPosY = sonarPosY + transducer.Item.WorldPosition.Y
                end
                if count > 0 then
                    sonarPosX = sonarPosX / count
                    sonarPosY = sonarPosY / count
                end
            end
        end

        if sonarPosX ~= 0 or sonarPosY ~= 0 then
            deltaX = sonarPosX - instance.item.Submarine.WorldPosition.X
            deltaY = sonarPosY - instance.item.Submarine.WorldPosition.Y
        end
        local posX = instance.PosToMaintain.X + deltaX
        local posY = instance.PosToMaintain.Y + deltaY

        instance.item.SendSignal(posX * Physics.DisplayToRealWorldRatio, "maintain_out_X")
        if Level.Loaded == nil then
            instance.item.SendSignal(-posY * Physics.DisplayToRealWorldRatio, "maintain_out_Y")
        else
            instance.item.SendSignal(Level.Loaded.GetRealWorldDepth(posY), "maintain_out_Y")
        end
    end
end, Hook.HookMethodType.After)

