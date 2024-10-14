/*using Barotrauma.Extensions;
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
    class AIObjectiveRepairItemsDieHard  : IAssemblyPlugin
    {


        public Harmony harmony;
        
        
        public void Initialize()
        {
            harmony = new Harmony("AIObjectiveRepairItemsDieHard");

            harmony.Patch(
                original: typeof(AIObjectiveRepairItems).GetMethod("IsValidTarget"),
                prefix: new HarmonyMethod(typeof(AIObjectiveRepairItemsDieHard).GetMethod(nameof(IsValidTargetPrefix)))
            );
            
        }

        public void OnLoadCompleted() { }
        public void PreInitPatching() { }

        public void Dispose()
        {
            harmony.UnpatchAll();
            harmony = null;
        }


        public static bool IsValidTargetPrefix(Item item, Character character, AIObjectiveRepairItem __instance, ref bool __result)
{
    DebugConsole.NewMessage("Check");
    
    if (item == null) 
    { 
        DebugConsole.NewMessage("Check1");
        __result = false; 
        return false; 
    }
    if (item.IgnoreByAI(character)) 
    { 
        DebugConsole.NewMessage("Check2");
        __result = false; 
        return false; 
    }
    if (!item.IsInteractable(character)) 
    { 
        DebugConsole.NewMessage("Check3");
        __result = false; 
        return false; 
    }
    if (item.IsFullCondition) 
    { 
        DebugConsole.NewMessage("Check4");
        DebugConsole.NewMessage($"Item Condition: {item.Condition}, Max Condition: {item.MaxCondition}");
        __result = false; 
        return false; 
    }
    if (item.Submarine == null || character.Submarine == null) 
    { 
        DebugConsole.NewMessage("Check5");
        __result = false; 
        return false; 
    }
    if (item.IsClaimedByBallastFlora) 
    { 
        DebugConsole.NewMessage("Check6");
        __result = false; 
        return false; 
    } 
    if (character.IsOnPlayerTeam && item.Submarine.Info.IsOutpost) 
    { 
        DebugConsole.NewMessage("Check7");
        __result = false; 
        return false; 
    }
    if (!character.Submarine.IsEntityFoundOnThisSub(item, includingConnectedSubs: true)) 
    { 
        DebugConsole.NewMessage("Check8");
        __result = false; 
        return false; 
    }
    
    // Check for RepairableDieHard component
    if (item.GetComponent<RepairableDieHard>() != null) 
    { 
        DebugConsole.NewMessage("Check9");
        __result = true; 
        return true; 
    }

    // If the item does not have Repairables or RepairableDieHard, return false
    if (item.Repairables.None()) 
    { 
        DebugConsole.NewMessage("Check10");
        __result = false; 
        return false; 
    }

    __result = true; // Set __result to true if all checks pass
    return false;
}


    }
}*/