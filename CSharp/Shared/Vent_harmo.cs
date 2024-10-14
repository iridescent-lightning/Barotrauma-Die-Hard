using System;
using System.Xml.Linq;

using Barotrauma.Items.Components;
using Barotrauma.Extensions;
using Barotrauma;
using HarmonyLib;
using HullModNamespace;
using OxygenGeneratorMod;

namespace VentModNameSpace//todo make a structural namespace DieHard.Item.Components. namespace can't be used in elsewhere
{
    class VentMod : IAssemblyPlugin
    {
        public  Harmony harmony;
		

        public void Initialize()
		{
			harmony = new Harmony("VentMod");
			
			harmony.Patch(
                original: typeof(Vent).GetMethod("Update"),
                prefix: new HarmonyMethod(typeof(VentMod).GetMethod(nameof(Update)))
            );
			
				
			}

        public void OnLoadCompleted() { }
        public void PreInitPatching() { }

        public void Dispose()
        {
            harmony.UnpatchAll();
            harmony = null;
        }
        public static float co2Flow;

        public static float CO2Flow
        {
            get { return co2Flow; }
            set { co2Flow = Math.Max(value, 0.0f); }
        }

        public static float  purifyingFlow;

        public static float PurifyingFlow
        {
            get { return purifyingFlow; }
            set { purifyingFlow = Math.Max(value, 0.0f); }
        }

        public static float heatFlow;
        public static float HeatFlow
        {
            get { return heatFlow; }
            set { heatFlow = Math.Max(value, 0.0f); }
        }
        private static float updateTimer = 0.0f;
        private static float updateInterval = 0.1f;
        public static bool Update(float deltaTime, Camera cam, Vent __instance)
        {
            updateTimer += deltaTime;
            if (updateTimer > updateInterval)
            {
            Vent _ = __instance;

            if (_.item.CurrentHull == null || _.item.InWater) { return false; }

            if (_.oxygenFlow > 0.0f)
            {
                _.ApplyStatusEffects(ActionType.OnActive, deltaTime);
            }

            _.item.CurrentHull.Oxygen += _.oxygenFlow * deltaTime;
            _.OxygenFlow -= deltaTime * 1000.0f;

            HullMod.AddGas(_.item.CurrentHull, "CO2", -CO2Flow * 80f, deltaTime); //higher co to make sure the vent can clear the room
            HullMod.AddGas(_.item.CurrentHull, "CO", -PurifyingFlow * 30f, deltaTime);
            HullMod.AddGas(_.item.CurrentHull, "Chlorine", -PurifyingFlow * 10f, deltaTime); //There is no CL
            if (HullMod.GetGas(_.item.CurrentHull, "Temperature") < 300f)
            {
                HullMod.AddGas(_.item.CurrentHull, "Temperature", HeatFlow  * 10f, deltaTime);
            }
            updateTimer = 0.0f;
            }
            return false;
        }
    }
}
