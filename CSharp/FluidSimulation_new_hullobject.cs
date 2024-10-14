using Barotrauma;
using Barotrauma.Extensions;
using Barotrauma.Items.Components;
using Barotrauma.MapCreatures.Behavior;
using Barotrauma.Networking;
using FarseerPhysics;
using FarseerPhysics.Dynamics;
using HarmonyLib;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;

namespace FluidSimulationMod
{

    public class HullData
{
    public Dictionary<string, float> Gasses { get; private set; }
    public float Temperature { get; set; }
    public float Size { get; set; }

    public float TotalGas 
    { 
        get { return Gasses.Values.Sum(); } 
    }

    public HullData(float initialTemperature)
    {
        Gasses = new Dictionary<string, float>();
        Temperature = initialTemperature;
    }
}


    class FluidSimulation : IAssemblyPlugin
    {
        private Harmony harmony;
        
		private string gas;
        private const float NormalTemperature = 300;
        private static List<string> listGasses = new List<string>();

        //make it public so that it can be accessed by other classes
        public static Dictionary<Hull, HullData> hullDataStorage = new Dictionary<Hull, HullData>();


        private static bool isInitialized = false;
        private static float escapedTime;

        private static readonly float updateTimer = 0.1f;


        // Constants for controlling gas and temperature distribution
        private const float GasDistributionSpeedConstant = 0.001f;
        private const float TemperatureSpeedConstant = 0.001f;
    
        public void Initialize()
        {
            harmony = new Harmony("FluidSimulation");

            

            harmony.Patch(
                original: AccessTools.Method(typeof(Submarine), "Update"),
                postfix: new HarmonyMethod(typeof(FluidSimulation).GetMethod("Update"))
            );

            
            
        }

        public void OnLoadCompleted() { }
        public void PreInitPatching() 
        {
            DefineGas("CO2");
            DefineGas("Chlorine");
        }

        public void Dispose()
        {
            harmony.UnpatchAll();
            harmony = null;
            
        }

