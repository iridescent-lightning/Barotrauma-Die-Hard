using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Barotrauma.Items.Components;
using Barotrauma.Networking;
using Barotrauma.Extensions;
using Barotrauma;
using HullModNamespace;

#if CLIENT
using Microsoft.Xna.Framework.Graphics;
#endif

namespace AirMonitor//todo make a structural namespace DieHard.Item.Components. namespace can't be used in elsewhere
{
    class AirMonitor : ItemComponent
    {
#if CLIENT
		private GUIFrame mainFrame;
		
        private GUITextBlock temperatureTextBlock;
        private GUITextBlock co2TextBlock;
        private GUITextBlock chlorineTextBlock;
        private GUITextBlock coTextBlock;
        private GUITextBlock clTextBlock;
#endif
        /*public override void OnItemLoaded()
        {
            base.OnItemLoaded();
        }*/

        public AirMonitor(Item item, ContentXElement element)
            : base(item, element)
        {
            #if CLIENT
            if (GuiFrame == null) { return; }
            // Create the main frame for the GUI if it doesn't exist
            if (mainFrame == null)
            {
                mainFrame = new GUIFrame(new RectTransform(new Vector2(0.95f, 0.95f), GuiFrame.RectTransform, Anchor.Center), null);
            }

            // Create the temperature text block if it doesn't exist
            if (temperatureTextBlock == null)
            {
                temperatureTextBlock = new GUITextBlock(new RectTransform(new Vector2(1.0f, 0.1f), mainFrame.RectTransform, Anchor.TopLeft){RelativeOffset = new Vector2(0.0f, 0.1f)}, "", textAlignment: Alignment.Center);
            }

            if (co2TextBlock == null)
            {
                co2TextBlock = new GUITextBlock(new RectTransform(new Vector2(1.0f, 0.1f), mainFrame.RectTransform, Anchor.TopLeft){RelativeOffset = new Vector2(0.0f, 0.2f)}, "", textAlignment: Alignment.Center);
            }

            if (coTextBlock == null)
            {
                coTextBlock = new GUITextBlock(new RectTransform(new Vector2(1.0f, 0.1f), mainFrame.RectTransform, Anchor.TopLeft){RelativeOffset = new Vector2(0.0f, 0.3f)}, "", textAlignment: Alignment.Center);
            }
            
            if (clTextBlock == null)
            {
                clTextBlock = new GUITextBlock(new RectTransform(new Vector2(1.0f, 0.1f), mainFrame.RectTransform, Anchor.TopLeft){RelativeOffset = new Vector2(0.0f, 0.4f)}, "", textAlignment: Alignment.Center);
            }
            

            #endif
            //IsActive = true; //use xml to determine if active or not
            
        }
		
#if CLIENT
		public override void CreateGUI() 
        {
            
        }

		
        public override void Update(float deltaTime, Camera cam)
        {
            if (!item.InPlayerSubmarine || item == null || item.CurrentHull == null) {return;}
            
                float currentTemperature = HullMod.GetGas(item.CurrentHull, "Temperature");
                temperatureTextBlock.Text = $"Temperature: {currentTemperature.ToString("F1")} K";
            
            
                float currentCO2 = HullMod.GetGas(item.CurrentHull, "CO2");
                float cO2Percentage = currentCO2 / item.CurrentHull.Volume;
                co2TextBlock.Text = $"CO2: {cO2Percentage.ToString("F4")}" + "%";
            
                float currentCO = HullMod.GetGas(item.CurrentHull, "CO");
                float cOPercentage = currentCO;
                coTextBlock.Text = $"CO: {cOPercentage.ToString("F1")}" + "PPM";
            
                float currentCL = HullMod.GetGas(item.CurrentHull, "Chlorine");
                float cLPercentage = currentCL;
                clTextBlock.Text = $"CL: {cLPercentage.ToString("F1")}" + "PPM";
                
            
        }
#endif
        
    }
}
