﻿using Barotrauma.Extensions;
using Barotrauma;
using Barotrauma.Items.Components;
using Barotrauma.Networking;
using Networking;

using FarseerPhysics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace BarotraumaDieHard
{
    partial class MiniMapLegacy : Powered
    {
        private GUIFrame submarineContainer;

        private GUIFrame hullInfoFrame;

        private GUITextBlock hullNameText, hullBreachText, hullAirQualityText, hullWaterText, hullCO2Text, hullCOText, hullChlorineText, lockRoomHitText, temperatureText;

        private string noPowerTip = "";

        private readonly List<Submarine> displayedSubs = new List<Submarine>();

        private Point prevResolution;

        partial void InitProjSpecific(ContentXElement element)
        {
            noPowerTip = TextManager.Get("SteeringNoPowerTip").Value;
            CreateGUI();
        }

        public override void CreateGUI()
        {
            GuiFrame.RectTransform.RelativeOffset = new Vector2(0.05f, 0.0f);
            GuiFrame.CanBeFocused = true;
            new GUICustomComponent(new RectTransform(GuiFrame.Rect.Size - GUIStyle.ItemFrameMargin, GuiFrame.RectTransform, Anchor.Center) { AbsoluteOffset = GUIStyle.ItemFrameOffset },
                DrawHUDBack, null);
            submarineContainer = new GUIFrame(new RectTransform(new Vector2(0.95f, 0.9f), GuiFrame.RectTransform, Anchor.Center), style: null);

            new GUICustomComponent(new RectTransform(GuiFrame.Rect.Size - GUIStyle.ItemFrameMargin, GuiFrame.RectTransform, Anchor.Center) { AbsoluteOffset = GUIStyle.ItemFrameOffset },
                DrawHUDFront, null)
            {
                CanBeFocused = false
            };

            hullInfoFrame = new GUIFrame(new RectTransform(new Vector2(0.13f, 0.13f), GUI.Canvas, minSize: new Point(250, 150)),
                style: "GUIToolTip")
            {
                CanBeFocused = false
            };
            var hullInfoContainer = new GUILayoutGroup(new RectTransform(new Vector2(0.9f, 0.9f), hullInfoFrame.RectTransform, Anchor.Center))
            {
                Stretch = true,
                RelativeSpacing = 0.05f
            };

            hullNameText = new GUITextBlock(new RectTransform(new Vector2(1.0f, 0.4f), hullInfoContainer.RectTransform), "") { Wrap = true };
            hullBreachText = new GUITextBlock(new RectTransform(new Vector2(1.0f, 0.3f), hullInfoContainer.RectTransform), "") { Wrap = true };
            hullAirQualityText = new GUITextBlock(new RectTransform(new Vector2(1.0f, 0.3f), hullInfoContainer.RectTransform), "") { Wrap = true };
            hullWaterText = new GUITextBlock(new RectTransform(new Vector2(1.0f, 0.3f), hullInfoContainer.RectTransform), "") { Wrap = true };

            //moded part
            hullCO2Text = new GUITextBlock(new RectTransform(new Vector2(1.0f, 0.3f), hullInfoContainer.RectTransform), "") { Wrap = true };
            hullCOText = new GUITextBlock(new RectTransform(new Vector2(1.0f, 0.3f), hullInfoContainer.RectTransform), "") { Wrap = true };
            hullChlorineText = new GUITextBlock(new RectTransform(new Vector2(1.0f, 0.3f), hullInfoContainer.RectTransform), "") { Wrap = true };
            
            temperatureText = new GUITextBlock(new RectTransform(new Vector2(1.0f, 0.6f), hullInfoContainer.RectTransform), "") { Wrap = true };
            lockRoomHitText = new GUITextBlock(new RectTransform(new Vector2(1.0f, 0.9f), hullInfoContainer.RectTransform), "") { Wrap = true };


            hullInfoFrame.Children.ForEach(c =>
            {
                c.CanBeFocused = false;
                c.Children.ForEach(c2 => c2.CanBeFocused = false);
            });
        }

        public override void AddToGUIUpdateList(int order = 0)
        {
            base.AddToGUIUpdateList(order);
            hullInfoFrame?.AddToGUIUpdateList(order: order + 1);
            
        }

        private void CreateHUD()
        {
            prevResolution = new Point(GameMain.GraphicsWidth, GameMain.GraphicsHeight);
            submarineContainer?.ClearChildren();

            if (item.Submarine == null) { return; }

            item.Submarine.CreateMiniMap(submarineContainer);
            displayedSubs.Clear();
            displayedSubs.Add(item.Submarine);
            displayedSubs.AddRange(item.Submarine.DockedTo);
        }

        public override void UpdateHUDComponentSpecific(Character character, float deltaTime, Camera cam)
        {
            //recreate HUD if the subs we should display have changed
            if ((item.Submarine == null && displayedSubs.Count > 0) ||                                       //item not inside a sub anymore, but display is still showing subs
                !displayedSubs.Contains(item.Submarine) ||                                                   //current sub not displayer
                prevResolution.X != GameMain.GraphicsWidth || prevResolution.Y != GameMain.GraphicsHeight || //resolution changed
                item.Submarine.DockedTo.Any(s => !displayedSubs.Contains(s)) ||                              //some of the docked subs not diplayed
                !submarineContainer.Children.Any() ||                                                        // We lack a GUI
                displayedSubs.Any(s => s != item.Submarine && !item.Submarine.DockedTo.Contains(s)))         //displaying a sub that shouldn't be displayed
            {
                CreateHUD();
            }
            
            float distort = 1.0f - item.Condition / item.MaxCondition;
            foreach (HullData hullData in hullDatas.Values)
            {
                hullData.DistortionTimer -= deltaTime;
                if (hullData.DistortionTimer <= 0.0f)
                {
                    hullData.Distort = Rand.Range(0.0f, 1.0f) < distort * distort;
                    if (hullData.Distort)
                    {
                        hullData.Oxygen = Rand.Range(0.0f, 100.0f);
                        hullData.Water = Rand.Range(0.0f, 1.0f);
                    }
                    hullData.DistortionTimer = Rand.Range(1.0f, 10.0f);
                }
            }
        }

		
        private void DrawHUDFront(SpriteBatch spriteBatch, GUICustomComponent container)
        {
            if (Voltage < MinVoltage)
            {
                Vector2 textSize = GUIStyle.Font.MeasureString(noPowerTip);
                Vector2 textPos = GuiFrame.Rect.Center.ToVector2();

                GUI.DrawString(spriteBatch, textPos - textSize / 2, noPowerTip,
                               GUIStyle.Orange * (float)Math.Abs(Math.Sin(Timing.TotalTime)), Color.Black * 0.8f, font: GUIStyle.SubHeadingFont);
                return;
            }
            if (!submarineContainer.Children.Any()) { return; }
            foreach (GUIComponent child in submarineContainer.Children.FirstOrDefault()?.Children)
            {
                if (child.UserData is Hull hull)
                {
                    if (hull.Submarine == null || !hull.Submarine.Info.IsOutpost) { continue; }
                    string text = TextManager.GetWithVariable("MiniMapOutpostDockingInfo", "[outpost]", hull.Submarine.Info.Name).Value;
                    Vector2 textSize = GUIStyle.Font.MeasureString(text);
                    Vector2 textPos = child.Center;
                    if (textPos.X + textSize.X / 2 > submarineContainer.Rect.Right)
                        textPos.X -= ((textPos.X + textSize.X / 2) - submarineContainer.Rect.Right) + 10 * GUI.xScale;
                    if (textPos.X - textSize.X / 2 < submarineContainer.Rect.X)
                        textPos.X += (submarineContainer.Rect.X - (textPos.X - textSize.X / 2)) + 10 * GUI.xScale;
                    GUI.DrawString(spriteBatch, textPos - textSize / 2, text,
                       GUIStyle.Orange * (float)Math.Abs(Math.Sin(Timing.TotalTime)), Color.Black * 0.8f);

                      
                    break;
                }
            }
            

            
        }

        // Manual adjustment factors
        float xAdjuster = 1.0f; // Adjust the horizontal position scaling
        float yAdjuster = 1.0f; // Adjust the vertical position scaling
        float xOffset = 20.0f;   // Manual X offset
        float yOffset = 15.0f;   // Manual Y offset

        private void DrawPersonalIndicators(SpriteBatch spriteBatch, Hull hull)
        {
            var hullFrame = submarineContainer.Children.FirstOrDefault()?.FindChild(hull);
            if (hullFrame == null) { return; }

            // Get the world size of the hull and the screen size of the hull frame
            Vector2 hullWorldSize = hull.Rect.Size.ToVector2();
            Vector2 hullScreenSize = hullFrame.Rect.Size.ToVector2();

            // Calculate the scaling ratio
            Vector2 scaleRatio = hullScreenSize / hullWorldSize;

            foreach (Character character in Character.CharacterList)
            {
                if (character.CurrentHull != hull || character.CurrentHull.Submarine !=Submarine.MainSub) { continue; }

                // Calculate the character's position relative to the hull dimensions
                Vector2 relativePos = (character.WorldPosition - hull.WorldPosition) / hullWorldSize;

                // Map the relative position to the hullFrame's screen position and apply scaling with manual adjustments
                Vector2 indicatorPos = new Vector2(
                    hullFrame.Rect.X + (hullFrame.Rect.Width * relativePos.X * scaleRatio.X * xAdjuster) + xOffset,
                    hullFrame.Rect.Y + (hullFrame.Rect.Height * (1 - relativePos.Y) * scaleRatio.Y * yAdjuster) + yOffset // Invert Y axis
                );

                // Draw a square to indicate character's position
                Rectangle indicatorRect = new Rectangle((int)indicatorPos.X - 5, (int)indicatorPos.Y - 5, 10, 10);
                spriteBatch.Draw(GUI.WhiteTexture, indicatorRect, Color.Green);
            }
            
        }

        

    
        // Function triggered on click
        private void OnHullClick(Hull clickedHull)
        {
            
            // Lock all doors linked to gaps
            foreach (Gap gap in clickedHull.ConnectedGaps)
            {
                if (gap.IsRoomToRoom)
                {
                    var door = gap.ConnectedDoor; // Get the connected door

                    if (door != null && door.Item.InPlayerSubmarine)
                    {
                        if (door.OpenState == 0f)
                        {
                            door.IsJammed = true;
                            SendDoorJammedMessage(door.Item, true); // Notify other clients of the jammed state
                        }
                        
                        else if (door.OpenState == 1f)
                        {
                            door.SetState(false, true, true, false); // I think using this method can network the door state.
                            door.IsJammed = true;
                            SendDoorJammedMessage(door.Item, true); // Notify other clients of the jammed state
                        }
                        
                    }
                }
            }
        }

        private void UnlockAllDoors(Hull clickedHull)
        {
            foreach (Gap gap in clickedHull.ConnectedGaps)
            {
                if (gap.IsRoomToRoom)
                {
                    var door = gap.ConnectedDoor; // Get the connected door

                    if (door != null && door.Item.InPlayerSubmarine)
                    {
                        door.IsJammed = false; // Free the door
                        SendDoorJammedMessage(door.Item, false); // Notify other clients of the jammed state
                    }
                }
            }
        }

        

        private void DrawHUDBack(SpriteBatch spriteBatch, GUICustomComponent container)
        {
			
			float flashSpeed = 5.0f; // Adjust this for faster/slower flashes
			float flashIntensity = (float)Math.Sin(Timing.TotalTime * flashSpeed) * 0.5f + 0.5f;
			

            Hull mouseOnHull = null;
            hullInfoFrame.Visible = false;
            
            
            foreach (Hull hull in Hull.HullList)
            {
                var hullFrame = submarineContainer.Children.FirstOrDefault()?.FindChild(hull);
                if (hullFrame == null) { continue; }

                bool allDoorsLocked = true; // Assume all doors are locked

                // Lock room feature starts
                foreach (Gap gap in hull.ConnectedGaps)
                {
                    if (gap.IsRoomToRoom)
                    {
                        var door = gap.ConnectedDoor;
                        if (door != null && door.Item.InPlayerSubmarine && !door.IsJammed)
                        {
                            allDoorsLocked = false; // Found an unlocked door
                            break; // Exit loop
                        }
                        // No need to check for a gap without a door,
                        // just ensure that if any doors are jammed, they count as locked.
                    }
                }

                // Draw the red square if all doors are locked
                if (allDoorsLocked && hull.Submarine == Submarine.MainSub)
                {
                    Rectangle lockedIndicatorRect = new Rectangle(
                        hullFrame.Rect.X,
                        hullFrame.Rect.Y,
                        10, // Width of the indicator
                        10  // Height of the indicator
                    );
                    spriteBatch.Draw(GUI.WhiteTexture, lockedIndicatorRect, Color.Red);
                }


                    if (GUI.MouseOn == hullFrame || hullFrame.IsParentOf(GUI.MouseOn))
                    {
                        mouseOnHull = hull;
                        hullFrame.Color = Color.White; // Highlight hull when hovering
                        
                        // Detect Mouse Click (Left Button)
                        if (PlayerInput.PrimaryMouseButtonClicked())
                        {
                            if (PlayerInput.KeyDown(Microsoft.Xna.Framework.Input.Keys.LeftControl)) // Check if Ctrl key is held down
                            {
                                
                                UnlockAllDoors(mouseOnHull); // Call the method to unlock all doors
                            }
                            else
                            {
                                OnHullClick(mouseOnHull); // Call the regular function when clicked
                            }
                        }
                    }
                // Lock room feature ends.

                if (item.Submarine == null || !hasPower)
                {
                    hullFrame.Color = Color.DarkCyan * 0.3f;
                    if (hullFrame.Children != null && hullFrame.Children.Any()) // Somehow using the legacy version needs to check if the content is there. Otherwise will crash the game.
					{
						hullFrame.Children.First().Color = Color.DarkCyan * 0.3f;
					}
                }

                // Call to draw personal indicators for characters inside the current hull
                DrawPersonalIndicators(spriteBatch, hull);
                
            }
			

            if (Voltage < MinVoltage)
            {
                return;
            }

            float scale = 1.0f;
            HashSet<Submarine> subs = new HashSet<Submarine>();
            foreach (Hull hull in Hull.HullList)
            {
                if (hull.Submarine == null) { continue; }
                var hullFrame = submarineContainer.Children.FirstOrDefault()?.FindChild(hull);
                if (hullFrame == null) { continue; }

                hullFrame.Visible = true;
                if (!submarineContainer.Rect.Contains(hullFrame.Rect))
                {
                    if (hull.Submarine.Info.Type != SubmarineType.Player) 
                    {
                        hullFrame.Visible = false;
                        continue; 
                    }
                }

                hullDatas.TryGetValue(hull, out HullData hullData);
                if (hullData == null)
                {
                    hullData = new HullData();
                    GetLinkedHulls(hull, hullData.LinkedHulls);
                    hullDatas.Add(hull, hullData);
                }
                
                Color neutralColor = Color.DarkCyan;
                if (hull.IsWetRoom)
                {
                    neutralColor = new Color(9, 80, 159);
                }

                if (hullData.Distort && hullFrame.Children != null && hullFrame.Children.Any())
                {
                    hullFrame.Children.First().Color = Color.Lerp(Color.Black, Color.DarkGray * 0.5f, Rand.Range(0.0f, 1.0f));
                    hullFrame.Color = neutralColor * 0.5f;
                    continue;
                }
                
                subs.Add(hull.Submarine);
                scale = Math.Min(
                    hullFrame.Parent.Rect.Width / (float)hull.Submarine.Borders.Width, 
                    hullFrame.Parent.Rect.Height / (float)hull.Submarine.Borders.Height);
                
                Color borderColor = neutralColor;
                
                float? gapOpenSum = 0.0f;
                if (ShowHullIntegrity)
                {
                    gapOpenSum = hull.ConnectedGaps.Where(g => !g.IsRoomToRoom).Sum(g => g.Open);
                    borderColor = Color.Lerp(neutralColor, GUIStyle.Red, Math.Min((float)gapOpenSum, 1.0f));
                }

                float? oxygenAmount = null;
                if (!RequireOxygenDetectors || hullData?.Oxygen != null)
                {
                    oxygenAmount = RequireOxygenDetectors ? hullData.Oxygen : hull.OxygenPercentage;
                    GUI.DrawRectangle(
                        spriteBatch, hullFrame.Rect, 
                        Color.Lerp(GUIStyle.Red * 0.5f, GUIStyle.Green * 0.3f, (float)oxygenAmount / 100.0f), 
                        true);
                }

                float? waterAmount = null;
                if (!RequireWaterDetectors || hullData.Water != null)
                {
                    waterAmount = RequireWaterDetectors ? hullData.Water : Math.Min(hull.WaterVolume / hull.Volume, 1.0f);
                    if (hullFrame.Rect.Height * waterAmount > 3.0f)
                    {
                        Rectangle waterRect = new Rectangle(
                            hullFrame.Rect.X, (int)(hullFrame.Rect.Y + hullFrame.Rect.Height * (1.0f - waterAmount)),
                            hullFrame.Rect.Width, (int)(hullFrame.Rect.Height * waterAmount));

                        waterRect.Inflate(-3, -3);

                        GUI.DrawRectangle(spriteBatch, waterRect, new Color(85, 136, 147), true);
                        GUI.DrawLine(spriteBatch, new Vector2(waterRect.X, waterRect.Y), new Vector2(waterRect.Right, waterRect.Y), Color.LightBlue);
                    }
                }

                if (mouseOnHull == hull ||
                    hullData.LinkedHulls.Contains(mouseOnHull))
                {
					if (hullFrame.Children != null && hullFrame.Children.Any())
					{
                    borderColor = Color.Lerp(borderColor, Color.White, 0.5f);
                    hullFrame.Children.First().Color = Color.White;
                    hullFrame.Color = borderColor;
					}
                }
                else
                {
					if (hullFrame.Children != null && hullFrame.Children.Any())
					{
                    hullFrame.Children.First().Color = neutralColor * 0.8f;
					}
                }

                if (mouseOnHull == hull)
                {
                    hullInfoFrame.RectTransform.ScreenSpaceOffset = hullFrame.Rect.Center;
                    if (hullInfoFrame.Rect.Right > GameMain.GraphicsWidth) { hullInfoFrame.RectTransform.ScreenSpaceOffset -= new Point(hullInfoFrame.Rect.Width, 0); }
                    if (hullInfoFrame.Rect.Bottom > GameMain.GraphicsHeight) { hullInfoFrame.RectTransform.ScreenSpaceOffset -= new Point(0, hullInfoFrame.Rect.Height); }

                    hullInfoFrame.Visible = true;
                    hullNameText.Text = hull.DisplayName;


                    foreach (Hull linkedHull in hullData.LinkedHulls)
                    {
                        gapOpenSum += linkedHull.ConnectedGaps.Where(g => !g.IsRoomToRoom).Sum(g => g.Open);
                        oxygenAmount += linkedHull.OxygenPercentage;
                        waterAmount += Math.Min(linkedHull.WaterVolume / linkedHull.Volume, 1.0f);

                    }
                    oxygenAmount /= (hullData.LinkedHulls.Count + 1);
                    waterAmount /= (hullData.LinkedHulls.Count + 1);

                    hullBreachText.Text = gapOpenSum > 0.1f ? TextManager.Get("MiniMapHullBreach") : "";
                    hullBreachText.TextColor = GUIStyle.Red;

                    hullAirQualityText.Text = oxygenAmount == null ? TextManager.Get("MiniMapAirQualityUnavailable") :
                        TextManager.AddPunctuation(':', TextManager.Get("MiniMapAirQuality"), + (int)oxygenAmount + " %");
                    hullAirQualityText.TextColor = oxygenAmount == null ? GUIStyle.Red : Color.Lerp(GUIStyle.Red, Color.LightGreen, (float)oxygenAmount / 100.0f);

                    hullWaterText.Text = waterAmount == null ? TextManager.Get("MiniMapWaterLevelUnavailable") : 
                        TextManager.AddPunctuation(':', TextManager.Get("MiniMapWaterLevel"), (int)(waterAmount * 100.0f) + " %");
                    hullWaterText.TextColor = waterAmount == null ? GUIStyle.Red : Color.Lerp(Color.LightGreen, GUIStyle.Red, (float)waterAmount);
                    
                    
                            
                            float co2Amount = HullMod.GetGas(hull, "CO2");
                            float coAmount = HullMod.GetGas(hull, "CO");
                            float chlorineAmount = HullMod.GetGas(hull, "Chlorine");
                            float temperature = HullMod.GetGas(hull, "Temperature");

                            float celsiusTemperature = (float)temperature - 273.15f;
                            string formattedTemperature = celsiusTemperature.ToString("0.0") + " °C";
                        

                    temperatureText.Text = temperature == null ? TextManager.Get("MiniMapAirQualityUnavailable") :
                    TextManager.AddPunctuation(':', TextManager.Get("MiniMapTemperature"), formattedTemperature);
                    


                    hullCO2Text.Text = co2Amount == null ? TextManager.Get("MiniMapAirQualityUnavailable") :
                    TextManager.AddPunctuation(':', TextManager.Get("MiniMapCO2"), (int)co2Amount + " ppm");
                    hullCO2Text.TextColor = co2Amount == null ? GUIStyle.Red : Color.Lerp(GUIStyle.Green, Color.Red, (float)co2Amount / 100.0f);

                    hullCOText.Text = coAmount == null ? TextManager.Get("MiniMapAirQualityUnavailable") :
                        TextManager.AddPunctuation(':', TextManager.Get("MiniMapCO"), (int)coAmount + " ppm");
                    hullCOText.TextColor = coAmount == null ? GUIStyle.Red : Color.Lerp(GUIStyle.Green, Color.Red, (float)coAmount / 100.0f);

                    hullChlorineText.Text = chlorineAmount == null ? TextManager.Get("MiniMapAirQualityUnavailable") :
                        TextManager.AddPunctuation(':', TextManager.Get("MiniMapChlorine"), (int)chlorineAmount + " ppm");
                    hullChlorineText.TextColor = chlorineAmount == null ? GUIStyle.Red : Color.Lerp(GUIStyle.Green, Color.Red, (float)chlorineAmount / 100.0f);


                    lockRoomHitText.Text = TextManager.Get("MiniMapLockRoomHit");
                    
                }
                
                hullFrame.Color = borderColor;
            }
            
            foreach (Submarine sub in subs)
            {
                if (sub.HullVertices == null || sub.Info.IsOutpost) { continue; }
                
                Rectangle worldBorders = sub.GetDockedBorders();
                worldBorders.Location += sub.WorldPosition.ToPoint();
                
                scale = Math.Min(
                    submarineContainer.Rect.Width / (float)worldBorders.Width,
                    submarineContainer.Rect.Height / (float)worldBorders.Height) * 0.9f;

                float displayScale = ConvertUnits.ToDisplayUnits(scale);
                Vector2 offset = ConvertUnits.ToSimUnits(sub.WorldPosition - new Vector2(worldBorders.Center.X, worldBorders.Y - worldBorders.Height / 2));
                Vector2 center = container.Rect.Center.ToVector2();
                
                for (int i = 0; i < sub.HullVertices.Count; i++)
                {
                    Vector2 start = (sub.HullVertices[i] + offset) * displayScale;
                    start.Y = -start.Y;
                    Vector2 end = (sub.HullVertices[(i + 1) % sub.HullVertices.Count] + offset) * displayScale;
                    end.Y = -end.Y;
                    GUI.DrawLine(spriteBatch, center + start, center + end, Color.DarkCyan * Rand.Range(0.3f, 0.35f), width: (int)(10 * GUI.Scale));
                }
            }
        }

        private void GetLinkedHulls(Hull hull, List<Hull> linkedHulls)
        {
            foreach (var linkedEntity in hull.linkedTo)
            {
                if (linkedEntity is Hull linkedHull)
                {
                    if (linkedHulls.Contains(linkedHull)) { continue; }
                    linkedHulls.Add(linkedHull);
                    GetLinkedHulls(linkedHull, linkedHulls);
                }
            }
        }





        private static void SendDoorJammedMessage(Item item, bool isJammed)
        {
            IWriteMessage msg = NetUtil.CreateNetMsg(NetEvent.DOOR_JAMMED_STATE_CHANGE);

            msg.WriteUInt16(item.ID); // ID of the door item
            msg.WriteBoolean(isJammed); // Write the jammed state
            NetUtil.SendServer(msg, DeliveryMethod.Reliable);
            
        }

    }
}
