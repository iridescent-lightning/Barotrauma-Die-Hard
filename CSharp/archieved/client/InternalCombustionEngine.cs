using Microsoft.Xna.Framework;
using System;
using System.Globalization;
using System.Xml.Linq;
using Barotrauma.Networking;
using Barotrauma.Items.Components;
using Barotrauma;
using Microsoft.Xna.Framework.Graphics;


namespace BarotraumaDieHard
{
    public partial class InternalCombustionEngine : Engine
    {
        private  GUIFrame mainPanel;
        private  Sprite fissionRateMeter;
        private  Sprite turbineOutputMeter;
        private  Sprite meterPointer;
        private  Sprite sectorSprite;
        private  Sprite tempMeterFrame;
        private  Sprite tempMeterBar;
        private  Sprite tempRangeIndicator;
        private  Sprite meterMatte;
        private  Sprite ui;
        private  Sprite verticalBarMeterBackground;
        //public delegate void DrawFissionRateMeterDelegate(SpriteBatch spriteBatch, GUICustomComponent container);
        private readonly int updateGraphInterval = 500;
        private int graphTimer;

        private  Color optimalRangeColor = new Color(74,238,104,255);
        private  Color offRangeColor = Color.Orange;
        private  Color warningColor = Color.Red;
        private  readonly Color[] temperatureColors = new Color[] { Color.Blue, Color.LightBlue, Color.Orange, Color.Red };


        


