if SERVER then return end


-- customization
isToggle=true -- toggle or hold behaviour
smoothZoom=false -- smooth or step

gzsStep=0.33333 -- step size for when smoothZoom=false
gzsSpeed=0.5 -- speed for when smoothZoom=true
gzsStart=1.5 -- default zoom level [0.5, 2]
zoomOn=true -- default zoom state

zKey=Keys.p -- zoom key

-- customization end


gzsDefault=false
gzsNew=gzsStart
gzsMin=1.5
gzsMax=1.5
gzsUpd=false

dHeld=false
iHeld=false


LuaUserData.MakeFieldAccessible(Descriptors["Barotrauma.Camera"],"globalZoomScale")
LuaUserData.MakeMethodAccessible(Descriptors["Barotrauma.Camera"],"CreateMatrices")

Hook.HookMethod("Barotrauma.Camera","CreateMatrices",function(instance,ptable)
	gzsDefault=instance.globalZoomScale
	gzsMin=math.max(0.5,gzsDefault*0.5)
	gzsMax=math.min(1.5,gzsDefault*1.5)
	gzsNew=math.max(math.min(gzsMax,gzsDefault*gzsStart),gzsMin)
	gzsUpd=true
end,Hook.HookMethodType.After)

Hook.HookMethod("Barotrauma.Character","ControlLocalPlayer",function(instance,ptable)
print("zoom")
	gzsUpd=false
	if not gzsDefault then
		ptable.cam.CreateMatrices()
	else
		if not Character.DisableControls and Character.Controlled then
			if zoomOn then
					if smoothZoom then
						gzsNew=math.min(gzsMax,gzsNew*(1+gzsSpeed))
						gzsUpd=true
					elseif not iHeld then
						gzsNew=math.min(gzsMax,gzsNew+gzsStep)
						iHeld=true
						gzsUpd=true
					end
				else
					iHeld=false
				end



				if not isToggle then
					if not zHeld then
						zoomOn=not zoomOn
						zHeld=true
						gzsUpd=true
					end
				else
					zoomOn=true
					gzsUpd=true
				end
			elseif isToggle then
				zHeld=false
			elseif zoomOn then
				zoomOn=false
				gzsUpd=true
			end
		if gzsUpd then
			ptable.cam.globalZoomScale=zoomOn and gzsNew or gzsDefault
		end
	end
end,Hook.HookMethodType.After)