        public void SetGasses(string gasses)
        {
            gas = gasses;
        }

        
        public static void Update(Submarine __instance, float deltaTime)
        {
            escapedTime += deltaTime;
                if (escapedTime > updateTimer){
            if (__instance != Submarine.MainSub){
                return;}
            if (!isInitialized)
            {
                isInitialized = true;
                StoreMainSubHulls();
            }
            //DebugConsole.NewMessage(escapedTime.ToString(), Color.White);
            
            foreach (KeyValuePair<Hull, HullData> entry in hullDataStorage)
            {
                Hull hull = entry.Key;
                HullData hullData = entry.Value;
                foreach (Gap gap in hull.ConnectedGaps)
                {
                        if (gap.Open <= 0) continue;
                    var linkedHulls = gap.linkedTo;
                    if (linkedHulls.Count != 2) continue;

                    // Ensure both linked objects are Hulls
                    if (!(linkedHulls[0] is Hull hull1) || !(linkedHulls[1] is Hull hull2)) continue;

                    // Avoid duplicate processing for the same pair of hulls
                    if (hullDataStorage.ContainsKey(hull1) && hullDataStorage.ContainsKey(hull2))
                    {
                        if (hull1 == hull) 
                        {
                            Simulate(hull1, hull2, gap, deltaTime);
                        }
                    }

                }
            }

            foreach (KeyValuePair<Hull, HullData> entry in hullDataStorage)
            {
                Hull hull = entry.Key;
                HullData hullData = entry.Value;

                
                // Decrease the temperature
                hullData.Temperature = Math.Clamp(hullData.Temperature - (10f/hullData.Size) * deltaTime, 273.15f, 99999999);
                //DebugConsole.NewMessage(hullData.Temperature.ToString(), Color.White);
                if (hull.FireSources.Count > 0)
                {   
                        foreach (FireSource fire in hull.FireSources)
                    {
                        Vector2 size = fire.Size;
                        float sizeFire = size.X * size.Y / 10000;
                        
                        hullData.Temperature = Math.Clamp(hullData.Temperature + (0.1f * sizeFire/hullData.Size) * deltaTime, 0, 99999999);
                        AddGas(hull, "CO2", 1f * sizeFire * deltaTime);
                        //DebugConsole.NewMessage(sizeFire.ToString(), Color.White);
                        
                    }
                }
                if (hull.WaterPercentage > 0.3f)
                {
                    hullData.Temperature = Math.Clamp(hullData.Temperature - ((0.1f * hull.WaterVolume/10000)/hullData.Size) * deltaTime, 0, 99999999);
                   
                }
                //DebugConsole.NewMessage(hullData.Temperature.ToString(), Color.White);
                
                
            }
            
            foreach (Character character in Character.CharacterList)
            {

                // Check if the character is currently in a hull
                if (character.CurrentHull != null && character.IsHuman)
                {
                    // Check if the character's current hull is one of the valid hulls
                    if (hullDataStorage.ContainsKey(character.CurrentHull))
                    {
                        
                        AddGas(character.CurrentHull, "CO2", 0.1f * deltaTime);
                        float co2Amount = GetGas(character.CurrentHull, "CO2");
                        float co2Concentration = co2Amount / hullDataStorage[character.CurrentHull].Size;

                        float chlorineAmount = GetGas(character.CurrentHull, "Chlorine");
                        float chlorineConcentration = chlorineAmount / hullDataStorage[character.CurrentHull].Size;

                    // Print the CO2 concentration for debugging
                    //DebugConsole.NewMessage($"CO2 Concentration: {co2Concentration}", Color.White);

                        if (co2Concentration > 0.4f && character.UseHullOxygen)
                        {
                            character.CharacterHealth.ApplyAffliction(character.AnimController.MainLimb, AfflictionPrefab.Prefabs["co_poisoning"].Instantiate(0.1f * deltaTime));
                            
                        }

                        if(chlorineConcentration > 0.3f && character.UseHullOxygen)
                        {
                            character.CharacterHealth.ApplyAffliction(character.AnimController.MainLimb, AfflictionPrefab.Prefabs["chlorine_poisoning"].Instantiate(0.01f * deltaTime));
                        }
                        

                        if (hullDataStorage[character.CurrentHull].Temperature < 278.15f)
                        {
                            character.CharacterHealth.ApplyAffliction(character.AnimController.MainLimb, AfflictionPrefab.Prefabs["coldwater"].Instantiate(0.01f * deltaTime));
                            
                        }
                        else if (hullDataStorage[character.CurrentHull].Temperature > 323.15f)
                        {
                            character.CharacterHealth.ApplyAffliction(character.AnimController.MainLimb, AfflictionPrefab.Prefabs["burn"].Instantiate(0.01f * deltaTime));

                            character.CharacterHealth.ApplyAffliction(character.AnimController.MainLimb, AfflictionPrefab.Prefabs["coldwater"].Instantiate(-0.05f * deltaTime));
                        }
                        if (hullDataStorage[character.CurrentHull].Temperature > 295f)
                        {
                            if (character.CharacterHealth.GetAfflictionStrength("debuff", "coldwater", true) > 0)
                            {
                            character.CharacterHealth.ApplyAffliction(character.AnimController.MainLimb, AfflictionPrefab.Prefabs["coldwater"].Instantiate(-0.03f * deltaTime));
                            }
                        }
                        // - Access hull gas data: hullDataStorage[character.CurrentHull].Gasses
                        // ... etc.
                    }
                }
            }

            //This is a hand-in. Check if any hulls in the dictionary no longer exist or are invalid. It seems quit the test in the editor doesn't remove the hulls from the dictionary. Don't know if this is the case in campaign mode.
            //Still have to use this because multipler doesn't trigger roundend.
        ValidateAndReinitializeHullDataStorage();
        escapedTime = 0.0f;}
            


        
        }