        partial void InitProjSpecificDieHard(ContentXElement element)
        {
            fissionRateMeter = new Sprite(element.GetChildElement("fissionratemeter")?.GetChildElement("sprite"));
            turbineOutputMeter = new Sprite(element.GetChildElement("turbineoutputmeter")?.GetChildElement("sprite"));
            meterPointer = new Sprite(element.GetChildElement("meterpointer")?.GetChildElement("sprite"));
            sectorSprite = new Sprite(element.GetChildElement("sectorsprite")?.GetChildElement("sprite"));
            tempMeterFrame = new Sprite(element.GetChildElement("tempmeterframe")?.GetChildElement("sprite"));
            tempMeterBar = new Sprite(element.GetChildElement("tempmeterbar")?.GetChildElement("sprite"));
            tempRangeIndicator = new Sprite(element.GetChildElement("temprangeindicator")?.GetChildElement("sprite"));
            meterMatte = new Sprite(element.GetChildElement("MeterBackGround")?.GetChildElement("sprite"));
            ui = new Sprite(element.GetChildElement("UIBackground")?.GetChildElement("sprite"));
            verticalBarMeterBackground = new Sprite(element.GetChildElement("LubricantMeterBackground")?.GetChildElement("sprite"));

            mainPanel = new GUIFrame(new RectTransform(new Vector2(3f, 4f), this.GuiFrame.RectTransform, Anchor.Center)
            {
                RelativeOffset = new Vector2(-0.6f, -1.5f)
                
            }, null);

            var uiBackground = new GUICustomComponent(new RectTransform(new Vector2(1.5f, 1.2f), mainPanel.RectTransform, Anchor.Center)
            {
                RelativeOffset = new Vector2(-0.5f, 0.0f)
            }, (spriteBatch, customContainer) => DrawUIBackground(spriteBatch, customContainer, ui, 1f));

            //var line = new GUIFrame(new RectTransform(new Vector2(1.0f, 1f), mainPanel.RectTransform), style: "HorizontalLine");
            var mattedBackground = new GUICustomComponent(new RectTransform(new Vector2(0.9f, 0.6f), mainPanel.RectTransform, Anchor.TopCenter)
            {
                RelativeOffset = new Vector2(-0.05f, 0.1f)
            }, (spriteBatch, customContainer) => DrawSimpleBackground(spriteBatch, customContainer, meterMatte, 1f));

            var forceMeter = new GUICustomComponent(new RectTransform(new Vector2(0.4f, 0.7f), mattedBackground.RectTransform, Anchor.Center)
            {
                RelativeOffset = new Vector2(-0.3f, 0f)
            },
                (spriteBatch, customContainer) => DrawForceMeterInit(spriteBatch, customContainer), null)
            {
                ToolTip = TextManager.Get("ReactorTipFissionRate")
            };
            var forceText = new GUITextBlock(new RectTransform(new Vector2(0.25f, 0.1f), forceMeter.RectTransform)
            {RelativeOffset = new Vector2(0.38f, 1.1f) },
            "ForceOutput", null, // null to use default color
            font: GUIStyle.SubHeadingFont,
            textAlignment: Alignment.Center
            );

            var turbineMeter = new GUICustomComponent(new RectTransform(new Vector2(0.4f, 0.7f), mattedBackground.RectTransform, Anchor.Center)
            {
                RelativeOffset = new Vector2(-0.05f,0)
            },
                (spriteBatch, customContainer) => DrawTurbineOutputMeter(spriteBatch, customContainer), null)
            {
                ToolTip = TextManager.Get("ReactorTipFissionRate")
            };
            var turbineText = new GUITextBlock(new RectTransform(new Vector2(0.25f, 0.1f), turbineMeter.RectTransform)
            {RelativeOffset = new Vector2(0.38f, 1.1f) },
            "turbineoutput", null, // null to use default color
            font: GUIStyle.SubHeadingFont,
            textAlignment: Alignment.Center
            );


            var requiredTurbineMeter = new GUICustomComponent(new RectTransform(new Vector2(0.4f, 0.7f), mattedBackground.RectTransform, Anchor.Center)
            {
                RelativeOffset = new Vector2(0.19f,0f)
            },
                (spriteBatch, customContainer) => DrawRquiredTurbineOutputMeter(spriteBatch, customContainer), null)
            {
                ToolTip = TextManager.Get("ReactorTipFissionRate")
            };
            var requiredTurbineText = new GUITextBlock(new RectTransform(new Vector2(0.25f, 0.1f), requiredTurbineMeter.RectTransform)
            {RelativeOffset = new Vector2(0.38f, 1.1f) },
            "required turbineoutput", null, // null to use default color
            font: GUIStyle.SubHeadingFont,
            textAlignment: Alignment.Center
            );

            var barMeterBackGround = new GUICustomComponent(new RectTransform(new Vector2(0.06f, 0.4f), mainPanel.RectTransform, Anchor.TopCenter)
            {
                RelativeOffset = new Vector2(0.25f, 0.15f)
            }, (spriteBatch, customContainer) => DrawSimpleBackground(spriteBatch, customContainer, verticalBarMeterBackground, 1f));
            
            var lubricationMeter = new GUICustomComponent(new RectTransform(new Vector2(0.45f, 0.8f), barMeterBackGround.RectTransform, Anchor.TopCenter)
            {
                RelativeOffset = new Vector2(0f, 0.04f)
            },
                (spriteBatch, customContainer) => DrawLubMeter(spriteBatch, customContainer), null)
            {
                ToolTip = TextManager.Get("ReactorTipLubrication")
            };

            var barMeterBackGroundTempreature = new GUICustomComponent(new RectTransform(new Vector2(0.06f, 0.4f), mainPanel.RectTransform, Anchor.TopCenter)
            {
                RelativeOffset = new Vector2(0.29f, 0.15f)
            }, (spriteBatch, customContainer) => DrawSimpleBackground(spriteBatch, customContainer, verticalBarMeterBackground, 1f));
            
            var tempMeter = new GUICustomComponent(new RectTransform(new Vector2(0.45f, 0.8f), barMeterBackGroundTempreature.RectTransform, Anchor.TopCenter)
            {
                RelativeOffset = new Vector2(0f, 0.04f)
            },
                (spriteBatch, customContainer) => DrawTempMeter(spriteBatch, customContainer), null)
            {
                ToolTip = TextManager.Get("ReactorTipLubrication")
            };
            var barMeterBackGroundHydrogen = new GUICustomComponent(new RectTransform(new Vector2(0.06f, 0.4f), mainPanel.RectTransform, Anchor.TopCenter)
            {
                RelativeOffset = new Vector2(0.33f, 0.15f)
            }, (spriteBatch, customContainer) => DrawSimpleBackground(spriteBatch, customContainer, verticalBarMeterBackground, 1f));
            
            var fuelMeter = new GUICustomComponent(new RectTransform(new Vector2(0.45f, 0.8f), barMeterBackGroundHydrogen.RectTransform, Anchor.TopCenter)
            {
                RelativeOffset = new Vector2(0f, 0.04f)
            },
                (spriteBatch, customContainer) => DrawFuelMeter(spriteBatch, customContainer), null)
            {
                ToolTip = TextManager.Get("ReactorTipLubrication")
            };

            var EngineModeSwitch = new GUIButton(new RectTransform(new Vector2(0.05f, 0.1f), mainPanel.RectTransform, Anchor.BottomLeft){RelativeOffset = new Vector2(0.1f,0.1f)}, string.Empty, style: "SwitchDieHardButton")
            {
                
                Selected = false,
                Enabled = true,
                ClickSound = GUISoundType.UISwitch,
                OnClicked = (button, data) =>
                {
                    button.Selected = !button.Selected;
                    this.useElectric = !this.useElectric;
                    
                    if (GameMain.Client != null)
                    {
                        
                    }
                    return true;
                }
            };
            
            var paddedFrame = new GUIFrame(new RectTransform(new Vector2(0.85f, 0.65f), GuiFrame.RectTransform, Anchor.Center)
            {
                RelativeOffset = new Vector2(0, 0.04f)
            }, style: null);

            var lightsArea = new GUIFrame(new RectTransform(new Vector2(1, 0.38f), paddedFrame.RectTransform, Anchor.TopLeft), style: null);
            powerIndicator = new GUITickBox(new RectTransform(new Vector2(0.45f, 0.8f), lightsArea.RectTransform, Anchor.Center, Pivot.CenterRight)
            {
                RelativeOffset = new Vector2(-0.05f, 0)
            }, TextManager.Get("EnginePowered"), font: GUIStyle.SubHeadingFont, style: "IndicatorLightGreen")
            {
                CanBeFocused = false
            };
            autoControlIndicator = new GUITickBox(new RectTransform(new Vector2(0.45f, 0.8f), lightsArea.RectTransform, Anchor.Center, Pivot.CenterLeft)
            {
                RelativeOffset = new Vector2(0.05f, 0)
            }, TextManager.Get("PumpAutoControl", "ReactorAutoControl"), font: GUIStyle.SubHeadingFont, style: "IndicatorLightYellow")
            {
                Selected = false,
                Enabled = false,
                ToolTip = TextManager.Get("AutoControlTip")
            };
            powerIndicator.TextBlock.Wrap = autoControlIndicator.TextBlock.Wrap = true;
            powerIndicator.TextBlock.OverrideTextColor(GUIStyle.TextColorNormal);
            autoControlIndicator.TextBlock.OverrideTextColor(GUIStyle.TextColorNormal);
            GUITextBlock.AutoScaleAndNormalize(powerIndicator.TextBlock, autoControlIndicator.TextBlock);

            var sliderArea = new GUIFrame(new RectTransform(new Vector2(1, 0.6f), paddedFrame.RectTransform, Anchor.BottomLeft), style: null);
            LocalizedString powerLabel = TextManager.Get("EngineForce");
            new GUITextBlock(new RectTransform(new Vector2(1.0f, 0.3f), sliderArea.RectTransform, Anchor.TopCenter), "", textColor: GUIStyle.TextColorNormal, font: GUIStyle.SubHeadingFont, textAlignment: Alignment.Center)
            {
                AutoScaleHorizontal = true,
                TextGetter = () => 
                { 
                    return TextManager.AddPunctuation(':', powerLabel, 
                        TextManager.GetWithVariable("percentageformat", "[value]", ((int)MathF.Round(targetForce)).ToString())); 
                }
            };
            forceSlider = new GUIScrollBar(new RectTransform(new Vector2(0.95f, 0.45f), sliderArea.RectTransform, Anchor.Center), barSize: 0.1f, style: "DeviceSlider")
            {
                Step = 0.05f,
                OnMoved = (GUIScrollBar scrollBar, float barScroll) =>
                {
                    lastReceivedTargetForce = null;
                    float newTargetForce = barScroll * 200.0f - 100.0f;
                    if (Math.Abs(newTargetForce - targetForce) < 0.01) { return false; }

                    targetForce = newTargetForce;
                    User = Character.Controlled;

                    if (GameMain.Client != null)
                    {
                        correctionTimer = CorrectionDelay;
                        item.CreateClientEvent(this);
                    }
                    return true;
                }
            };
            var textsArea = new GUIFrame(new RectTransform(new Vector2(1, 0.25f), sliderArea.RectTransform, Anchor.BottomCenter), style: null);
            var backwardsLabel = new GUITextBlock(new RectTransform(new Vector2(0.4f, 1.0f), textsArea.RectTransform, Anchor.CenterLeft), TextManager.Get("EngineBackwards"),
                textColor: GUIStyle.TextColorNormal, font: GUIStyle.SubHeadingFont, textAlignment: Alignment.CenterLeft);
            var forwardsLabel = new GUITextBlock(new RectTransform(new Vector2(0.4f, 1.0f), textsArea.RectTransform, Anchor.CenterRight), TextManager.Get("EngineForwards"),
                textColor: GUIStyle.TextColorNormal, font: GUIStyle.SubHeadingFont, textAlignment: Alignment.CenterRight);
            GUITextBlock.AutoScaleAndNormalize(backwardsLabel, forwardsLabel);


            
            
            var turbineSlider = new GUIScrollBar(new RectTransform(new Vector2(0.95f, 0.45f), sliderArea.RectTransform, Anchor.Center){RelativeOffset = new Vector2(-1f, 0)}, barSize: 0.1f, style: "DeviceSlider")
            {
                //Step = 0.05f,
                
                OnMoved = (GUIScrollBar scrollBar, float barScroll) =>
                {
                    
                    if (ammonia > 0)
                    {
                        turbineRate = MathHelper.Lerp(0f,200f,barScroll);
                        DebugConsole.NewMessage("Turbine Rate: " + turbineRate, Color.Red);
                        if (GameMain.Client != null)
                        {
                            correctionTimer = CorrectionDelay;
                            item.CreateClientEvent(this);
                        }
                    }
                    else
                    {
                        turbineRate = 0.0f;  // Set turbineRate to 0 if ammonia is 0
                    }
                    return true;
                }
            };
            LocalizedString powerLabelTurbine = TextManager.Get("TurbineRateSliderText");
            new GUITextBlock(new RectTransform(new Vector2(1.0f, 0.3f), turbineSlider.RectTransform, Anchor.TopCenter){RelativeOffset = new Vector2(0,-0.18f)}, "", textColor: GUIStyle.TextColorNormal, font: GUIStyle.SubHeadingFont, textAlignment: Alignment.Center)
            {
                AutoScaleHorizontal = true,
                TextGetter = () => 
                { 
                    return TextManager.AddPunctuation(':', powerLabelTurbine, 
                        TextManager.GetWithVariable("percentageformat", "[value]", ((int)MathF.Round(turbineRate)).ToString())); 
                }
            };
            

            foreach (var subElement in element.Elements())
            {
                switch (subElement.Name.ToString().ToLowerInvariant())
                {
                    case "propellersprite":
                        propellerSprite = new SpriteSheet(subElement);
                        AnimSpeed = subElement.GetAttributeFloat("animspeed", 1.0f);
                        break;
                }
            }

            
            
            
            
        }
        
        
        private void DrawForceMeterInit(SpriteBatch spriteBatch, GUICustomComponent container)
        {
            
            if (this.item.Removed) { return; }

            DrawForceMeter(spriteBatch, container.Rect,
                fissionRateMeter, targetForce, new Vector2(0.0f, 100.0f), new Vector2(0.0f, 100.0f), new Vector2(0.0f, 100.0f), 100f);
            
        }

        
        
