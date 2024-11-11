using Barotrauma.MapCreatures.Behavior;
using Barotrauma.Networking;
using Microsoft.Xna.Framework;
using System;
using System.Globalization;
using System.Linq;

using Barotrauma.Items.Components;
using Barotrauma.Extensions;
using Barotrauma;
using HarmonyLib;


namespace BarotraumaDieHard
{
    class CustomPump : IAssemblyPlugin
    {
        public  Harmony harmony;
		private static Item motor;

        public void Initialize()
		{
			harmony = new Harmony("CustomPump");
			
			harmony.Patch(
                original: typeof(Pump).GetMethod("Update"),
                prefix: new HarmonyMethod(typeof(CustomPump).GetMethod(nameof(Update)))
            );
			
				
			}

        public void OnLoadCompleted() { }
        public void PreInitPatching() { }

        public void Dispose()
        {
            harmony.UnpatchAll();
            harmony = null;
        }

        public static bool Update(float deltaTime, Camera cam, Pump __instance)
		{
			Pump _ = __instance;
            // add this tag so no other pump without container won't crash the game.
            if (_.item.HasTag("cspump"))
            {
			    motor = _.item.GetComponent<ItemContainer>().Inventory.GetItemAt(0) as Item;
            }


            _.pumpSpeedLockTimer -= deltaTime;
            _.isActiveLockTimer -= deltaTime;

            if (!_.IsActive)
            {
                return false;
            }

            if ((motor == null || motor.Condition <= 0) && _.item.HasTag("cspump"))
			{
				// No motor found, stop the pump
				_.IsActive = false;
				_.flowPercentage = 0.0f;
				_.currFlow = 0.0f;
				
				return false;
			}
            
            // Pressurized Air feature.
            if (_.item.CurrentHull != null && _.item.CurrentHull.IsWetRoom)
            {
                float subamrineDepth = _.item.Submarine.RealWorldDepth;
                float requiredAirPressure = _.item.Submarine.RealWorldDepth * 4f;

                float currentPressurizedAir = HullMod.GetGas(_.item.CurrentHull, "PressurizedAir");

                if (currentPressurizedAir < requiredAirPressure)
                {
                    HullMod.AddGas(_.item.CurrentHull, "PressurizedAir", 300f, deltaTime);
                }
            }
            
            

            _.currFlow = 0.0f;

            if (_.TargetLevel != null)
            {
                float hullPercentage = 0.0f;
                if (_.item.CurrentHull != null) 
                {
                    float hullWaterVolume = _.item.CurrentHull.WaterVolume;
                    float totalHullVolume = _.item.CurrentHull.Volume;
                    foreach (var linked in _.item.CurrentHull.linkedTo)
                    {
                        if ((linked is Hull linkedHull))
                        {
                            hullWaterVolume += linkedHull.WaterVolume;
                            totalHullVolume += linkedHull.Volume;
                        }
                    }
                    hullPercentage = hullWaterVolume / totalHullVolume * 100.0f; 
                }
                _.FlowPercentage = ((float)_.TargetLevel - hullPercentage) * 10.0f;
            }

            if (!_.HasPower)
            {
                return false;
            }

            _.UpdateProjSpecific(deltaTime);

            _.ApplyStatusEffects(ActionType.OnActive, deltaTime);

            if (_.item.CurrentHull == null) { return false; }      

            float powerFactor = Math.Min(_.currPowerConsumption <= 0.0f || _.MinVoltage <= 0.0f ? 1.0f : _.Voltage, Pump.MaxOverVoltageFactor);

            _.currFlow = _.flowPercentage / 100.0f * _.item.StatManager.GetAdjustedValueMultiplicative(ItemTalentStats.PumpMaxFlow, _.MaxFlow) * powerFactor;

            if (_.item.GetComponent<Repairable>() is { IsTinkering: true } repairable)
            {
                _.currFlow *= 1f + repairable.TinkeringStrength * Pump.TinkeringSpeedIncrease;
            }

            _.currFlow = _.item.StatManager.GetAdjustedValueMultiplicative(ItemTalentStats.PumpSpeed, _.currFlow);

            //less effective when in a bad condition
            _.currFlow *= MathHelper.Lerp(0.5f, 1.0f, _.item.Condition / _.item.MaxCondition);

            _.item.CurrentHull.WaterVolume += _.currFlow * deltaTime * Timing.FixedUpdateRate; 
            if (_.item.CurrentHull.WaterVolume > _.item.CurrentHull.Volume) { _.item.CurrentHull.Pressure += 30.0f * deltaTime; }


			
			if (Math.Abs(_.currFlow) > 0)
			{
				motor.Condition = motor.Condition - 0.001f;
			}
			return false;
		}
    }
}
