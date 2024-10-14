--the higher the load, the slower the sub 
--just like the boosters, sub movement is in charge of server so it should be server only



Hook.Patch("Barotrauma.SubmarineBody", "Update", function(instance,patable)

--if not subDimensionRatio then return end
--if Server and Game.IsMultiplayer then return end
	
    
    local decelerationFactor = 0.001
	 
    local decelerationForce = totalDensity * decelerationFactor * 1/tonumber(subDimensionRatio)
	
	
    Submarine.MainSub.ApplyForce(Submarine.MainSub.velocity * -decelerationForce)
	
end,Hook.HookMethodType.After)