        private void DrawForceMeter(SpriteBatch spriteBatch, Rectangle rect, Sprite meterSprite, float value, Vector2 range, Vector2 optimalRange, Vector2 allowedRange, float maxForce)
        {
            float scale = Math.Min(rect.Width / meterSprite.size.X, rect.Height / meterSprite.size.Y);
            Vector2 pos = new Vector2(rect.Center.X, rect.Y + meterSprite.Origin.Y * scale);

            

            Vector2 pointerPos = pos - new Vector2(5, 220) * scale;
            Vector2 meterBackgroundPos = pos - new Vector2(0, 0) * scale;

            float scaleMultiplier = 0.95f;
            

            meterSprite.Draw(spriteBatch, pos, 0, scale);
            
            float adjustedAngle = MathHelper.Lerp(MathHelper.ToRadians(250f), MathHelper.ToRadians(500f), Math.Abs(value / maxForce)); 

            //DebugConsole.NewMessage("value: " + value);
            //DebugConsole.NewMessage("maxForce: " + maxForce);
            meterPointer.Draw(spriteBatch, pointerPos, adjustedAngle, scale);
        }


        


        private void DrawTurbineOutputMeter(SpriteBatch spriteBatch, GUICustomComponent container)
        {
            
            if (this.item.Removed) { return; }
            //make the pointer jitter a bit if it's at the upper limit of the fission rate
            float jitter = 0.0f;
            if (this.force > this.maxForce - 115.0f)
            {
                //DebugConsole.NewMessage("Jittering", Color.White);
                float jitterAmount = 100f; //Math.Min(TargetFissionRate - allowedFissionRate.Y, 10.0f);
                //float t = graphTimer / updateGraphInterval;

                jitter = (PerlinNoise.GetPerlin( 0.5f,  0.1f) - 0.5f) * jitterAmount;
            }

            // This draws the turbine output meter
            DrawTurbineMeter(spriteBatch, container.Rect,
                fissionRateMeter, this.turbineRate + jitter, new Vector2(0.0f, 100.0f), new Vector2(0.0f, 100.0f), new Vector2(0.0f, 100.0f), 100f);

            
        }

