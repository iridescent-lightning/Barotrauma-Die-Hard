using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Barotrauma.Items.Components;
using Barotrauma.Networking;
using Barotrauma.Extensions;
using Barotrauma;

using Networking;
using VentModNameSpace;

#if CLIENT
using Microsoft.Xna.Framework.Graphics;
#endif

namespace OxygenGeneratorMod//todo make a structural namespace DieHard.Item.Components. namespace can't be used in elsewhere
{
    class CustomOxygenGenerator : OxygenGenerator
    {
        private ItemContainer[] itemContainers; // Change to an array
        private ItemContainer secondItemContainer; // Add a second ItemContainer
        

        private float minGeneratedAmountFactor = 0.1f;
        private float maxGeneratedAmountFactor = 1.0f;

        private float newGeneratedAmountFactor = 1.0f;
        
        private float powerConsumptionDisplay;
        private float o2Production;
        private bool turnedOn = true;
        private float recycledAmount;

        [Editable, Serialize(9.0f, IsPropertySaveable.Yes, description: "How much CO2 gas it can reduced.", alwaysUseInstanceValues: true)]
        public float RecycledAmount
        {
            get { return recycledAmount; }
            set { recycledAmount = MathHelper.Clamp(value, -100.0f, 100.0f); }
        }

        public float CurrRecycleFlow
        {
            get;
            private set;
        }

        private float purifyingAmount;
        [Editable, Serialize(10.0f, IsPropertySaveable.Yes, description: "How much toxic gas it can reduced.", alwaysUseInstanceValues: true)]
        public float PurifyingAmount
        {
            get { return purifyingAmount; }
            set { purifyingAmount = MathHelper.Clamp(value, -100.0f, 100.0f); }
        }
        public float CurrPurifyingFlow
        {
            get;
            private set;
        }

        private float heatingAmount;
        [Editable, Serialize(25.0f, IsPropertySaveable.Yes, description: "How much heat it can generate. Setting too high will cause unexpected consequence.", alwaysUseInstanceValues: true)]
        public float HeatingAmount
        {
            get { return heatingAmount; }
            set { heatingAmount = MathHelper.Clamp(value, -10000.0f, 10000.0f); }
        }
        public float CurrHeatingFlow
        {
            get;
            private set;
        }

        public bool HasPower => IsActive && Voltage >= MinVoltage;
#if CLIENT
		private GUIFrame mainFrame;
		public GUIButton PowerButton;
        private GUITickBox powerLight;
        private GUIScrollBar powerSlider;
        private GUITextBlock powerConText;
        private GUITextBlock o2ProductionText;
#endif
        public override void OnItemLoaded()
        {
            base.OnItemLoaded();

            // Retrieve all ItemContainer components attached to the item
            itemContainers = item.GetComponents<ItemContainer>()
                                 .OfType<ItemContainer>()
                                 .ToArray();


            // Check if there are any ItemContainers
            if (itemContainers == null || itemContainers.Length < 2)
            {
                DebugConsole.LogError("Not enough ItemContainer components found on the item.");
                return;  // Return or handle the error appropriately
            }

            // Access the second ItemContainer from the array
            secondItemContainer = itemContainers[1];
#if CLIENT
            powerSlider.BarScroll = 1.0f;
    
            // Calculate the new generated amount factor based on the scrollbar's position
            newGeneratedAmountFactor = MathHelper.Lerp(minGeneratedAmountFactor, maxGeneratedAmountFactor, powerSlider.BarScroll);
            
#endif            
        }
        
