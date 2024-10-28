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
    partial class RepairableDieHard  : IAssemblyPlugin
    {


        public Harmony harmony;
        
        
        public void Initialize()
        {
            harmony = new Harmony("RepairableDieHard");

            harmony.Patch(
                original: typeof(Repairable).GetMethod("CheckCharacterSuccess"),
                prefix: new HarmonyMethod(typeof(RepairableDieHard).GetMethod(nameof(CheckCharacterSuccessPrefix)))
            );

            harmony.Patch(
                original: typeof(Repairable).GetMethod("Update"),
                prefix: new HarmonyMethod(typeof(RepairableDieHard).GetMethod(nameof(UpdatePostfix)))
            );

#if CLIENT
            harmony.Patch
            (
                original: typeof(Repairable).GetMethod("DrawHUD"),
                prefix: new HarmonyMethod(typeof(RepairableDieHard).GetMethod(nameof(DrawHUDPrefix)))
            );
#endif
            
        }

        public void OnLoadCompleted() { }
        public void PreInitPatching() { }

        public void Dispose()
        {
            harmony.UnpatchAll();
            harmony = null;
        }


        public static bool CheckCharacterSuccessPrefix(Character character, Item bestRepairItem, Repairable __instance, ref bool __result)
        {
            
            if (character == null) { __result = false; return false; }

            if (__instance.statusEffectLists == null) { __result = true; return false; }

            if (bestRepairItem != null && bestRepairItem.Prefab.CannotRepairFail) { __result = true; return false; }
            
            // unpowered (electrical) items can be repaired without a risk of electrical shock
            // if (__instance.RequiredSkills.Any(s => s != null && s.Identifier == "electrical")). modding: inlcuding all powered items no matter requires e skill or other skills.
            


                if (__instance.item.GetComponent<PowerContainer>() is PowerContainer powerContainer) 
                {
                    if (powerContainer.powerOut.Grid == null && powerContainer.powerIn.Grid == null)
                    {
                        __result = true;
                        return false;
                    }
                    if (powerContainer.powerOut.Grid.Load > 0 || powerContainer.powerIn.Grid.Load > 0)
                    {
                        __instance.ApplyStatusEffects(ActionType.OnFailure, 1.0f, character);
                        __result = false;
                        return false;
                    }

                    __result = true;
                    return false;

                }
                
                if (__instance.item.GetComponent<Reactor>() is Reactor reactor)
                {
                    if (MathUtils.NearlyEqual(reactor.CurrPowerConsumption, 0.0f, 0.1f)) { __result = true; return false; }
                }
                else if (__instance.item.GetComponent<Powered>() is Powered powered) 
                {
                    if (powered == null) // Check for devices that don't have powered component.
                    {
                        __result = true;
                        return false;
                    }
                    if (powered.powerIn == null || powered.powerIn.Grid == null) // Check for broken devices. // The first null check is necessary because door like items may have inherited Powered class but doesn't have any 'powrIn'.
                    {
                        __result = true;
                        return false;
                    }
                    else if (powered.powerIn.Grid.Load <= 0f)
                    {
                        __result = true;
                        return false;
                    }

                    __instance.ApplyStatusEffects(ActionType.OnFailure, 1.0f, character);
                    __result = false;
                    
                    return false;
                }

            // powered reactor will surely shock the repairer
            if (__instance.item.GetComponent<Reactor>() is Reactor reactorPowered && !MathUtils.NearlyEqual(reactorPowered.CurrPowerConsumption, 0.0f, 0.1f)) 
            {
                __instance.ApplyStatusEffects(ActionType.OnFailure, 1.0f, character);
                __result = false;
                return false; // Powered reactor will shock
            }

           
            bool success = Rand.Range(0.0f, 0.5f) < __instance.RepairDegreeOfSuccess(character, __instance.RequiredSkills);
            ActionType actionType = success ? ActionType.OnSuccess : ActionType.OnFailure;

            ApplyStatusEffectsAndCreateEntityEvent(__instance, actionType, character);
            ApplyStatusEffectsAndCreateEntityEvent(__instance, ActionType.OnUse, character);
            if (bestRepairItem != null && bestRepairItem.GetComponent<Holdable>() is Holdable holdable)
            {
                ApplyStatusEffectsAndCreateEntityEvent(holdable, actionType, character);
                ApplyStatusEffectsAndCreateEntityEvent(holdable, ActionType.OnUse, character);
            }
            static void ApplyStatusEffectsAndCreateEntityEvent(ItemComponent ic, ActionType actionType, Character character)
            {
                ic.ApplyStatusEffects(actionType, 1.0f, character);
                if (GameMain.NetworkMember is { IsServer: true } && ic.statusEffectLists != null && ic.statusEffectLists.ContainsKey(actionType))
                {
                    GameMain.NetworkMember.CreateEntityEvent(ic.Item, new Item.ApplyStatusEffectEventData(actionType, ic, character));
                }
            }
            __result = success;

            return false;
        }


        public static void UpdatePostfix(float deltaTime, Camera cam, Repairable __instance)
        {
            if (__instance.CurrentFixer == null) return;
            if (__instance.item.GetComponent<Powered>() is Powered powered) 
                {
                    if (powered == null || powered.powerIn == null || powered.powerIn.Grid == null) // Check for devices that don't have powered component.
                    {
                       return;
                    }
                    else if (powered.powerIn.Grid.Load > 0f)
                    {
                        __instance.ApplyStatusEffects(ActionType.OnFailure, 1.0f, __instance.CurrentFixer);
                        return;
                    }
                }
        }


    }
}