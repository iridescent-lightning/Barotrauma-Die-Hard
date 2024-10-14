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


namespace AIObjectiveManagerMod
{
    class AIObjectiveManager : IAssemblyPlugin
    {


        public Harmony harmony;
        
        
        public void Initialize()
        {
            harmony = new Harmony("AIObjectiveManagerMod");

            harmony.Patch(
                original: typeof(AIObjectiveManager).GetMethod("CreateObjective"),
                postfix: new HarmonyMethod(typeof(AIObjectiveManagerMod).GetMethod(nameof(CreateObjective)))
            );
            
        }

        public void OnLoadCompleted() { }
        public void PreInitPatching() { }

        public void Dispose()
        {
            harmony.UnpatchAll();
            harmony = null;
        }


        public static AIObjective CreateObjective(Order order, float priorityModifier, AIObjectiveManager objectiveManager, Character character)
        {
                if (order == null || order.IsDismissal) { return null; }
        AIObjective newObjective;
        switch (order.Identifier.Value.ToLowerInvariant())
        {
            default:
                    if (order.TargetItemComponent == null) { return null; }
                    if (!order.TargetItemComponent.Item.IsInteractable(character)) { return null; }
                    newObjective = new AIObjectiveOperateItem(order.TargetItemComponent, character, objectiveManager, order.Option,
                        requireEquip: false, useController: order.UseController, controller: order.ConnectedController, priorityModifier: priorityModifier)
                    {
                        IsLoop = true,
                        // Don't override unless it's an order by a player
                        Override = order.OrderGiver != null && order.OrderGiver.IsCommanding
                    };
                    if (newObjective.Abandon) { return null; }
                    break;

            
        }
        if (newObjective != null)
            {
                newObjective.Identifier = order.Identifier;
            }
            newObjective.IgnoreAtOutpost = order.IgnoreAtOutpost;
            return newObjective;
        }
    }
}