        public CustomOxygenGenerator(Item item, ContentXElement element)
            : base(item, element)
        {
            ventUpdateTimer = Rand.Range(0.0f, VentUpdateInterval);
            IsActive = true;
#if CLIENT
            if (GuiFrame == null) { return; }
            if (mainFrame == null)
            {
                mainFrame = new GUIFrame(new RectTransform(new Vector2(0.95f, 0.95f), GuiFrame.RectTransform, Anchor.Center), null);
            }

            var powerButtonArea = new GUIFrame(new RectTransform(new Vector2(0.25f, 0.5f), mainFrame.RectTransform, Anchor.CenterLeft)
            {
                RelativeOffset = new Vector2(0, 0.1f)
            }, style: null);

            var powerLightArea = new GUIFrame(new RectTransform(new Vector2(0.2f, 0.2f), powerButtonArea.RectTransform, Anchor.TopRight), style: null);
            powerLight = new GUITickBox(new RectTransform(Vector2.One, powerLightArea.RectTransform, Anchor.Center),
                TextManager.Get("PowerLabel"), font: GUIStyle.SubHeadingFont, style: "IndicatorLightPower")
            {
                CanBeFocused = false
            };

            PowerButton = new GUIButton(new RectTransform(new Vector2(1f, 1f), powerButtonArea.RectTransform, Anchor.Center)
            {
                RelativeOffset = new Vector2(0, 0.1f)
            }, style: "PowerButton")
            {
                //UserData = UIHighlightAction.ElementId.PowerButton,
                OnClicked = (button, data) =>
                {
                    turnedOn = !turnedOn;
                    //IsActive = !IsActive;
                    if (GameMain.Client != null)
                    {
                        correctionTimer = CorrectionDelay;
                        SendToggleOxygenGeneratorMessage(item, turnedOn);
                    }
                    powerLight.Selected = IsActive;
                    return true;
                }
            };
            powerSlider = new GUIScrollBar(new RectTransform(new Vector2(0.5f, 0.15f), mainFrame.RectTransform, Anchor.BottomLeft)
            {
                RelativeOffset = new Vector2(0.45f, 0.1f)
            }, barSize: 0.15f, isHorizontal: true, style: "DeviceSlider")
            {
                OnMoved = (scrollbar, scroll) =>
                {
                    
                    // Calculate the new generated amount factor based on the scrollbar's position
                    newGeneratedAmountFactor = MathHelper.Lerp(minGeneratedAmountFactor, maxGeneratedAmountFactor, scroll);
                    //DebugConsole.NewMessage(newGeneratedAmountFactor.ToString());
                    
                    
                    
                    if (GameMain.Client != null)
                    {
                        //unsentChanges = true;
                        correctionTimer = Sonar.CorrectionDelay;
                        SendGeneratedAmountFactorMessage(item, newGeneratedAmountFactor);
                    }
                    return true;
                }
            };

            var digitalBackground = new GUIFrame(new RectTransform(new Vector2(0.35f, 0.2f), mainFrame.RectTransform, Anchor.CenterRight){RelativeOffset = new Vector2(0.04f, 0.1f)}, style: "DigitalFrameDark");

            var powerconsuptionLabel = new GUITextBlock(new RectTransform(new Vector2(0.5f, 0.25f), digitalBackground.RectTransform, Anchor.CenterLeft){RelativeOffset = new Vector2(-0.5f, 0)},
                TextManager.Get("KW"), font: GUIStyle.SubHeadingFont, textAlignment: Alignment.CenterRight);
                //textBlocksToScaleAndNormalize.Add(powerconsuptionLabel);
            powerConText = new GUITextBlock(new RectTransform(new Vector2(0.9f, 0.95f), digitalBackground.RectTransform, Anchor.Center), 
                "", font: GUIStyle.DigitalFont, textColor: GUIStyle.TextColorDark)
            {
                TextAlignment = Alignment.CenterRight,
                //ToolTip = TextManager.Get("SonarHertzTip"), // Update tooltip as necessary
                TextGetter = () => powerConsumptionDisplay.ToString("F1")// Display the current hertz value
            };

            var digitalBackgroundO2 = new GUIFrame(new RectTransform(new Vector2(0.35f, 0.2f), mainFrame.RectTransform, Anchor.CenterRight){RelativeOffset = new Vector2(0.04f, -0.2f)}, style: "DigitalFrameDark");

            var o2ProductionLabel = new GUITextBlock(new RectTransform(new Vector2(0.5f, 0.25f), digitalBackgroundO2.RectTransform, Anchor.CenterLeft){RelativeOffset = new Vector2(-0.5f, 0)},
                TextManager.Get("O2"), font: GUIStyle.SubHeadingFont, textAlignment: Alignment.CenterRight);

            o2ProductionText = new GUITextBlock(new RectTransform(new Vector2(0.9f, 0.95f), digitalBackgroundO2.RectTransform, Anchor.Center), 
                "", font: GUIStyle.DigitalFont, textColor: GUIStyle.TextColorDark)
            {
                TextAlignment = Alignment.CenterRight,
                //ToolTip = TextManager.Get("SonarHertzTip"), // Update tooltip as necessary
                TextGetter = () => o2Production.ToString("F0")// Display the current hertz value
            };
#endif
#if SERVER
            NetUtil.Register(NetEvent.CUSTOM_OXYGENGENERATOR_TOGGLE, OnReceiveToggleOxygenGeneratorMessage);

            NetUtil.Register(NetEvent.CUSTOM_OXYGENGENERATOR_GENERATEDAMOUNTFACTOR, OnReceiveGeneratedAmountFactorMessage);
#endif
        }

