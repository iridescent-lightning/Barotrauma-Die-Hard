using System;
using System.Reflection;
using System.Collections.Generic;
using Barotrauma;
using Barotrauma.Items.Components;
using HarmonyLib;
using Microsoft.Xna.Framework;
using System.Linq;

namespace ReactorMod
{
    class ReactorMod : IAssemblyPlugin
    {
        public Harmony harmony;
        private static ItemContainer[] itemContainers; // Change to an array
        private static ItemContainer secondItemContainer;
        private static Item coolant;

        private static float escapedTime;
        private static float updateTimer = 1.0f;

        
        public void Initialize()
        {
            harmony = new Harmony("reactormod");

            harmony.Patch(
                original: typeof(Reactor).GetMethod("Update"),
                postfix: new HarmonyMethod(typeof(ReactorMod).GetMethod(nameof(UpdatePostfix)))
            );
            harmony.Patch(
                original: typeof(Reactor).GetMethod("OnMapLoaded"),
                postfix: new HarmonyMethod(typeof(ReactorMod).GetMethod(nameof(OnMapLoaded)))
            );
        }

        public void OnLoadCompleted() { }
        public void PreInitPatching() { }

        public void Dispose()
        {
            harmony.UnpatchAll();
            harmony = null;
        }

        public static void OnMapLoaded(Reactor __instance)
        {
            // clear the list of ItemContainers
            //itemContainers = null;
            
            itemContainers = __instance.item.GetComponents<ItemContainer>()
                                    .OfType<ItemContainer>()
                                    .ToArray();

            // Check if there is at least one ItemContainer
            if (itemContainers == null || itemContainers.Length < 2)
            {
                DebugConsole.LogError("No ItemContainer components found on the item.");
                return;  // Return or handle the error appropriately
            }

            // Get the coolant from the second ItemContainer
            secondItemContainer = itemContainers[1];
            DebugConsole.NewMessage("Second ItemContainer found: " + secondItemContainer.ToString());
        }

        public static void UpdatePostfix(float deltaTime, Camera cam, Reactor __instance)
        {
            //escapedTime += deltaTime;
                //if (escapedTime > updateTimer){
                    
                // Retrieve all ItemContainer components attached to the item
                if (secondItemContainer == null)
                {
                    DebugConsole.LogError("No ItemContainer components found on the item.");
                    return;  // Return or handle the error appropriately
                }
                coolant = secondItemContainer.Inventory.GetItemAt(0);

                // Check the condition of the coolant and the temperature of the reactor
                if (coolant != null && coolant.Condition <= 0 && __instance.Temperature > 10f)
                {
                    __instance.Item.Condition -= 3f;
                }
                else if (coolant == null && __instance.Temperature > 10f)
                {
                    __instance.Item.Condition -= 3f;
                }
                else if (__instance.item.InPlayerSubmarine && __instance.Temperature > 10f)
                {
                    coolant.Condition -= 0.05f;
                }

                // Trigger an action if the reactor's condition is critical
                if (__instance.Item.Condition < 1f && __instance.Item.Condition > 0f && __instance.Temperature > 10f)
                {
                    Entity.Spawner.AddItemToSpawnQueue(ItemPrefab.GetItemPrefab("reactorcsexplosionhelper"), __instance.Item.WorldPosition);
                }
                //escapedTime = 0f;
            
        }
    }
}
