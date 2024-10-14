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
    class RepairableDieHard  : IAssemblyPlugin
    {


        public Harmony harmony;
        
        
        public void Initialize()
        {
            harmony = new Harmony("RepairableDieHard");

            harmony.Patch(
                original: typeof(Repairable).GetMethod("CheckCharacterSuccess"),
                prefix: new HarmonyMethod(typeof(RepairableDieHard).GetMethod(nameof(CheckCharacterSuccessPrefix)))
            );
            
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
            
            if (character == null) { return false; }

            if (__instance.statusEffectLists == null) { return true; }

            if (bestRepairItem != null && bestRepairItem.Prefab.CannotRepairFail) { return true; }

            // unpowered (electrical) items can be repaired without a risk of electrical shock
            if (__instance.RequiredSkills.Any(s => s != null && s.Identifier == "electrical"))
            {
                if (__instance.item.GetComponent<Reactor>() is Reactor reactor)
                {
                    if (MathUtils.NearlyEqual(reactor.CurrPowerConsumption, 0.0f, 0.1f)) { return true; }
                }
                else if (__instance.item.GetComponent<Powered>() is Powered powered && powered.Voltage < 0.1f) 
                {
                    return true;
                }
                else if (__instance.item.Condition == 0f) // have to have this. Otherwise completely brokedn device always shock players no matter powered or not.
                {
                    return true;
                }
            }

            // powered device will surely shock the repairer
            if (__instance.item.GetComponent<Reactor>() is Reactor reactorPowered && !MathUtils.NearlyEqual(reactorPowered.CurrPowerConsumption, 0.0f, 0.1f)) 
            {
                character.CharacterHealth.ApplyAffliction(character.AnimController.GetLimb(LimbType.LeftHand), AfflictionPrefab.Prefabs["burn"].Instantiate(10f));
                character.CharacterHealth.ApplyAffliction(character.AnimController.GetLimb(LimbType.RightHand), AfflictionPrefab.Prefabs["burn"].Instantiate(10f));
                character.SetStun(3f);
                return false; // Powered reactor will shock
            }
            else if (__instance.item.GetComponent<Powered>() is Powered poweredDevice && poweredDevice.Voltage >= 0.1f && !__instance.item.HasTag("battery")) // Exclude the battery since completely broken device will set voltage as 1. Battery cannot be unpowered. Need this for it be able to be fixed.
            {
                DebugConsole.NewMessage(poweredDevice.Voltage.ToString());
                character.CharacterHealth.ApplyAffliction(character.AnimController.GetLimb(LimbType.LeftHand), AfflictionPrefab.Prefabs["burn"].Instantiate(10f));
                character.CharacterHealth.ApplyAffliction(character.AnimController.GetLimb(LimbType.RightHand), AfflictionPrefab.Prefabs["burn"].Instantiate(10f));
                character.SetStun(3f);
                return false; // Powered device will shock
            }
            //DebugConsole.NewMessage(__instance.item.GetComponent<Powered>().Voltage.ToString());
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
            return success;

            return false;
        }


    }
}