        public override void Update(float deltaTime, Camera cam)
        {
            //DebugConsole.NewMessage(this.GeneratedAmount.ToString());
			
            CurrRecycleFlow = 0.0f;
            CurrFlow = 0.0f;
            CurrPurifyingFlow = 0.0f;
            CurrHeatingFlow = 0.0f;

            if (item.CurrentHull == null) { return; }
            
            if (Voltage < MinVoltage && PowerConsumption > 0)
            {
                return;
            }
                // Get the items at slot 0 from the second container
            Item waterTank = secondItemContainer.Inventory.GetItemAt(0) as Item;
            
            if (!turnedOn)
            {   
                CurrHeatingFlow = 0.0f;
                CurrPurifyingFlow = 0.0f;
                CurrRecycleFlow = 0.0f;
                CurrFlow = 0.0f;
                
                VentMod.CO2Flow = 0.0f;
                VentMod.PurifyingFlow = 0.0f;
                VentMod.HeatFlow = 0.0f;
                return;
            }
            else
            {
                //DebugConsole.NewMessage("Oxygen Generator is turned on");
            }
                if (waterTank != null && waterTank.Condition <= 0)
                {
                    // Water tank is empty, set CurrFlow to 0
                    CurrFlow = 0.0f;
                    VentMod.CO2Flow = 0.0f;
                    VentMod.PurifyingFlow = 0.0f;
                    VentMod.HeatFlow = 0.0f;
                }
                else if (waterTank == null)
                {
                    CurrFlow = 0.0f;
                    VentMod.CO2Flow = 0.0f;
                    VentMod.PurifyingFlow = 0.0f;
                    VentMod.HeatFlow = 0.0f;
                }
                else
                {
                    CurrFlow = Math.Min(PowerConsumption > 0 ? Voltage : 1.0f, MaxOverVoltageFactor) * generatedAmount * 10.0f;
                    float conditionMult = item.Condition / item.MaxCondition;
                    
                    
                    //100% condition = 100% oxygen
                    //50% condition = 25% oxygen
                    //20% condition = 4%
                    CurrFlow *= conditionMult * conditionMult * newGeneratedAmountFactor;
                    //DebugConsole.NewMessage(CurrFlow.ToString());
                    

                    CurrRecycleFlow = Math.Min(PowerConsumption > 0 ? Voltage : 1.0f, MaxOverVoltageFactor) * recycledAmount * 0.01f;
                    CurrRecycleFlow *= conditionMult * conditionMult * newGeneratedAmountFactor;
                    
                    CurrPurifyingFlow = Math.Min(PowerConsumption > 0 ? Voltage : 1.0f, MaxOverVoltageFactor) * purifyingAmount * 0.01f;
                    CurrPurifyingFlow *= conditionMult * conditionMult * newGeneratedAmountFactor;

                    CurrHeatingFlow = Math.Min(PowerConsumption > 0 ? Voltage : 1.0f, MaxOverVoltageFactor) * heatingAmount;
                    CurrHeatingFlow *= conditionMult * conditionMult * newGeneratedAmountFactor;


                    UpdateVents(CurrFlow, CurrRecycleFlow, CurrPurifyingFlow, CurrHeatingFlow, deltaTime);


                    if (item.InPlayerSubmarine)
                    {
                        waterTank.Condition = waterTank.Condition - 0.01f * newGeneratedAmountFactor * deltaTime;
                    }
                }
            
            item.SendSignal(CurrFlow.ToString(), "oxygen_generated_amount_out");
            

            //DebugConsole.NewMessage(this.CurrFlow.ToString("F0"));

        }

