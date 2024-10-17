using Barotrauma.Extensions;
using Barotrauma.Items.Components;
using System.Collections.Generic;
using System.Linq;
using Barotrauma;

namespace BarotraumaDieHard.AI
{
    class AIObjectiveFindAndEquipRadiationSuit : AIObjective
    {
        public override Identifier Identifier { get; set; } = "testorder".ToIdentifier();
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
    
    DebugConsole.NewMessage("Act");

    // Try to find the radiation suit in the character's inventory
    targetItem = character.Inventory.FindItem(it => it.HasTag(gearTag), true);

    
    // If a target item is still not found
    if (targetItem == null)
    {
        // Check if the character is on the player team for dialogue
        if (character.IsOnPlayerTeam)
        {
            character.Speak(TextManager.Get("DialogGetRadiationGear").Value, null, 0.0f, "getradiationgear".ToIdentifier(), 30.0f);
        }

        // Add logic to find a suitable radiation suit (or diving gear) from other sources
        var findRadiationGearObjective = new AIObjectiveGetItem(character, gearTag, objectiveManager, equip: true)
        {
            AllowStealing = true, // Adjust based on your game logic
            AllowDangerousPressure = true,
            EquipSlotType = InvSlotType.OuterClothes | InvSlotType.InnerClothes | InvSlotType.Head,
            Wear = true
        };

        TryAddSubObjective(ref getRadiationGear, () =>
                {
                    if (targetItem == null && character.IsOnPlayerTeam)
                    {
                        character.Speak(TextManager.Get("DialogGetDivingGear").Value, null, 0.0f, "getRadiationGear".ToIdentifier(), 30.0f);
                    }
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
                    
                    Abandon = true;
                },
                onCompleted: () =>
                {
                    RemoveSubObjective(ref getRadiationGear);
                    if (gearTag == TagsDieHard.RadiationGear && HumanAIController.HasItem(character, TagsDieHard.RadiationGear, out IEnumerable<Item> masks, requireEquipped: true))
                    {
                        foreach (Item mask in masks)
                        {
                            if (mask != targetItem)
                            {
                                character.Inventory.TryPutItem(mask, character, CharacterInventory.AnySlot);
                            }
                        }
                    }
                });
    }
    
}


    }
}
