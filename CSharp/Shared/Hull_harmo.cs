using Barotrauma.Networking;
using FarseerPhysics;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using Barotrauma.MapCreatures.Behavior;
using Barotrauma.Items.Components;
using Barotrauma.Extensions;

using HarmonyLib;
using Barotrauma;


namespace BarotraumaDieHard
{
    class HullMod : IAssemblyPlugin
    {
        private Harmony harmony;


        
    
        public void Initialize()
        {
            

            harmony = new Harmony("HullMod");
            var originalConstructor = typeof(Hull).GetConstructor(new[] { typeof(Rectangle), typeof(Submarine), typeof(ushort) });
            var postfix = new HarmonyMethod(typeof(HullMod).GetMethod(nameof(HullMod.HullConstructorPostfix)));
            harmony.Patch(originalConstructor, null, postfix);

            

            harmony.Patch(
                original: AccessTools.Method(typeof(Hull), "Update"),
                postfix: new HarmonyMethod(typeof(HullMod).GetMethod("Update"))
            );

            harmony.Patch(
                original: AccessTools.Method(typeof(Hull), "ApplyFlowForces"),
                prefix: new HarmonyMethod(typeof(HullMod).GetMethod("ApplyFlowForces"))
            );
            
            
        }

        public void OnLoadCompleted() { }
        public void PreInitPatching() 
        {

        }

        public void Dispose()
        {
            harmony.UnpatchAll();
            harmony = null;
            
        }

        public static bool ApplyFlowForces(Hull __instance, float deltaTime, Item item)
        {
            if (item.body.Mass <= 0.0f)
            {
                return false;
            }
            foreach (var gap in __instance.ConnectedGaps.Where(gap => gap.Open > 0))
            {
                var distance = MathHelper.Max(Vector2.DistanceSquared(item.Position, gap.Position) / 1000, 1f);
                Vector2 force = (gap.LerpedFlowForce / (distance / 15)) * deltaTime;
                if (force.LengthSquared() > 0.01f)
                {
                    item.body.ApplyForce(force * 100);
                }
            }
            return false;
        }

        //this is used to assign gas from the GasInfo class to each hull
        public static Dictionary<Hull, GasInfo> gasMap = new Dictionary<Hull, GasInfo>();

        
        public static void HullConstructorPostfix(Hull __instance)
    {
        
        DebugConsole.Log("Hull constructor has been patched.");
        float volume = __instance.Volume;

        //init each gas for each hull here
        GasInfo gasInfo = new GasInfo
            {
                Temperature = 300.0f, // Example value
                CO2 = 0f,
                CO = 0f,
                Nitrogen = Rand.Range(0f, 100.0f),
                NobleGas = Rand.Range(0f, 100.0f),
                Chlorine = 0f,
                PressurizedAir = 0f,
                // Pressurized Air
                GapOpenSum = 0.0f
            };
            gasMap[__instance] = gasInfo;
        //__instance.ToxicGasPercentage = volume <= 0.0f ? 100.0f : __instance.toxicGas / volume * 100.0f;
    }
        public static void Update(Hull __instance, float deltaTime, Camera cam)
        {
            Hull hull = __instance;
            if (hull == null) { return; }

            // Fire
            if (hull.FireSources.Count > 0)
            {
                AddGas(hull, "Temperature", 1f, deltaTime);
                AddGas(hull, "CO2", 20f, deltaTime);
                AddGas(hull, "CO", 10f, deltaTime);
            }

            // Temperature
            if (GetGas(hull, "Temperature") > 273.15f && hull.WaterPercentage > 0.3f)
            {
                AddGas(hull, "Temperature", -0.3f, deltaTime);
            }
            else if (GetGas(hull, "Temperature") > 273.15f)
            {
                AddGas(hull, "Temperature", -0.025f, deltaTime);
            }
            else if (GetGas(hull, "Temperature") > 318.15f && hull.WaterPercentage > 0.3f)
            {
                AddGas(hull, "Temperature", -5f, deltaTime);
            }

            // Calculate and store GapOpenSum
            float gapOpenSum = hull.ConnectedGaps
            .Where(g => g.linkedTo.Count == 1 && !g.IsHidden)
            .Sum(g => g.Open);
            

            // Store the GapOpenSum in the GasInfo struct for this hull. Currently has no use.
            // Maybe we need something else to access the value in the future.
            if (gasMap.TryGetValue(hull, out GasInfo gasInfo))
            {
                gasInfo.GapOpenSum = gapOpenSum;
                gasMap[hull] = gasInfo; // Update the dictionary with the modified struct
            }
            // Check if the hull is breached 
            if (gapOpenSum > 0.1f)
{
    DebugConsole.NewMessage($"Hull {hull.ID} is breached. GapOpenSum: {gapOpenSum}");
    
    // Apply a reduction of pressurized air as it escapes through the gap
    AddGas(hull, "PressurizedAir", -500f * gapOpenSum, deltaTime);

    // Check if there's pressurized air in the hull
    float pressurizedAirAmount = GetGas(hull, "PressurizedAir");
    
    if (pressurizedAirAmount > 0.0f)
    {
        // Calculate the force based on pressurized air amount and the gap open sum
        float forceMultiplier = pressurizedAirAmount * gapOpenSum;

        // Ensure the force is applied in a direction pushing the water out of the hull
        Vector2 flowDirection = (hull.Position - flowTargetHull.Position); // Direction from hull to target (gap direction)
        flowDirection.Normalize(); // Normalize to get direction vector
        Vector2 flowForce = flowDirection * forceMultiplier * deltaTime;

        // Apply the calculated flow force to the hull
        hull1.WaterVolume -= Math.Min(flowForce.Length(), hull1.WaterVolume); // Ensure water volume doesn't go negative
        hull1.Pressure -= Math.Min(flowForce.Length() / 10f, hull1.Pressure); // Pressure drop due to air escaping
        flowTargetHull.WaterVolume += flowForce.Length(); // Water flowing into the target hull due to pressurized air
    }
}


            
        }
        

