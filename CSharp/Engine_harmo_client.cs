using Microsoft.Xna.Framework;
using System;
using System.Globalization;
using System.Xml.Linq;
using Barotrauma.Networking;
using System.Collections.Generic; // for Dictionary

using Barotrauma.Items.Components;
using Microsoft.Xna.Framework.Graphics;
using Barotrauma;
using HarmonyLib;


namespace BarotraumaDieHard
{
    public partial class EngineMod : IAssemblyPlugin
    {
        private static GUIFrame mainPanel;
        private static Sprite fissionRateMeter;
        private static Sprite turbineOutputMeter;
        private static Sprite meterPointer;
        private static Sprite sectorSprite;
        private static Sprite tempMeterFrame;
        private static Sprite tempMeterBar;
        private static Sprite tempRangeIndicator;
        private static Sprite meterMatte;
        private static Sprite ui;
        private static Sprite verticalBarMeterBackground;
        //public delegate void DrawFissionRateMeterDelegate(SpriteBatch spriteBatch, GUICustomComponent container, Engine __instance);
        private readonly int updateGraphInterval = 500;
        private int graphTimer;

        private static Color optimalRangeColor = new Color(74,238,104,255);
        private static Color offRangeColor = Color.Orange;
        private static Color warningColor = Color.Red;
        private static readonly Color[] temperatureColors = new Color[] { Color.Blue, Color.LightBlue, Color.Orange, Color.Red };

