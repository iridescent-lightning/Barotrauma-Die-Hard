--Has to wait to avoid data jamming. 
--gas = dofile(DH.Path.."/Lua/Scripts/Shared/gasses.lua")what the hell did i put it here

Hook.Add("roundStart","InitWatch",function()
if not CLIENT then return end
watchinit = {}
Timer.Wait(function()
	for _,watch in pairs (Item.ItemList) do
		if watch.Prefab.Identifier == "divingwatch" then
		watchinit[watch] = 
		{
			initseconds = 0,
			initmintues = 0
		}
		end
		
	end
end, 3000) 
end)

Hook.Add("roundEnd","ClearWatch",function()
if not CLIENT then return end
watchinit = {}
end)



Hook.Add("Watch","Watch",function(effect, deltaTime, item, targets, worldPosition)
if not CLIENT then return end
watchinit[item].initseconds = watchinit[item].initseconds + 1
if watchinit[item].initseconds == 60 then
watchinit[item].initmintues = watchinit[item].initmintues + 1
watchinit[item].initseconds = 0
end


item.Description = "Used by diviers to count time.\nTimer: "..formattime(watchinit[item].initmintues*60 + watchinit[item].initseconds)

end)


Hook.Add("ResetWatch","ResetWatch",function(effect, deltaTime, item, targets, worldPosition)
if not CLIENT then return end
watchinit[item].initseconds = 0
watchinit[item].initmintues = 0
end)
Hook.Add("AddTime","AddTime",function(effect, deltaTime, item, targets, worldPosition)
if not CLIENT then return end
watchinit[item].initseconds = watchinit[item].initseconds + 10
end)
Hook.Add("MinusTime","MinusTime",function(effect, deltaTime, item, targets, worldPosition)
if not CLIENT then return end
watchinit[item].initseconds = watchinit[item].initseconds - 10
end)

function formattime(seconds)
	local minutes = math.floor(seconds / 60)
	local seconds_remainder = seconds % 60
	return string.format("%02d:%02d", minutes, seconds_remainder)
end

function DH.distanceBetween(point1, point2)
    local xd = point1.X - point2.X
    local yd = point1.Y - point2.Y
    return math.sqrt(xd*xd + yd*yd)
end

function DH.FindDepth(item)
    if SERVER then
      return Level.Loaded.GetRealWorldDepth(item.WorldPosition.Y)
    else
      return item.WorldPosition.Y * Physics.DisplayToRealWorldRatio
    end
end

