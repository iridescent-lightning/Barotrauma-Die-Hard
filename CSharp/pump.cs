using Barotrauma.MapCreatures.Behavior;
using Barotrauma.Networking;
using Microsoft.Xna.Framework;
using System;
using System.Globalization;
using System.Linq;

using Barotrauma.Items.Components;
using Barotrauma.Extensions;
using Barotrauma;

namespace pumpMod//todo make a structural namespace DieHard.Item.Components. namespace can't be used in elsewhere
{
    class CustomPump : Pump
    {
        private ItemContainer itemContainer; // Change to an array
		private Item motor;

        public override void OnItemLoaded()
        {
            base.OnItemLoaded();

            // Retrieve all ItemContainer components attached to the item
            itemContainer = item.GetComponent<ItemContainer>();
			
			//DebugConsole.NewMessage(motor.ToString());
            // Check if there is at least one ItemContainer
            if (itemContainer == null)
            {
                DebugConsole.LogError("No ItemContainer components found on the item.");
                return;  // Return or handle the error appropriately
            }
        }

        public CustomPump(Item item, ContentXElement element)
            : base(item, element)
        {
            // Additional initialization if necessary
            // I guess this is where to link the xml element to the code
        }

        public override void Update(float deltaTime, Camera cam)
		{
			motor = item.GetComponent<ItemContainer>().Inventory.GetItemAt(0) as Item;
			//just directly use it if only one is certain. cast it. only when update it can get.GetItemsAt will give you a list
			
			pumpSpeedLockTimer -= deltaTime;
            isActiveLockTimer -= deltaTime;

            if (!IsActive)
            {
                return;
            }

            if (motor == null || motor.Condition <= 0)
			{
				// No motor found, stop the pump
				IsActive = false;
				flowPercentage = 0.0f;
				currFlow = 0.0f;
				
				return;
			}

            currFlow = 0.0f;

            if (TargetLevel != null)
            {
                float hullPercentage = 0.0f;
                if (item.CurrentHull != null) 
                {
                    float hullWaterVolume = item.CurrentHull.WaterVolume;
                    float totalHullVolume = item.CurrentHull.Volume;
                    foreach (var linked in item.CurrentHull.linkedTo)
                    {
                        if ((linked is Hull linkedHull))
                        {
                            hullWaterVolume += linkedHull.WaterVolume;
                            totalHullVolume += linkedHull.Volume;
                        }
                    }
                    hullPercentage = hullWaterVolume / totalHullVolume * 100.0f; 
                }
                FlowPercentage = ((float)TargetLevel - hullPercentage) * 10.0f;
            }

            if (!HasPower)
            {
                return;
            }

            UpdateProjSpecific(deltaTime);

            ApplyStatusEffects(ActionType.OnActive, deltaTime);

            if (item.CurrentHull == null) { return; }      

            float powerFactor = Math.Min(currPowerConsumption <= 0.0f || MinVoltage <= 0.0f ? 1.0f : Voltage, MaxOverVoltageFactor);

            currFlow = flowPercentage / 100.0f * item.StatManager.GetAdjustedValue(ItemTalentStats.PumpMaxFlow, MaxFlow) * powerFactor;

            if (item.GetComponent<Repairable>() is { IsTinkering: true } repairable)
            {
                currFlow *= 1f + repairable.TinkeringStrength * TinkeringSpeedIncrease;
            }

            currFlow = item.StatManager.GetAdjustedValue(ItemTalentStats.PumpSpeed, currFlow);

            //less effective when in a bad condition
            currFlow *= MathHelper.Lerp(0.5f, 1.0f, item.Condition / item.MaxCondition);

            item.CurrentHull.WaterVolume += currFlow * deltaTime * Timing.FixedUpdateRate; 
            if (item.CurrentHull.WaterVolume > item.CurrentHull.Volume) { item.CurrentHull.Pressure += 30.0f * deltaTime; }


			
			if (Math.Abs(currFlow) > 0)
			{
				motor.Condition = motor.Condition - 0.001f;
			}
			

			
		}

        // Override other methods as needed
		
    }
}