        private void DrawRquiredTurbineOutputMeter(SpriteBatch spriteBatch, GUICustomComponent container)
        {
            
            if (this.item.Removed) { return; }
            //make the pointer jitter a bit if it's at the upper limit of the fission rate
            float jitter = 0.0f;
            if (this.force > this.maxForce - 115.0f)
            {
                //DebugConsole.NewMessage("Jittering", Color.White);
                float jitterAmount = 100f; //Math.Min(TargetFissionRate - allowedFissionRate.Y, 10.0f);
                //float t = graphTimer / updateGraphInterval;

                jitter = (PerlinNoise.GetPerlin( 0.5f,  0.1f) - 0.5f) * jitterAmount;
            }

            // This draws the turbine output meter
            DrawTurbineMeter(spriteBatch, container.Rect,
                fissionRateMeter, this.requiredTurbineRate + jitter, new Vector2(0.0f, 100.0f), new Vector2(0.0f, 100.0f), new Vector2(0.0f, 100.0f), 100f);

            
        }

        private void DrawTurbineMeter(SpriteBatch spriteBatch, Rectangle rect, Sprite meterSprite, float value, Vector2 range, Vector2 optimalRange, Vector2 allowedRange, float maxForce)
        {
            float scale = Math.Min(rect.Width / meterSprite.size.X, rect.Height / meterSprite.size.Y);
            Vector2 pos = new Vector2(rect.Center.X, rect.Y + meterSprite.Origin.Y * scale);

            
            Vector2 pointerPos = pos - new Vector2(5, 220) * scale;
            Vector2 meterBackgroundPos = pos - new Vector2(0, 0) * scale;

            float scaleMultiplier = 0.95f;
            

            meterSprite.Draw(spriteBatch, pos, 0, scale);
            //float initialAngle = MathHelper.ToRadians(190f);
            float adjustedAngle = MathHelper.Lerp(MathHelper.ToRadians(250f), MathHelper.ToRadians(500f), Math.Abs((value / maxForce) / 2.0f));

            
            meterPointer.Draw(spriteBatch, pointerPos, adjustedAngle, scale);
        }


