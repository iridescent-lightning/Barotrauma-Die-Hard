// This class is patched to make bots don't find diving suits in hulls
using Barotrauma.Extensions;
using Barotrauma.Items.Components;
using Barotrauma.Networking; // used by the server
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

using HarmonyLib;
using System.Reflection;
using System.Xml.Linq;

using Barotrauma;


namespace BarotraumaDieHard
{
    class HumanAIControllerDieHard  : IAssemblyPlugin
    {


        public Harmony harmony;
        
        
        public void Initialize()
        {
            harmony = new Harmony("HumanAIControllerDieHard");

            // For this normal public direct patch
            harmony.Patch(
		    original: typeof(HumanAIController).GetMethod("NeedsDivingGear"),
		    prefix: new HarmonyMethod(typeof(HumanAIControllerDieHard).GetMethod("NeedsDivingGearPrefix"))
            );
            
        }

        public void OnLoadCompleted() { }
        public void PreInitPatching() { }

        public void Dispose()
        {
            harmony.UnpatchAll();
            harmony = null;
        }


        public static bool NeedsDivingGearPrefix(Hull hull, out bool needsSuit, HumanAIController __instance, ref bool __result)
        {
            HumanAIController _ = __instance;

            needsSuit = false;
            bool needsAir = _.Character.NeedsAir && _.Character.CharacterHealth.OxygenLowResistance < 1;

            if (hull == null) // No hull means the character is outside, needs a suit for pressure protection
            {
                needsSuit = !_.Character.IsImmuneToPressure;
                __result = needsAir || needsSuit;
                return false;
            }

            // If inside a hull, only check if air is needed, no suit is required even if breached
            if (hull.WaterPercentage > 90 || hull.LethalPressure > 0 || hull.ConnectedGaps.Any(gap => !gap.IsRoomToRoom && gap.Open > 0.9f))
            {
                needsSuit = false;  // No suit required since they're inside a hull, despite gaps or lethal pressure
                __result = needsAir; // Only needs air, not a suit
                return false;
            }

            // If hull has water or low oxygen, check air needs
            if (hull.WaterPercentage > 60 || (hull.IsWetRoom && hull.WaterPercentage > 10) || hull.OxygenPercentage < HumanAIController.HULL_LOW_OXYGEN_PERCENTAGE + 1)
            {
                __result = needsAir;
                return false;
            }

            __result = false;
            return false;
        }





        
    }
}