        // these methods are used to get and set gas for each hull. They interact with the GasInfo class but are called from Hull class.
        public static float GetGas(Hull hull, string gasType)
        {
            if (gasMap.TryGetValue(hull, out GasInfo gasInfo))
            {
                return gasInfo.GetGasAmount(gasType);
            }
            return 0.0f; // Default value if not set
        }

        public static void SetGas(Hull hull, string gasType, float value)
        {
            if (gasMap.TryGetValue(hull, out GasInfo gasInfo))
            {
                gasInfo.SetGasAmount(gasType, value);
                gasMap[hull] = gasInfo;
            }
        }

        public static void AddGas(Hull hull, string gasType, float amount, float deltaTime)
        {
            if (gasMap.TryGetValue(hull, out GasInfo gasInfo))
            {
                // Apply delta time to the amount
                float adjustedAmount = amount * deltaTime;

                // Get current gas amount and calculate new amount
                float currentGasAmount = gasInfo.GetGasAmount(gasType);
                float newGasAmount = currentGasAmount + adjustedAmount;

                // Ensure the new gas amount does not go below zero
                newGasAmount = Math.Max(newGasAmount, 0.0f);

                // Set the new gas amount
                gasInfo.SetGasAmount(gasType, newGasAmount);

                // Update the gas information in the map
                gasMap[hull] = gasInfo;
            }
        }



        // This is the GasInfo class. It is used to store gas information for each hull.
        public struct GasInfo
        {
            public float Temperature;
            public float CO2;
            public float CO;
            public float Nitrogen;
            public float Chlorine;
            public float NobleGas;
            public float PressurizedAir;
            public float GapOpenSum;

            public float GetGasAmount(string gasType)
            {
                switch (gasType)
                {
                    case "Temperature":
                        return Temperature;
                    case "CO2":
                        return CO2;
                    case "CO":
                        return CO;
                    case "Nitrogen":
                        return Nitrogen;
                    case "Chlorine":
                        return Chlorine;
                    case "NobleGas":
                        return NobleGas;
                    case "PressurizedAir":
                        return PressurizedAir;
                    default:
                        return 0.0f;
                }
            }

            public void SetGasAmount(string gasType, float value)
            {
                switch (gasType)
                {
                    case "Temperature":
                        Temperature = value;
                        break;
                    case "CO2":
                        CO2 = value;
                        break;
                    case "CO":
                        CO = value;
                        break;
                    case "Nitrogen":
                        Nitrogen = value;
                        break;
                    case "Chlorine":
                        Chlorine = value;
                        break;
                    case "NobleGas":
                        NobleGas = value;
                        break;
                    case "PressurizedAir":
                        PressurizedAir = value;
                        break;
                }
            }

            public void AddGas(string gasType, float amount)
            {
                switch (gasType)
                {
                    case "Temperature":
                        Temperature += amount;
                        break;
                    case "CO2":
                        CO2 += amount;
                        break;
                    case "CO":
                        CO += amount;
                        break;
                    case "Nitrogen":
                        Nitrogen += amount;
                        break;
                    case "Chlorine":
                        Chlorine += amount;
                        break;
                    case "NobleGas":
                        NobleGas += amount;
                        break;
                    case "PressurizedAir":
                        PressurizedAir += amount;
                        break;
                }
            }
        }
    }
}
