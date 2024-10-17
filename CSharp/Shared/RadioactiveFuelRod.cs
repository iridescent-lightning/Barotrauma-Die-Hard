// Useless for now
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Barotrauma.Items.Components;
using Barotrauma.Networking;
using Barotrauma.Extensions;
using Barotrauma;

namespace BarotraumaDieHard.Items//todo make a structural namespace DieHard.Item.Components. namespace can't be used in elsewhere
{



    class RadioactiveFuelRod : ItemComponent
    {
		
		public static List<Item> DangerousFuelRods = new List<Item>();
        
        public RadioactiveFuelRod(Item item, ContentXElement element)
            : base(item, element)
        {
            IsActive = true;
        }
		
		
		
        public override void Update(float deltaTime, Camera cam)
        {
            if (this.item.Condition >= item.MaxCondition) return;

            


            // Check if the item is deteriorating and there is no radiation shield
            if ((this.item.RootContainer != null && !this.item.RootContainer.HasTag("radiationshield")) || (this.item.Condition < item.MaxCondition && this.item.RootContainer == null))
            {

                if (!DangerousFuelRods.Contains(this.item))
                {
                    
                    DangerousFuelRods.Add(this.item);
                }
                else
                {
                    DangerousFuelRods.Remove(this.item);
                }


                // Get the current hull of the item
                Hull currentHull = this.item.CurrentHull;
                if (currentHull != null)
                {
                    // Iterate through each character in the game and check if they are in the same hull
                    foreach (Character character in Character.CharacterList)
                    {
                        if (character.CurrentHull == currentHull)
                        {
                            // Check if the character is wearing a radiation suit in the outer clothes slot
                            Item outerClothes = character.Inventory.GetItemInLimbSlot(InvSlotType.OuterClothes);

                            if (outerClothes == null || !outerClothes.HasTag("radiationsuit"))
                            {
                                // Apply radiation sickness to characters in the same hull
                                character.CharacterHealth.ApplyAffliction(character.AnimController.MainLimb, AfflictionPrefab.Prefabs["radiationsickness"].Instantiate(1f * deltaTime));
                            }

                            // Get items in the character's hands
                            var leftHand = character.Inventory.GetItemInLimbSlot(InvSlotType.LeftHand);
                            var rightHand = character.Inventory.GetItemInLimbSlot(InvSlotType.RightHand);

                            // Check if the character is holding the fuel rod (this.item)
                            if (leftHand == this.item || rightHand == this.item)
                            {
                                
                                // Define a variable to store the fuel rod case
                                Item fuelRodHolder = null;

                                // Check if the character is also holding a fuelRodHolder in the other hand
                                if (leftHand != null && leftHand.Prefab.Identifier == "fuelrodholder" && leftHand != this.item)
                                {
                                    fuelRodHolder = leftHand;
                                }
                                else if (rightHand != null && rightHand.Prefab.Identifier == "fuelrodholder" && rightHand != this.item)
                                {
                                    fuelRodHolder = rightHand;
                                }

                                // If a fuel rod case is found, place the fuel rod inside it
                                if (fuelRodHolder != null)
                                {
                                    // Check if the case has space and the correct slot for the fuel rod
                                    var container = fuelRodHolder.GetComponent<ItemContainer>();
                                    if (container != null && container.Inventory.CanBePut(this.item))
                                    {
                                        container.Inventory.TryPutItem(this.item, character);
                                    }
                                }
                                else
                                {
                                    // If no fuel rod case is found, drop the fuel rod
                                    if (leftHand == this.item) // If the fuel rod is in the left hand
                                    {   
                                        // Use GetLimb(LimbType.) to get limb

                                        character.CharacterHealth.ApplyAffliction(character.AnimController.GetLimb(LimbType.LeftHand), AfflictionPrefab.Prefabs["burn"].Instantiate(1500f * deltaTime));
                                        leftHand.Drop(character); // Drop the item and burn left hand
                                    }

                                    if (rightHand == this.item) // If the fuel rod is in the right hand
                                    {
                                        character.CharacterHealth.ApplyAffliction(character.AnimController.GetLimb(LimbType.RightHand), AfflictionPrefab.Prefabs["burn"].Instantiate(1500f * deltaTime));
                                        rightHand.Drop(character); // Drop the item and burn right hand
                                    }
                                }
                            }
                        }
                    }

                }
            }

            // Handle item in character's inventory (if applicable)
            if (this.item.ParentInventory is CharacterInventory)
            {
                // You can add additional logic here to manage the item in the character's inventory
            }
        }



        
    }
}