        private void UpdateVents(float deltaOxygen, float deltaCO2, float deltaPurify, float deltaHeat, float deltaTime)
        {
            if (ventList == null || ventUpdateTimer < 0.0f)
            {
                GetVents();
                ventUpdateTimer = VentUpdateInterval;
            }
            ventUpdateTimer -= deltaTime;

            if (!ventList.Any() || totalHullVolume <= 0.0f) { return; }

            foreach ((Vent vent, float hullVolume) in ventList)
            {
                if (vent.Item.CurrentHull == null) { continue; }

                vent.OxygenFlow = deltaOxygen * (hullVolume / totalHullVolume);
                VentMod.co2Flow = deltaCO2 * (hullVolume / totalHullVolume);
                VentMod.purifyingFlow = deltaPurify * (hullVolume / totalHullVolume);
                VentMod.heatFlow = deltaHeat * (hullVolume / totalHullVolume);
                vent.IsActive = true;
            }
        }


#if CLIENT
        private float flickerTimer;
        private readonly float flickerFrequency = 1;

        private float isActiveLockTimer;
        public override void UpdateHUDComponentSpecific(Character character, float deltaTime, Camera cam)
        {
            
            PowerButton.Enabled = isActiveLockTimer <= 0.0f;
            if (HasPower)
            {
                flickerTimer = 0;
                powerLight.Selected = IsActive;
            }
            else if (IsActive)
            {
                flickerTimer += deltaTime;
                if (flickerTimer > flickerFrequency)
                {
                    flickerTimer = 0;
                    powerLight.Selected = !powerLight.Selected;
                }
            }
            else
            {
                flickerTimer = 0;
                powerLight.Selected = false;
            }
            
            powerConsumptionDisplay = this.GetCurrentPowerConsumption(this.powerIn) * newGeneratedAmountFactor;
            o2Production = this.CurrFlow;
            
        }

        
        private void SendToggleOxygenGeneratorMessage(Item item, bool turnedOn)
        {
            IWriteMessage msg = NetUtil.CreateNetMsg(NetEvent.CUSTOM_OXYGENGENERATOR_TOGGLE);

            msg.WriteUInt16(item.ID); // ID of the oxygen generator item
            msg.WriteBoolean(turnedOn); // New IsActive state
            NetUtil.SendServer(msg, DeliveryMethod.Reliable);
        }

        private void SendGeneratedAmountFactorMessage(Item item, float newGeneratedAmountFactor)
        {
            IWriteMessage msg = NetUtil.CreateNetMsg(NetEvent.CUSTOM_OXYGENGENERATOR_GENERATEDAMOUNTFACTOR);

            msg.WriteUInt16(item.ID); // ID of the oxygen generator item
            msg.WriteSingle(newGeneratedAmountFactor); // New generated amount factor
            NetUtil.SendServer(msg, DeliveryMethod.Reliable);
        }
#endif
        public override float GetCurrentPowerConsumption(Connection connection = null)
        {
            if (connection != this.powerIn || !IsActive || !turnedOn)
            {
                return 0;
            }

            float consumption = powerConsumption * newGeneratedAmountFactor;

            //consume more power when in a bad condition
            item.GetComponent<Repairable>()?.AdjustPowerConsumption(ref consumption);
            return consumption;
        }

        private void OnReceiveToggleOxygenGeneratorMessage(object[] args)
        {
            IReadMessage msg = (IReadMessage)args[0];
            ushort itemId = msg.ReadUInt16();
            bool isActive = msg.ReadBoolean();

            Item oxygenGeneratorItem = Entity.FindEntityByID(itemId) as Item;
            if (oxygenGeneratorItem != null)
            {
                var oxygenGenerator = oxygenGeneratorItem.GetComponent<CustomOxygenGenerator>();
                if (oxygenGenerator != null)
                {
                    oxygenGenerator.turnedOn = turnedOn;
                    // Additional logic if needed
                }
            }
        }

        private void OnReceiveGeneratedAmountFactorMessage(object[] args)
        {
            IReadMessage msg = (IReadMessage)args[0];
            ushort itemId = msg.ReadUInt16();
            float newGeneratedAmountFactor = msg.ReadSingle();

            Item oxygenGeneratorItem = Entity.FindEntityByID(itemId) as Item;
            if (oxygenGeneratorItem != null)
            {
                var oxygenGenerator = oxygenGeneratorItem.GetComponent<CustomOxygenGenerator>();
                if (oxygenGenerator != null)
                {
                    oxygenGenerator.newGeneratedAmountFactor = newGeneratedAmountFactor;
                    // Additional logic if needed
                }
            }

        }
    }
}
