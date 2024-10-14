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


namespace VentilationMod//todo make a structural namespace DieHard.Item.Components. namespace can't be used in elsewhere
{
    class VentilationVent : ItemComponent
    {
        public VentilationVent(Item item, ContentXElement element) : base(item, element) { }

        public void Activate(float deltaTime)
        {
            if (item.CurrentHull == null || item.InWater) { return; }

            // Assuming AddGas is a method in the FluidSimulationMod
            if (FluidSimulation.GetGas(item.CurrentHull, "CO2") > 2f)
            {
                FluidSimulationMod.FluidSimulation.AddGas(item.CurrentHull, "CO2", -10f/FluidSimulation.hullDataStorage[item.CurrentHull].Size);
            }
        }
    }

        
}