        private void DrawLubMeter(SpriteBatch spriteBatch, GUICustomComponent container)
        {
            
            Vector2 meterPos = new Vector2(container.Rect.X, container.Rect.Y);
            Vector2 meterScale = new Vector2(container.Rect.Width / (float)tempMeterFrame.SourceRect.Width, container.Rect.Height / (float)tempMeterFrame.SourceRect.Height);
            tempMeterFrame.Draw(spriteBatch, meterPos, Color.White, tempMeterFrame.Origin, 0.0f, scale: meterScale);

            float tempFill = lubricant / lubricantMax;
            float meterBarScale = container.Rect.Width / (float)tempMeterBar.SourceRect.Width;
            Vector2 meterBarPos = new Vector2(container.Center.X, container.Rect.Bottom - tempMeterBar.size.Y * meterBarScale - (int)(5 * GUI.yScale));
            while (meterBarPos.Y > container.Rect.Bottom + (int)(5 * GUI.yScale) - container.Rect.Height * tempFill)
            {
                float tempRatio = 1.0f - ((meterBarPos.Y - container.Rect.Y) / container.Rect.Height);
                
                tempMeterBar.Draw(spriteBatch, meterBarPos, color: Color.Orange, scale: meterBarScale);
                int spacing = 2;
                meterBarPos.Y -= tempMeterBar.size.Y * meterBarScale + spacing;
            }

            /*if (temperature > optimalTemperature.Y)
            {
                GUI.DrawRectangle(spriteBatch,
                    meterPos,
                    new Vector2(container.Rect.Width, (container.Rect.Bottom - container.Rect.Height * optimalTemperature.Y / 100.0f) - container.Rect.Y),
                    warningColor * (float)Math.Sin(Timing.TotalTime * 5.0f) * 0.7f, isFilled: true);
            }
            if (temperature < optimalTemperature.X)
            {
                GUI.DrawRectangle(spriteBatch,
                    new Vector2(meterPos.X, container.Rect.Bottom - container.Rect.Height * optimalTemperature.X / 100.0f),
                    new Vector2(container.Rect.Width, container.Rect.Bottom - (container.Rect.Bottom - container.Rect.Height * optimalTemperature.X / 100.0f)),
                    warningColor * (float)Math.Sin(Timing.TotalTime * 5.0f) * 0.7f, isFilled: true);
            }

            float tempRangeIndicatorScale = container.Rect.Width / (float)tempRangeIndicator.SourceRect.Width;
            tempRangeIndicator.Draw(spriteBatch, new Vector2(container.Center.X, container.Rect.Bottom - container.Rect.Height * lubricant / 100.0f), Color.White, tempRangeIndicator.Origin, 0, scale: tempRangeIndicatorScale);
            tempRangeIndicator.Draw(spriteBatch, new Vector2(container.Center.X, container.Rect.Bottom - container.Rect.Height * lubricant / 100.0f), Color.White, tempRangeIndicator.Origin, 0, scale: tempRangeIndicatorScale);*/
        }

