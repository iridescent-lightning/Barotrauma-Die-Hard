// This class is patched to allow bots to swap weldingtoolequipment from bag to hands in case of fixing leaks.
using Barotrauma.Extensions;
using Barotrauma.Items.Components;
using FarseerPhysics;
using Microsoft.Xna.Framework;
using System;
using System.Linq;

using HarmonyLib;
using System.Reflection;
using System.Xml.Linq;

using Barotrauma;


namespace BarotraumaDieHard
{
    class AIObjectiveFixLeakDieHard  : IAssemblyPlugin
    {


        public Harmony harmony;
        
        
        public void Initialize()
        {
            harmony = new Harmony("AIObjectiveFixLeakDieHard");

            var originalAct = typeof(AIObjectiveFixLeak).GetMethod("Act", BindingFlags.NonPublic | BindingFlags.Instance);
            var PostfixAct = typeof(AIObjectiveFixLeakDieHard).GetMethod(nameof(ActPostfix), BindingFlags.Public | BindingFlags.Static);

            harmony.Patch(originalAct, new HarmonyMethod(PostfixAct), null);


            
            
        }

        public void OnLoadCompleted() { }
        public void PreInitPatching() { }

        public void Dispose()
        {
            harmony.UnpatchAll();
            harmony = null;
        }


        public static void ActPostfix(float deltaTime, AIObjectiveFixLeak __instance)
        {
            AIObjectiveFixLeak _ = __instance;

            var BagSlotIndex = _.character.Inventory.FindLimbSlot(InvSlotType.Bag);
            var HandSlotIndex = _.character.Inventory.FindLimbSlot(InvSlotType.RightHand);
            Item itemInBag = _.character.Inventory.GetItemAt(BagSlotIndex);

            if (itemInBag != null && itemInBag.HasTag("weldingequipment"))
            {
                _.character.Inventory.TryPutItem(itemInBag, HandSlotIndex, true, false, Character.Controlled, true, true);
            }
        }
    }
}