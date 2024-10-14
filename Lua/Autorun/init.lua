
DH = {} -- Die Hard
DH.Name="Die Hard"
DH.Version = "1.1.2.1" 
DH.VersionNum = 01010201 -- seperated into groups of two digits: 01020304 -> 1.2.3h4; major, minor, patch, hotfix
DH.Path = table.pack(...)[1]

-- register Die Hard as a neurotrauma "expansion"
DH.MinNTVersion = "A1.8.1"
DH.MinNTVersionNum = 01080100
Timer.Wait(function() if NTC ~= nil and NTC.RegisterExpansion ~= nil then NTC.RegisterExpansion(DH) end end,1)

-- config loading
if not File.Exists(DH.Path .. "/config.json") then

    -- create default config if there is no config file
    DH.Config = dofile(DH.Path .. "/Lua/defaultconfig.lua")
    File.Write(DH.Path .. "/config.json", json.serialize(DH.Config))

else

    -- load existing config
    DH.Config = json.parse(File.Read(DH.Path .. "/config.json"))
    
    -- add missing entries
    local defaultConfig = dofile(DH.Path .. "/Lua/defaultconfig.lua")
    for key, value in pairs(defaultConfig) do
        if DH.Config[key] == nil then
            DH.Config[key] = value
        end
    end
end

-- define global helper functions (they're used everywhere else!)
dofile(DH.Path.."/Lua/Scripts/DHhelperfunctions1.lua")
dofile(DH.Path.."/Lua/Scripts/DHhelperfunctions.lua")


-- shared code
    dofile(DH.Path.."/Lua/Scripts/Shared/itemupdate.lua")--shared
	dofile(DH.Path.."/Lua/Scripts/Client/watch.lua")--shared
	--dofile(DH.Path.."/Lua/Scripts/Shared/fluidmod.lua")--shared
	dofile(DH.Path.."/Lua/Scripts/Shared/weaponaim.lua")--shared
	dofile(DH.Path.."/Lua/Scripts/Shared/wreckupdate.lua")--shared
	
	dofile(DH.Path.."/Lua/Scripts/Shared/itemprefix.lua")--shared
	--dofile(DH.Path.."/Lua/Scripts/Shared/characterupdate.lua")--shared
	dofile(DH.Path.."/Lua/Scripts/Shared/dynamic_collision.lua")--shared broke by update need to fix
	dofile(DH.Path.."/Lua/Scripts/Shared/changedoorspeed.lua")--shared
	dofile(DH.Path.."/Lua/Scripts/Server/interactivesfx.lua")--shared
	
	--dofile(DH.Path.."/Lua/Scripts/Shared/armor.lua")--shared now a c# feature
    dofile(DH.Path.."/Lua/Scripts/Shared/repair.lua")--shared
	--dofile(DH.Path.."/Lua/Scripts/Shared/weaponaim.lua")--shared
-- server-side code (also run in singleplayer)
if (Game.IsMultiplayer and SERVER) or not Game.IsMultiplayer then

    -- Version and expansion display
    Timer.Wait(function() Timer.Wait(function()
        local runstring = "\n/// Running Die Hard V "..DH.Version.." ///\n"

        -- add dashes
        local linelength = string.len(runstring)+4
        local i = 0
        while i < linelength do runstring=runstring.."-" i=i+1 end

        -- if you were to ever create Die Hard expansions then here would be the place
        -- to print them out alongside the Die Hard version

        print(runstring)
    end,1) end,1)

    -- this is where we run all the other lua files
    -- (jamming them all in autorun is bad for organization and surrenders control of what is to be executed)
	
	
	
	--dofile(DH.Path.."/Lua/Scripts/Shared/disable_despawn.lua")
    
	--dofile(DH.Path.."/Lua/Scripts/Server/calculatingmass.lua")--server
	--dofile(DH.Path.."/Lua/Scripts/Shared/verticalengine.lua")--server
    dofile(DH.Path.."/Lua/Scripts/Shared/zdoor.lua")--server
	dofile(DH.Path.."/Lua/Scripts/Server/jailsystem.lua")--server
	
	
end

-- client-side code
if CLIENT then
    --[[dofile(DH.Path.."/Lua/Scripts/Client/configgui.lua")--]]
	
	--dofile(DH.Path.."/Lua/Scripts/Shared/fluidmod.lua")--shared
	
	--dofile(DH.Path.."/Lua/Scripts/Client/watch.lua")--shared
	dofile(DH.Path.."/Lua/Scripts/Client/weapon_zoom.lua")--client-side
	dofile(DH.Path.."/Lua/Scripts/Client/zoom.lua")--client-side
	dofile(DH.Path.."/Lua/Scripts/Client/itemdropkey.lua")--client-side
	dofile(DH.Path.."/Lua/Scripts/Client/debugcommand.lua")--client-side
	dofile(DH.Path.."/Lua/Scripts/Client/playdead.lua")--client-side
	dofile(DH.Path.."/Lua/Scripts/Client/repairhint.lua")--client-side
	dofile(DH.Path.."/Lua/Scripts/Client/HolsterUnholsterKeyOuter.lua")--client-side
	dofile(DH.Path.."/Lua/Scripts/Client/guimachine.lua")--client-side
	
	
	--[[dofile(DH.Path.."/Lua/Scripts/Shared/wreckupdate.lua")--shared
	dofile(DH.Path.."/Lua/Scripts/Shared/itemupdate.lua")--should be shared
	dofile(DH.Path.."/Lua/Scripts/Shared/itemprefix.lua")--shared
	dofile(DH.Path.."/Lua/Scripts/Shared/characterupdate.lua")--shared
	dofile(DH.Path.."/Lua/Scripts/Shared/dynamic_collision.lua")--shared
	dofile(DH.Path.."/Lua/Scripts/Shared/changedoorspeed.lua")--shared
	dofile(DH.Path.."/Lua/Scripts/Server/interactivesfx.lua")--shared
	dofile(DH.Path.."/Lua/Scripts/Shared/armor.lua")--shared
	dofile(DH.Path.."/Lua/Scripts/Shared/repair.lua")--shared--]]
end