        public static void InitProjSpecificPostfix(ContentXElement element, Engine __instance)
        {
            
            Engine _ = __instance;
            if (_.item == null) return;
            if (_.item.Prefab.Identifier != "engine") { return; } // right now only works on engine, otherwise can't load in multiplayer. TODO: update for all engines
            
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

            mainPanel = new GUIFrame(new RectTransform(new Vector2(3f, 4f), _.GuiFrame.RectTransform, Anchor.Center)
            {
                RelativeOffset = new Vector2(-0.6f, -1.5f)
                
            }, null);

            var uiBackground = new GUICustomComponent(new RectTransform(new Vector2(1.5f, 1.2f), mainPanel.RectTransform, Anchor.Center)
            {
                RelativeOffset = new Vector2(-0.5f, 0.0f)
            }, (spriteBatch, customContainer) => DrawUIBackground(spriteBatch, customContainer, ui, 1f));

            //var line = new GUIFrame(new RectTransform(new Vector2(1.0f, 1f), mainPanel.RectTransform), style: "HorizontalLine");
            var mattedBackground = new GUICustomComponent(new RectTransform(new Vector2(0.85f, 0.55f), mainPanel.RectTransform, Anchor.TopCenter)
            {
                RelativeOffset = new Vector2(-0.05f, 0.1f)
            }, (spriteBatch, customContainer) => DrawSimpleBackground(spriteBatch, customContainer, meterMatte, 1f));

            var forceMeter = new GUICustomComponent(new RectTransform(new Vector2(0.45f, 0.9f), mattedBackground.RectTransform, Anchor.Center)
            {
                RelativeOffset = new Vector2(-0.25f, 0f)
            },
                (spriteBatch, customContainer) => DrawForceMeterInit(spriteBatch, customContainer, _), null)
            {
                ToolTip = TextManager.Get("ReactorTipFissionRate")
            };
            var forceText = new GUITextBlock(new RectTransform(new Vector2(0.25f, 0.1f), forceMeter.RectTransform)
            {RelativeOffset = new Vector2(0.38f, 1.1f) },
            "ForceOutput", null, // null to use default color
            font: GUIStyle.SubHeadingFont,
            textAlignment: Alignment.Center
            );
            
            

            var turbineMeter = new GUICustomComponent(new RectTransform(new Vector2(0.45f, 0.9f), mattedBackground.RectTransform, Anchor.Center)
            {
                RelativeOffset = new Vector2(0.25f,0)
            },
                (spriteBatch, customContainer) => DrawTurbineOutputMeter(spriteBatch, customContainer, _), null)
            {
                ToolTip = TextManager.Get("ReactorTipFissionRate")
            };
            var turbineText = new GUITextBlock(new RectTransform(new Vector2(0.25f, 0.1f), turbineMeter.RectTransform)
            {RelativeOffset = new Vector2(0.38f, 1.1f) },
            "turbineoutput", null, // null to use default color
            font: GUIStyle.SubHeadingFont,
            textAlignment: Alignment.Center
            );

            var barMeterBackGround = new GUICustomComponent(new RectTransform(new Vector2(0.06f, 0.4f), mainPanel.RectTransform, Anchor.TopCenter)
            {
                RelativeOffset = new Vector2(-0.1f, 0.15f)
            }, (spriteBatch, customContainer) => DrawSimpleBackground(spriteBatch, customContainer, verticalBarMeterBackground, 1f));
            
            var lubricationMeter = new GUICustomComponent(new RectTransform(new Vector2(0.45f, 0.8f), barMeterBackGround.RectTransform, Anchor.TopCenter)
            {
                RelativeOffset = new Vector2(0f, 0.04f)
            },
                (spriteBatch, customContainer) => DrawLubMeter(spriteBatch, customContainer, _), null)
            {
                ToolTip = TextManager.Get("ReactorTipLubrication")
            };

            var barMeterBackGroundTempreature = new GUICustomComponent(new RectTransform(new Vector2(0.06f, 0.4f), mainPanel.RectTransform, Anchor.TopCenter)
            {
                RelativeOffset = new Vector2(-0.05f, 0.15f)
            }, (spriteBatch, customContainer) => DrawSimpleBackground(spriteBatch, customContainer, verticalBarMeterBackground, 1f));
            
            var tempMeter = new GUICustomComponent(new RectTransform(new Vector2(0.45f, 0.8f), barMeterBackGroundTempreature.RectTransform, Anchor.TopCenter)
            {
                RelativeOffset = new Vector2(0f, 0.04f)
            },
                (spriteBatch, customContainer) => DrawTempMeter(spriteBatch, customContainer, _), null)
            {
                ToolTip = TextManager.Get("ReactorTipLubrication")
            };
            var barMeterBackGroundHydrogen = new GUICustomComponent(new RectTransform(new Vector2(0.06f, 0.4f), mainPanel.RectTransform, Anchor.TopCenter)
            {
                RelativeOffset = new Vector2(0f, 0.15f)
            }, (spriteBatch, customContainer) => DrawSimpleBackground(spriteBatch, customContainer, verticalBarMeterBackground, 1f));
            
            var fuelMeter = new GUICustomComponent(new RectTransform(new Vector2(0.45f, 0.8f), barMeterBackGroundHydrogen.RectTransform, Anchor.TopCenter)
            {
                RelativeOffset = new Vector2(0f, 0.04f)
            },
                (spriteBatch, customContainer) => DrawFuelMeter(spriteBatch, customContainer, _), null)
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
                    //_.CurrentMode = button.Selected ? Sonar.Mode.Active : Sonar.Mode.Passive;
                    
                    if (GameMain.Client != null)
                    {
                        
                    }
                    return true;
                }
            };
        }
        
        private static void DrawForceMeterInit(SpriteBatch spriteBatch, GUICustomComponent container, Engine __instance)
        {
            Engine _ = __instance;
            if (_.item.Removed) { return; }

            //make the pointer jitter a bit if it's at the upper limit of the fission rate
            float jitter = 0.0f;
            if (_.force > _.maxForce - 5.0f)
            {
                float jitterAmount = 10f; //Math.Min(TargetFissionRate - allowedFissionRate.Y, 10.0f);
                //float t = graphTimer / updateGraphInterval;

                jitter = (PerlinNoise.GetPerlin( 0.5f,  0.1f) - 0.5f) * jitterAmount;
            }

            DrawForceMeter(spriteBatch, container.Rect,
                fissionRateMeter, _.force + jitter, new Vector2(0.0f, 100.0f), new Vector2(0.0f, 100.0f), new Vector2(0.0f, 100.0f), _.maxForce);

        }
        
        private static void DrawForceMeter(SpriteBatch spriteBatch, Rectangle rect, Sprite meterSprite, float value, Vector2 range, Vector2 optimalRange, Vector2 allowedRange, float maxForce)
        {
            float scale = Math.Min(rect.Width / meterSprite.size.X, rect.Height / meterSprite.size.Y);
            Vector2 pos = new Vector2(rect.Center.X, rect.Y + meterSprite.Origin.Y * scale);

            

            Vector2 pointerPos = pos - new Vector2(5, 220) * scale;
            Vector2 meterBackgroundPos = pos - new Vector2(0, 0) * scale;

            float scaleMultiplier = 0.95f;
            

            meterSprite.Draw(spriteBatch, pos, 0, scale);
            
            float adjustedAngle = MathHelper.Lerp(MathHelper.ToRadians(250f), MathHelper.ToRadians(900f), Math.Abs(value / maxForce)); 

            
            meterPointer.Draw(spriteBatch, pointerPos, adjustedAngle, scale);
        }


        private static void DrawTurbineOutputMeter(SpriteBatch spriteBatch, GUICustomComponent container, Engine __instance)
        {
            
            Engine _ = __instance;
            if (_.item.Removed) { return; }

            DebugConsole.NewMessage(_.force.ToString(), Color.White);
            DebugConsole.NewMessage(_.maxForce.ToString(), Color.White);
            
            //make the pointer jitter a bit if it's at the upper limit of the fission rate
            float jitter = 0.0f;
            if (_.force > _.maxForce - 115.0f)
            {
                DebugConsole.NewMessage("Jittering", Color.White);
                float jitterAmount = 100f; //Math.Min(TargetFissionRate - allowedFissionRate.Y, 10.0f);
                //float t = graphTimer / updateGraphInterval;

                jitter = (PerlinNoise.GetPerlin( 0.5f,  0.1f) - 0.5f) * jitterAmount;
            }

            // This draws the turbine output meter
            DrawTurbineMeter(spriteBatch, container.Rect,
                fissionRateMeter, _.force + jitter, new Vector2(0.0f, 100.0f), new Vector2(0.0f, 100.0f), new Vector2(0.0f, 100.0f), _.maxForce);

            
        }

        private static void DrawTurbineMeter(SpriteBatch spriteBatch, Rectangle rect, Sprite meterSprite, float value, Vector2 range, Vector2 optimalRange, Vector2 allowedRange, float maxForce)
        {
            float scale = Math.Min(rect.Width / meterSprite.size.X, rect.Height / meterSprite.size.Y);
            Vector2 pos = new Vector2(rect.Center.X, rect.Y + meterSprite.Origin.Y * scale);

            
            Vector2 pointerPos = pos - new Vector2(5, 220) * scale;
            Vector2 meterBackgroundPos = pos - new Vector2(0, 0) * scale;

            float scaleMultiplier = 0.95f;
            

            meterSprite.Draw(spriteBatch, pos, 0, scale);
            //float initialAngle = MathHelper.ToRadians(190f);
            float adjustedAngle = MathHelper.Lerp(MathHelper.ToRadians(250f), MathHelper.ToRadians(900f), Math.Abs((value / maxForce) / 2.0f));

            
            meterPointer.Draw(spriteBatch, pointerPos, adjustedAngle, scale);
        }


        private static void DrawLubMeter(SpriteBatch spriteBatch, GUICustomComponent container, Engine __instance)
        {
            Engine _ = __instance;
            Vector2 meterPos = new Vector2(container.Rect.X, container.Rect.Y);
            Vector2 meterScale = new Vector2(container.Rect.Width / (float)tempMeterFrame.SourceRect.Width, container.Rect.Height / (float)tempMeterFrame.SourceRect.Height);
            tempMeterFrame.Draw(spriteBatch, meterPos, Color.White, tempMeterFrame.Origin, 0.0f, scale: meterScale);

            float tempFill = lubrication[_.item.ID] / 100.0f;
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
            }*/

            float tempRangeIndicatorScale = container.Rect.Width / (float)tempRangeIndicator.SourceRect.Width;
            tempRangeIndicator.Draw(spriteBatch, new Vector2(container.Center.X, container.Rect.Bottom - container.Rect.Height * lubrication[_.item.ID] / 100.0f), Color.White, tempRangeIndicator.Origin, 0, scale: tempRangeIndicatorScale);
            tempRangeIndicator.Draw(spriteBatch, new Vector2(container.Center.X, container.Rect.Bottom - container.Rect.Height * lubrication[_.item.ID] / 100.0f), Color.White, tempRangeIndicator.Origin, 0, scale: tempRangeIndicatorScale);
        }

        private static void DrawTempMeter(SpriteBatch spriteBatch, GUICustomComponent container, Engine __instance)
        {
            Engine _ = __instance;
            Vector2 meterPos = new Vector2(container.Rect.X, container.Rect.Y);
            Vector2 meterScale = new Vector2(container.Rect.Width / (float)tempMeterFrame.SourceRect.Width, container.Rect.Height / (float)tempMeterFrame.SourceRect.Height);
            tempMeterFrame.Draw(spriteBatch, meterPos, Color.White, tempMeterFrame.Origin, 0.0f, scale: meterScale);

            float tempFill = engineInfo[_.item.ID].temperature / 100.0f;
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
            tempRangeIndicator.Draw(spriteBatch, new Vector2(container.Center.X, container.Rect.Bottom - container.Rect.Height * lubrication[_.item.ID] / 100.0f), Color.White, tempRangeIndicator.Origin, 0, scale: tempRangeIndicatorScale);
            tempRangeIndicator.Draw(spriteBatch, new Vector2(container.Center.X, container.Rect.Bottom - container.Rect.Height * lubrication[_.item.ID] / 100.0f), Color.White, tempRangeIndicator.Origin, 0, scale: tempRangeIndicatorScale);
        }

        private static void DrawFuelMeter(SpriteBatch spriteBatch, GUICustomComponent container, Engine __instance)
        {
            Engine _ = __instance;
            Vector2 meterPos = new Vector2(container.Rect.X, container.Rect.Y);
            Vector2 meterScale = new Vector2(container.Rect.Width / (float)tempMeterFrame.SourceRect.Width, container.Rect.Height / (float)tempMeterFrame.SourceRect.Height);
            tempMeterFrame.Draw(spriteBatch, meterPos, Color.White, tempMeterFrame.Origin, 0.0f, scale: meterScale);

            float tempFill = engineInfo[_.item.ID].hydrogen / engineInfo[_.item.ID].hydrogenMax;
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
            }*/

            float tempRangeIndicatorScale = container.Rect.Width / (float)tempRangeIndicator.SourceRect.Width;
            tempRangeIndicator.Draw(spriteBatch, new Vector2(container.Center.X, container.Rect.Bottom - container.Rect.Height * lubrication[_.item.ID] / 100.0f), Color.White, tempRangeIndicator.Origin, 0, scale: tempRangeIndicatorScale);
            tempRangeIndicator.Draw(spriteBatch, new Vector2(container.Center.X, container.Rect.Bottom - container.Rect.Height * lubrication[_.item.ID] / 100.0f), Color.White, tempRangeIndicator.Origin, 0, scale: tempRangeIndicatorScale);
        }

        private static void DrawSimpleBackground(SpriteBatch spriteBatch, GUICustomComponent customContainer, Sprite meterSprite, float factor)
        {
            Rectangle rect = customContainer.Rect;
            float scale = Math.Min(rect.Width / meterSprite.size.X, rect.Height / meterSprite.size.Y) * factor;
            Vector2 pos = new Vector2(rect.Center.X, rect.Y + meterSprite.Origin.Y * scale);
            meterSprite.Draw(spriteBatch, pos, 0, scale);
        }

        private static void DrawUIBackground(SpriteBatch spriteBatch, GUICustomComponent customContainer, Sprite meterSprite, float factor)
        {
            Rectangle rect = customContainer.Rect;
            float scale = Math.Min(rect.Width / meterSprite.size.X, rect.Height / meterSprite.size.Y) * factor;
            Vector2 pos = new Vector2(rect.Center.X, rect.Y + meterSprite.Origin.Y * scale);
            meterSprite.Draw(spriteBatch, pos, 0, scale);
        }

    }
}