        private void DrawTempMeter(SpriteBatch spriteBatch, GUICustomComponent container)
        {
            
            Vector2 meterPos = new Vector2(container.Rect.X, container.Rect.Y);
            Vector2 meterScale = new Vector2(container.Rect.Width / (float)tempMeterFrame.SourceRect.Width, container.Rect.Height / (float)tempMeterFrame.SourceRect.Height);
            tempMeterFrame.Draw(spriteBatch, meterPos, Color.White, tempMeterFrame.Origin, 0.0f, scale: meterScale);

            float tempFill = temperature / 1000.0f;
            float meterBarScale = container.Rect.Width / (float)tempMeterBar.SourceRect.Width;
            Vector2 meterBarPos = new Vector2(container.Center.X, container.Rect.Bottom - tempMeterBar.size.Y * meterBarScale - (int)(5 * GUI.yScale));
            while (meterBarPos.Y > container.Rect.Bottom + (int)(5 * GUI.yScale) - container.Rect.Height * tempFill)
            {
                float tempRatio = 1.0f - ((meterBarPos.Y - container.Rect.Y) / container.Rect.Height);
                Color color = ToolBox.GradientLerp(tempRatio, temperatureColors);
                tempMeterBar.Draw(spriteBatch, meterBarPos, color: color, scale: meterBarScale);
                int spacing = 2;
                meterBarPos.Y -= tempMeterBar.size.Y * meterBarScale + spacing;
            }

            /*if (temperature > optimalTemperature.Y)
            {
                GUI.DrawRectangle(spriteBatch,
                    meterPos,
                    new Vector2(container.Rect.Width, (container.Rect.Bottom - container.Rect.Height * optimalTemperature.Y / 100.0f) - container.Rect.Y),
                    warningColor * (float)Math.Sin(Timing.TotalTime * 5.0f) * 0.7f, isFilled: true);
            }
            if (temperature < optimalTemperature.X)
            {
                GUI.DrawRectangle(spriteBatch,
                    new Vector2(meterPos.X, container.Rect.Bottom - container.Rect.Height * optimalTemperature.X / 100.0f),
                    new Vector2(container.Rect.Width, container.Rect.Bottom - (container.Rect.Bottom - container.Rect.Height * optimalTemperature.X / 100.0f)),
                    warningColor * (float)Math.Sin(Timing.TotalTime * 5.0f) * 0.7f, isFilled: true);
            }*/

            float tempRangeIndicatorScale = container.Rect.Width / (float)tempRangeIndicator.SourceRect.Width;
            tempRangeIndicator.Draw(spriteBatch, new Vector2(container.Center.X, container.Rect.Bottom - container.Rect.Height * 0.8f), Color.White, tempRangeIndicator.Origin, 0, scale: tempRangeIndicatorScale);
            tempRangeIndicator.Draw(spriteBatch, new Vector2(container.Center.X, container.Rect.Bottom - container.Rect.Height * 0.5f), Color.White, tempRangeIndicator.Origin, 0, scale: tempRangeIndicatorScale);
        }