Hook.Add("WatchPanel","WatchPanel",function(effect, deltaTime, item, targets, worldPosition)
	--if item.ParentInventory == nil or item.ParentInventory.Owner == nil then return end
	if not CLIENT or not item.ParentInventory.Owner.IsLocalPlayer then return end
	
	if item.ParentInventory.Owner.CurrentHull then
	local inputString = string.lower(item.ParentInventory.Owner.CurrentHull.RoomName)
	roomNameWatch = inputString:gsub("roomname.", "")
	--local temperature = gas.GetTemperature(item.ParentInventory.Owner.CurrentHull)
	
	subDistance = "NA "
	
	waterDepth = "NA "
	
	else
	roomNameWatch = "NA "
	subDistance = string.format("%.2f", DH.distanceBetween(Submarine.MainSub.WorldPosition,item.WorldPosition)/100).."m "
	
	waterDepth = string.format("%.1f",DH.FindDepth(item)).."m "
	end
	
	local speed = string.format("%.2f", item.ParentInventory.Owner.CurrentSpeed)
	
	local frame = GUI.Frame(GUI.RectTransform(Vector2(0.3, 0.2)), nil)
		frame.CanBeFocused = false

	local display = formattime(watchinit[item].initmintues*60 + watchinit[item].initseconds)
	local text = GUI.TextBlock(GUI.RectTransform(Vector2(1, 0.05), frame.RectTransform), " SPD: "..speed.."m/s ".."SubDis: "..subDistance.."Depth: "..waterDepth.." Room: "..roomNameWatch..(" Timer: "..tostring(display)), nil, nil, GUI.Alignment.Center)
	--local button = GUI.Button(GUI.RectTransform(Vector2(0.2, 0.2), frame.RectTransform, GUI.Anchor.TopRight), "Custom GUI Example", GUI.Alignment.Center, "PowerButton")This is an powerbutton
	
		frame.AddToGUIUpdateList()
end)
--[[Hook.Add("airMonitor","airMonitor",function(effect, deltaTime, item, targets, worldPosition)
	if not CLIENT or not item.InPlayerSubmarine or not item.ParentInventory.Owner.IsLocalPlayer then return end
	if item.GetComponentString('ItemContainer').Inventory.GetItemAt(0) == nil then return end 
	if item.GetComponentString('ItemContainer').Inventory.GetItemAt(0).Condition >0 then
		

	local temperature = gas.GetTemperature(item.ParentInventory.Owner.CurrentHull)
	chlorineMole = gas.GetGas(item.ParentInventory.Owner.CurrentHull, 'Chlorine')
	co2Mole = gas.GetGas(item.ParentInventory.Owner.CurrentHull, 'co2')
	-- i dont know hwy these have to be golbal value
	
	local frame = GUI.Frame(GUI.RectTransform(Vector2(0.5, 0.3)), nil)
		frame.CanBeFocused = false

	local text = GUI.TextBlock(GUI.RectTransform(Vector2(1, 0.05), frame.RectTransform), "Temperature: "..string.format("%.1f", tonumber(gas.GetTemperature(Character.Controlled.CurrentHull)- 273.15)).. "°C ".." Chlorine: "..string.format("%.1f",chlorineMole).." CO2: "..string.format("%.1f",co2Mole), nil, nil, GUI.Alignment.Center)
	
		frame.AddToGUIUpdateList()
		--end
	end
end)
Hook.Add("WatchPanel","WatchPanel",function(effect, deltaTime, item, targets, worldPosition)
	if item.ParentInventory == nil or item.ParentInventory.Owner == nil then return end
	
	local frame = GUI.Frame(GUI.RectTransform(Vector2(0.2, 0.2)), nil)
		frame.CanBeFocused = false

local button = GUI.Button(GUI.RectTransform(Vector2(0.2, 0.2), frame.RectTransform, GUI.Anchor.TopRight), "Reset", GUI.Alignment.Center, "GUIButtonSmall")

button.OnClicked = function ()
    watchinit[item].init = 0
end

		frame.AddToGUIUpdateList()
end)--]]


--local densityUpdateTimer = 0

--local totalDensity = 0 --global so monitor can display on client

--guitext must be run on one pc ortherwise overlaping


--[[Hook.Patch("Barotrauma.Items.Components.MiniMap","UpdateHullStatus",function(instance,ptable)
somehow this breaks the speedcalculation

	local voltage = instance.Voltage
	local minvoltage = instance.MinVoltage
	if voltage < minvoltage then return end
	
	local frame1 = GUI.Frame(GUI.RectTransform(Vector2(0.5, 0.5)), nil)
		frame1.CanBeFocused = false
		
	local text = GUI.TextBlock(GUI.RectTransform(Vector2(0.5, 0.24), frame1.RectTransform), " Total item load mass in submarine: "..string.format("%.2f",totalDensity), nil, nil, GUI.Alignment.Center)
	text.TextColor = Color(12, 143, 86)
	local header1 = GUI.TextBlock(GUI.RectTransform(Vector2(0.5, 0.295), frame1.RectTransform), "Submarine Dimension", nil, nil, GUI.Alignment.Center)
	header1.TextColor = Color(12, 143, 86)
	
	local text2 = GUI.TextBlock(GUI.RectTransform(Vector2(0.5, 0.355), frame1.RectTransform), "Width: "..string.format("%.2f",subHeight/100).." Height: "..string.format("%.2f",subWidth/100).." Ratio: "..string.format("%.2f",subDimensionRatio), nil, nil, GUI.Alignment.Center)
	text2.TextColor = Color(12, 143, 86)
	
	local text3 = GUI.TextBlock(GUI.RectTransform(Vector2(0.5, 0.415), frame1.RectTransform)," Shape Optimization: "..string.format("%.2f",shapeFactor/subDimensionRatio*100).."%", nil, nil, GUI.Alignment.Center)
	text3.TextColor = Color(12, 143, 86)
	
        frame1.AddToGUIUpdateList()

end)--]]
