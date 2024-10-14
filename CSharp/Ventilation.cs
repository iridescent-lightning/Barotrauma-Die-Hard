using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Barotrauma.Items.Components;
using Barotrauma.Networking;
using Barotrauma.Extensions;
using Barotrauma;

namespace VentilationMod//todo make a structural namespace DieHard.Item.Components. namespace can't be used in elsewhere
{
    class VentilationMachine : Powered
    {
        private float ventilationUpdateTimer;
        private const float VentilationUpdateInterval = 5.0f;
        
        public VentilationMachine(Item item, ContentXElement element)
            : base(item, element)
        {
            ventilationUpdateTimer = Rand.Range(0.0f, VentilationUpdateInterval);
            IsActive = true;
        }

        public override void Update(float deltaTime, Camera cam)
        {
            if (!item.InPlayerSubmarine || item == null) { return; }

            if (Voltage < MinVoltage && PowerConsumption > 0)
            {
                return;
            }

            ventilationUpdateTimer -= deltaTime;
            if (ventilationUpdateTimer < 0.0f)
            {
                UpdateVents(deltaTime);
                ventilationUpdateTimer = VentilationUpdateInterval;
            }
        }

        private void UpdateVents(float deltaTime)
        {
            foreach (MapEntity entity in item.linkedTo)
            {
                if (entity is Item linkedItem)
                {
                    VentilationVent vent = linkedItem.GetComponent<VentilationVent>();
                    vent?.Activate(deltaTime);
                }
            }
        }
    }

        
}

