// Exploring. No real feature built from it.
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



namespace BarotraumaDieHard.AI
{
    class AIObjectiveManagerDieHard  : IAssemblyPlugin
    {


        public Harmony harmony;
        
        
        public void Initialize()
        {
            harmony = new Harmony("AIObjectiveManagerDieHard");

            var originalCreateObjective = typeof(AIObjectiveManager).GetMethod("CreateObjective", BindingFlags.Public | BindingFlags.Instance);
            var postfixCreateObjective = typeof(AIObjectiveManagerDieHard).GetMethod(nameof(CreateObjectivePostfix), BindingFlags.Public | BindingFlags.Static);

            harmony.Patch(originalCreateObjective, new HarmonyMethod(postfixCreateObjective), null);


           
            
        }

        public void OnLoadCompleted() { }
        public void PreInitPatching() { }

        public void Dispose()
        {
            harmony.UnpatchAll();
            harmony = null;
        }


        public static void CreateObjectivePostfix(Order order, float priorityModifier, AIObjectiveManager __instance, ref AIObjective __result)
{
    // Check if the order is to find a radiation suit
    if (order.Identifier.Value.Equals("testorder", StringComparison.OrdinalIgnoreCase))
    {
        
        // Create a new objective to find the radiation suit
        AIObjective findRadiationSuitObjective = new AIObjectiveFindAndEquipRadiationSuit(__instance.character, __instance, 100f);
        

        // Set the result to the newly created objective
        __result = findRadiationSuitObjective;

        // If necessary, you can add more logic here, such as setting up events or handling completion.
    }
}


    }
}