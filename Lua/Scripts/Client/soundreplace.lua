Hook.Patch("Barotrauma.SoundPlayer" , "PlaySound", 
{
	"Barotrauma.Sounds.Sound",
	"Microsoft.Xna.Framework.Vector2",
	"System.Single",
	"System.Single",
	"System.Single",
	"Barotrauma.Hull",
	"System.Boolean"
},
function(instance, ptable)

local sound = ptable["sound"]
local soundName = sound.filename

if string.find(soundName, "WaterAmbienceIn") then
	ptable["volume"] = Single(ptable["volume"] * 0)
elseif string.find(soundName, "WaterAmbienceMoving") then
	ptable["volume"] = Single(ptable["volume"] * 0)
end

end , Hook.HookMethodType.Before)