using Barotrauma;
using Barotrauma.Items.Components;
using FarseerPhysics;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

using HarmonyLib;


using System.Globalization;
using System.Reflection;// for bindingflags
using HullModNamespace;

namespace GapMod
{

    class GapMod : IAssemblyPlugin
    {
        private Harmony harmony;

    
        public void Initialize()
        {
            harmony = new Harmony("GapMod");

            

            var originalUpdateOxygen = typeof(Gap).GetMethod("UpdateOxygen", BindingFlags.NonPublic | BindingFlags.Instance);
            var prefixUpdateOxygen = new HarmonyMethod(typeof(GapMod).GetMethod(nameof(UpdateOxygenPrefix), BindingFlags.Public | BindingFlags.Static));
            harmony.Patch(originalUpdateOxygen, prefixUpdateOxygen, null);

            
        }

        public void OnLoadCompleted() { }
        public void PreInitPatching() { }
        
        public void Dispose()
        {
            harmony.UnpatchAll();
            harmony = null;
            
        }
        private static float updateTimer = 0.0f;
        private static float updateInterval = 1f;
        private const float TemperatureExchangeSpeed = 1f;

        public static bool UpdateOxygenPrefix(Gap __instance, Hull hull1, Hull hull2, float deltaTime)
        {
            updateTimer += deltaTime;
            if (updateTimer > updateInterval) {
                
            Gap _ = __instance;

            if (hull1 == null || hull2 == null) { return false; }

            if (_.IsHorizontal)
            {
                // If the water level is above the gap, oxygen doesn't circulate
                if (Math.Max(hull1.WorldSurface + hull1.WaveY[hull1.WaveY.Length - 1], hull2.WorldSurface + hull2.WaveY[0]) > _.WorldRect.Y) { return false; }
            }

            float totalVolume = hull1.Volume + hull2.Volume;


            float totalOxygen = hull1.Oxygen + hull2.Oxygen;
            
            float deltaOxygen = (totalOxygen * hull1.Volume / totalVolume) - hull1.Oxygen;
            deltaOxygen = MathHelper.Clamp(deltaOxygen, -Hull.OxygenDistributionSpeed * deltaTime, Hull.OxygenDistributionSpeed * deltaTime);

            hull1.Oxygen += deltaOxygen;
            hull2.Oxygen -= deltaOxygen;

            // Temperature exchange
            float TemperatureDifference = HullMod.GetGas(hull1, "Temperature") - HullMod.GetGas(hull2, "Temperature");
            
            if (Math.Abs(TemperatureDifference) > 0.2f)
            {
                float tempreatureExchangeAmount = TemperatureDifference * TemperatureExchangeSpeed;

                HullMod.AddGas(hull1, "Temperature", -tempreatureExchangeAmount, deltaTime);
                HullMod.AddGas(hull2, "Temperature", tempreatureExchangeAmount, deltaTime);
            }


            ExchangeGas(hull1, hull2, "CO2", deltaTime);
            ExchangeGas(hull1, hull2, "CO", deltaTime);
            ExchangeGas(hull1, hull2, "Chlorine", deltaTime);
            updateTimer = 0.0f;}
            //ExchangeGas(hull1, hull2, "NobleGas", deltaTime);
            return false;
        }


        public static void ExchangeGas(Hull hull1, Hull hull2, string gasType, float deltaTime)
        {
            if (hull1 == null || hull2 == null) return;

            
            float gasInHull1 = HullMod.GetGas(hull1, gasType);
            float gasInHull2 = HullMod.GetGas(hull2, gasType);

            if (gasInHull1 < 0.002f && gasInHull2 < 0.002f) return;

            float totalVolume = hull1.Volume + hull2.Volume;

            float totalGas = gasInHull1 + gasInHull2;
            float deltaGas = (totalGas * hull1.Volume / totalVolume) - gasInHull1;
            deltaGas = MathHelper.Clamp(deltaGas, -Hull.OxygenDistributionSpeed * deltaTime, Hull.OxygenDistributionSpeed * deltaTime);

            HullMod.AddGas(hull1, gasType, deltaGas, deltaTime);
            HullMod.AddGas(hull2, gasType, -deltaGas, deltaTime);
        }

    }
}