        private void DrawFuelMeter(SpriteBatch spriteBatch, GUICustomComponent container)
        {
            
            Vector2 meterPos = new Vector2(container.Rect.X, container.Rect.Y);
            Vector2 meterScale = new Vector2(container.Rect.Width / (float)tempMeterFrame.SourceRect.Width, container.Rect.Height / (float)tempMeterFrame.SourceRect.Height);
            tempMeterFrame.Draw(spriteBatch, meterPos, Color.White, tempMeterFrame.Origin, 0.0f, scale: meterScale);

            float tempFill = ammonia / ammoniaMax;
            float meterBarScale = container.Rect.Width / (float)tempMeterBar.SourceRect.Width;
            Vector2 meterBarPos = new Vector2(container.Center.X, container.Rect.Bottom - tempMeterBar.size.Y * meterBarScale - (int)(5 * GUI.yScale));
            while (meterBarPos.Y > container.Rect.Bottom + (int)(5 * GUI.yScale) - container.Rect.Height * tempFill)
            {
                float tempRatio = 1.0f - ((meterBarPos.Y - container.Rect.Y) / container.Rect.Height);
                
                tempMeterBar.Draw(spriteBatch, meterBarPos, color: Color.LightBlue, scale: meterBarScale);
                int spacing = 2;
                meterBarPos.Y -= tempMeterBar.size.Y * meterBarScale + spacing;
            }

            /*if (temperature > optimalTemperature.Y)
            {
                GUI.DrawRectangle(spriteBatch,
                    meterPos,
                    new Vector2(container.Rect.Width, (container.Rect.Bottom - container.Rect.Height * optimalTemperature.Y / 100.0f) - container.Rect.Y),
                    warningColor * (float)Math.Sin(Timing.TotalTime * 5.0f) * 0.7f, isFilled: true);
            }
            if (temperature < optimalTemperature.X)
            {
                GUI.DrawRectangle(spriteBatch,
                    new Vector2(meterPos.X, container.Rect.Bottom - container.Rect.Height * optimalTemperature.X / 100.0f),
                    new Vector2(container.Rect.Width, container.Rect.Bottom - (container.Rect.Bottom - container.Rect.Height * optimalTemperature.X / 100.0f)),
                    warningColor * (float)Math.Sin(Timing.TotalTime * 5.0f) * 0.7f, isFilled: true);
            }

            float tempRangeIndicatorScale = container.Rect.Width / (float)tempRangeIndicator.SourceRect.Width;
            tempRangeIndicator.Draw(spriteBatch, new Vector2(container.Center.X, container.Rect.Bottom - container.Rect.Height * lubricant / 100.0f), Color.White, tempRangeIndicator.Origin, 0, scale: tempRangeIndicatorScale);
            tempRangeIndicator.Draw(spriteBatch, new Vector2(container.Center.X, container.Rect.Bottom - container.Rect.Height * lubricant / 100.0f), Color.White, tempRangeIndicator.Origin, 0, scale: tempRangeIndicatorScale);*/
        }