        private static void Simulate(Hull hull1, Hull hull2, Gap gap, float deltaTime)
        {
            if (!hullDataStorage.TryGetValue(hull1, out HullData hullData1))
            {
                hullData1 = new HullData(NormalTemperature);
                hullDataStorage[hull1] = hullData1;
            }

            if (!hullDataStorage.TryGetValue(hull2, out HullData hullData2))
            {
                hullData2 = new HullData(NormalTemperature);
                hullDataStorage[hull2] = hullData2;
            }

            foreach (var gasName in listGasses)
            {
                float gasHull1 = hullData1.Gasses.GetValueOrDefault(gasName, 0);
                float gasHull2 = hullData2.Gasses.GetValueOrDefault(gasName, 0);
                float totalGas = gasHull1 + gasHull2;

                float totalVolume = hull1.Volume + hull2.Volume;
                float deltaGas = (totalGas * hull1.Volume / totalVolume) - gasHull1;
                deltaGas = Clamp(deltaGas, -gap.Size * GasDistributionSpeedConstant, gap.Size * GasDistributionSpeedConstant);

                if (Math.Abs(deltaGas) > 0)
                {
                    AddGas(hull1, gasName, deltaGas * deltaTime);
                    AddGas(hull2, gasName, -deltaGas * deltaTime);
                }
            }

            float temperatureDifference = GetTemperature(hull1) - GetTemperature(hull2);
            if (Math.Abs(temperatureDifference) > 0)
            {
                
                float deltaTemperature = Clamp(temperatureDifference, -gap.Size * TemperatureSpeedConstant, gap.Size * TemperatureSpeedConstant);
                deltaTemperature = Math.Min(deltaTemperature, Math.Min(GetTemperature(hull1), GetTemperature(hull2)));

                
                    
                    float dtemp1 = deltaTemperature;
                    float dtemp2 = deltaTemperature;

                    AddTemperature(hull1, -dtemp1 * deltaTime);
                    AddTemperature(hull2, dtemp2 * deltaTime);
            }
        }

        public static void DefineGas(string gasName)
        {
            if (!listGasses.Contains(gasName))
            {
                listGasses.Add(gasName);
            }
        }

        public static void AddGas(Hull hull, string gasName, float amount)
        {
            if (!hullDataStorage.TryGetValue(hull, out HullData hullData))
            {
                hullData = new HullData(NormalTemperature);
                hullDataStorage[hull] = hullData;
            }

            if (!hullData.Gasses.ContainsKey(gasName))
            {
                hullData.Gasses[gasName] = 0;
            }

            hullData.Gasses[gasName] += amount;
        }

        public static float GetTemperature(Hull hull)
        {
            if (hullDataStorage.TryGetValue(hull, out HullData hullData))
            {
                return hullData.Temperature;
            }
            return NormalTemperature;
        }


        public static void AddTemperature(Hull hull, float amount)
        {
            if (!hullDataStorage.TryGetValue(hull, out HullData hullData))
            {
                hullData = new HullData(NormalTemperature);
                hullDataStorage[hull] = hullData;
            }
            hullData.Temperature = Math.Clamp(hullData.Temperature + amount, 0, 99999999);
        }


        public static void SetTemperature(Hull hull, float temperature)
        {
            if (!hullDataStorage.TryGetValue(hull, out HullData hullData))
            {
                hullData = new HullData(temperature);
                hullDataStorage[hull] = hullData;
            }
            else
            {
                hullData.Temperature = Math.Clamp(temperature, 0, 99999999);
            }
        }

        public static float GetGas(Hull hull, string gasName)
        {
            if (hullDataStorage.TryGetValue(hull, out HullData hullData) && 
                hullData.Gasses.TryGetValue(gasName, out var amount))
            {
                return amount;
            }
            return 0;
        }


        public static float GetTotalMoles(Hull hull)
        {
            float sum = 0;
            if (hullDataStorage.TryGetValue(hull, out HullData hullData))
            {
                foreach (var gasName in listGasses)
                {
                    sum += hullData.Gasses.TryGetValue(gasName, out var amount) ? amount : 0;
                }
            }
            return sum;
        }


        public static float Clamp(float value, float min, float max)
        {
            return Math.Min(Math.Max(value, min), max);
        }



        public static void StoreMainSubHulls()
        {
            hullDataStorage.Clear();

            foreach (Hull hull in Submarine.MainSub.GetHulls(true))
            {
                Vector2 size = hull.Size;
                float sizeHull = size.X * size.Y / 10000;
                HullData hullData = new HullData(NormalTemperature) { Size = sizeHull };
                hullDataStorage[hull] = hullData;

                //DebugConsole.NewMessage($"Hull Size: {sizeHull}", Color.White);
            }
        }

            private static void ValidateAndReinitializeHullDataStorage()
        {
            // Check if any hulls in the dictionary no longer exist or are invalid
            var invalidHulls = hullDataStorage.Keys.Where(hull => hull.Removed).ToList();

            // If there are invalid hulls, reinitialize the hullDataStorage
            if (invalidHulls.Any())
            {
                StoreMainSubHulls();
            }
        }


        /*public static void OnRoundEnded(GameSession gameSession)
        {
            isInitialized = false;
            //DebugConsole.NewMessage("Round ended", Color.White);
        }*/
       

    }
}
