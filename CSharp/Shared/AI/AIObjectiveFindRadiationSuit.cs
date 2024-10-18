using Barotrauma.Extensions;
using Barotrauma.Items.Components;
using System.Collections.Generic;
using System.Linq;
using Barotrauma;

namespace BarotraumaDieHard.AI
{
    class AIObjectiveFindAndEquipRadiationSuit : AIObjective
    {
        public override Identifier Identifier { get; set; } = "Find and Equip Radiation Suit".ToIdentifier();
        public override string DebugTag => $"{Identifier} ({gearTag})";
        public override bool ForceRun => true;
        public override bool AbandonWhenCannotCompleteSubObjectives => false;
        public override bool AllowWhileHandcuffed => false;

        private readonly Identifier gearTag;
        private Item targetItem;
        private AIObjectiveGetItem getRadiationGear;

        public AIObjectiveFindAndEquipRadiationSuit(Character character, AIObjectiveManager objectiveManager, float priorityModifier = 1) 
            : base(character, objectiveManager, priorityModifier)
        {
            Priority = 100f;
            gearTag = TagsDieHard.RadiationGear;
        }

        public override bool CheckObjectiveSpecific() =>
            targetItem != null && character.HasEquippedItem(targetItem, slotType: InvSlotType.OuterClothes | InvSlotType.InnerClothes | InvSlotType.Head);

        public override void Act(float deltaTime)
        {
            // Check if the character already has a radiation suit equipped
            targetItem = character.Inventory.FindItem(it => it.HasTag(gearTag), true);

            // If a target item is not found, initiate finding logic
            if (targetItem == null)
            {
                // Check if the character is on the player team for dialogue
                if (character.IsOnPlayerTeam)
                {
                    character.Speak(TextManager.Get("dialog.bots.findradiationsuit").Value, null, 0.0f, "getradiationgear".ToIdentifier(), 30.0f);
                }

                // Add logic to find a suitable radiation suit from other sources
                var findRadiationGearObjective = new AIObjectiveGetItem(character, gearTag, objectiveManager, equip: true)
                {
                    AllowStealing = true,  // Adjust based on your game logic
                    AllowDangerousPressure = true,
                    EquipSlotType = InvSlotType.OuterClothes | InvSlotType.InnerClothes | InvSlotType.Head,
                    Wear = true
                };

                TryAddSubObjective(ref getRadiationGear, () =>
                {
                    // If no targetItem is found, and the character is on the player team, prompt additional dialogue
                    if (targetItem == null && character.IsOnPlayerTeam)
                    {
                        character.Speak(TextManager.Get("dialog.bots.findradiationsuit").Value, null, 0.0f, "getRadiationGear".ToIdentifier(), 30.0f);
                    }

                    // Create the item retrieval objective
                    var getItemObjective = new AIObjectiveGetItem(character, gearTag, objectiveManager, equip: true)
                    {
                        AllowStealing = HumanAIController.NeedsDivingGear(character.CurrentHull, out _),
                        AllowToFindDivingGear = false,
                        AllowDangerousPressure = true,
                        EquipSlotType = InvSlotType.OuterClothes | InvSlotType.InnerClothes | InvSlotType.Head,
                        Wear = true
                    };

                    return getItemObjective;
                }, 
                onAbandon: () =>
                {
                    character.Speak(TextManager.Get("dialog.bots.findradiationsuit.failure").Value, null, 0.0f, "getRadiationGearfaliure".ToIdentifier(), 30.0f);
                    Abandon = true;
                },
                onCompleted: () =>
                {
                    // Re-check the character's inventory to ensure the item has been equipped
                    targetItem = character.Inventory.FindItem(it => it.HasTag(gearTag), true);
                    
                    // If the item is now found, mark the sub-objective as completed
                    if (targetItem != null)
                    {
                        RemoveSubObjective(ref getRadiationGear);
                        
                        // Check if the item is radiation gear and ensure it's properly handled
                        if (gearTag == TagsDieHard.RadiationGear && HumanAIController.HasItem(character, TagsDieHard.RadiationGear, out IEnumerable<Item> masks, requireEquipped: true))
                        {
                            foreach (Item mask in masks)
                            {
                                if (mask != targetItem)
                                {
                                    // If multiple items, move extra masks to inventory
                                    character.Inventory.TryPutItem(mask, character, CharacterInventory.AnySlot);
                                }
                            }
                        }

                        // After the gear is equipped, dissolve the order
                        Abandon = true;
                    }
                });
            }
            else
            {
                // If the item is already equipped, remove the sub-objective and complete the order
                RemoveSubObjective(ref getRadiationGear);
                Abandon = true;  // Mark the order as complete
            }
        }





    }
}