        private void DrawSimpleBackground(SpriteBatch spriteBatch, GUICustomComponent customContainer, Sprite meterSprite, float factor)
        {
            Rectangle rect = customContainer.Rect;
            float scale = Math.Min(rect.Width / meterSprite.size.X, rect.Height / meterSprite.size.Y) * factor;
            Vector2 pos = new Vector2(rect.Center.X, rect.Y + meterSprite.Origin.Y * scale);
            meterSprite.Draw(spriteBatch, pos, 0, scale);
        }

        private void DrawUIBackground(SpriteBatch spriteBatch, GUICustomComponent customContainer, Sprite meterSprite, float factor)
        {
            Rectangle rect = customContainer.Rect;
            float scale = Math.Min(rect.Width / meterSprite.size.X, rect.Height / meterSprite.size.Y) * factor;
            Vector2 pos = new Vector2(rect.Center.X, rect.Y + meterSprite.Origin.Y * scale);
            meterSprite.Draw(spriteBatch, pos, 0, scale);
        }

        partial void UpdateAnimation(float deltaTime)
        {
            if (propellerSprite == null) { return; }
            spriteIndex += (force / 100.0f) * AnimSpeed * deltaTime;
            if (spriteIndex < 0)
            {
                spriteIndex = propellerSprite.FrameCount;
            }
            if (spriteIndex >= propellerSprite.FrameCount)
            {
                spriteIndex = 0.0f;
            }
        }
        
    }

    
}
