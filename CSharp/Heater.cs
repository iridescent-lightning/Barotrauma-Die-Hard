using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Barotrauma.Items.Components;
using Barotrauma.Networking;
using Barotrauma.Extensions;
using Barotrauma;

using FluidSimulationMod;

namespace HeaterMod//todo make a structural namespace DieHard.Item.Components. namespace can't be used in elsewhere
{
    class Heater : Powered
    {
        private float heaterUpdateTimer;
        private const float HeaterUpdateInterval = 5.0f;
        public  float heatPower;

        public  bool isHeaterOn;

        [Editable, Serialize(1f, IsPropertySaveable.Yes, description: "How powerful the heater is.", alwaysUseInstanceValues: true)]
        public float HeatPower
        {
            get { return heatPower; }
            set { heatPower = MathHelper.Clamp(value, 0f, 1000f); }
        }


		//class name must be the same as constructor name otherwise return type error
        public Heater(Item item, ContentXElement element)
            : base(item, element)
        {
            heaterUpdateTimer = Rand.Range(0.0f, HeaterUpdateInterval);
            IsActive = true; 
        }
		
		 
		
        public override void Update(float deltaTime, Camera cam)
        {
            if (!item.InPlayerSubmarine || item == null || item.CurrentHull == null) {return;}

            if (Voltage < MinVoltage && PowerConsumption > 0)
            {
                isHeaterOn = false;
                return;
                //DebugConsole.NewMessage("Heater is not getting enough power");
            }
            UpdateHeater(deltaTime);
        }

        public void UpdateHeater(float deltaTime)
        {
            heaterUpdateTimer -= deltaTime;
                if (heaterUpdateTimer < 0.0f)
            {
                

                if (FluidSimulation.hullDataStorage.ContainsKey(item.CurrentHull))
                {
                    // Handle the case where the hull is not registered
                    // Now it's safe to access the hull's temperature
                    if (FluidSimulation.hullDataStorage[item.CurrentHull].Temperature < 303f)
                    {
                    FluidSimulation.hullDataStorage[item.CurrentHull].Temperature = MathHelper.Clamp(FluidSimulation.hullDataStorage[item.CurrentHull].Temperature + heatPower/FluidSimulation.hullDataStorage[item.CurrentHull].Size, 0f, 9999999f);

                    //DebugConsole.NewMessage($"{item.ID} "+"Heater is getting enough power", Color.Green);
                    isHeaterOn = true;
                    }
                
                }
                heaterUpdateTimer = HeaterUpdateInterval;}
        }

        public override float GetCurrentPowerConsumption(Connection connection = null)
        {
            
            if (connection != this.powerIn || !isHeaterOn)
            {
                
                return 0;
            }

            float consumption = powerConsumption;
            return consumption;
        }
            
            
            
            
    }

        
}

