﻿using Barotrauma.Networking;

using Barotrauma.Items.Components;
using Barotrauma.Extensions;

using FarseerPhysics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

using System.Reflection;


using Barotrauma;
using HarmonyLib;
using System.Globalization;



namespace SonarMod
{
    partial class SonarMod : IAssemblyPlugin
    {
        
		public static float NewSectorAngle { get; set; } = 120.0f;

        // Calculate the dot product whenever NewSectorAngle is changed
        public static float NewDotProduct => (float)Math.Cos(MathHelper.ToRadians(NewSectorAngle) *0.5f);
        
        public static float steeringangleUpdate { get; private set; }
        public static bool lockDirectionalSonar = false;

        private static float minHertzValue = 30000f; // Minimum hertz value
        private static float maxHertzValue = 500000f; // Maximum hertz value

        private static float hertz = 30000f; // Default hertz value

        


		public static bool PingPrefix(Vector2 pingSource,  Vector2 transducerPos,  float pingRadius,  float prevPingRadius,  float displayScale,  float range,  bool passive,
             float pingStrength,  AITarget needsToBeInSector, Sonar __instance)
		{
			
			Sonar _ = __instance;
            float prevPingRadiusSqr = prevPingRadius * prevPingRadius;
            float pingRadiusSqr = pingRadius * pingRadius;
            
            
		
            foreach (Submarine submarine in Submarine.Loaded)
            {
				
				
                if (submarine.HullVertices == null) { continue; }
                //change: display the actual walls of own sub//nope, it messes up the passive
                /*if (!_.DetectSubmarineWalls)
                {
                    if (_.connectedSubs.Contains(submarine)) { continue; }                    
                }*/
                
                if ((_.CurrentMode == Sonar.Mode.Passive && submarine == Submarine.MainSub) || _.activePingsCount < 1) { continue; }//change: don't blip the main sub in passive mode. update: use ping to count the active ping. can only blip if there is an active ping
                
                
                //display the actual walls if the ping source is inside the sub (but not inside a hull, that's handled above)
                //only relevant in the end levels or maybe custom subs with some kind of non-hulled parts
                Rectangle worldBorders = submarine.GetDockedBorders();
                worldBorders.Location += submarine.WorldPosition.ToPoint();

                //this block display the submarine wall if the device is inside the sub
                /*if (Submarine.RectContains(worldBorders, pingSource))
                {
                    _.CreateBlipsForSubmarineWalls(submarine, pingSource, transducerPos, pingRadius, prevPingRadius, range, passive);
                    continue;
                }*/
                
                //this block is for displaying submarine in both sonar in the original game
                
                for (int i = 0; i < submarine.HullVertices.Count; i++)
                {
                    Vector2 start = FarseerPhysics.ConvertUnits.ToDisplayUnits(submarine.HullVertices[i]);
                    Vector2 end = FarseerPhysics.ConvertUnits.ToDisplayUnits(submarine.HullVertices[(i + 1) % submarine.HullVertices.Count]);

                    if (_.item.Submarine == submarine)
                    {
                        start += Rand.Vector(0.0f);//was 500, make it percise so the ping doesn't bounce on onw sub
                        end += Rand.Vector(0.0f);
                    }

                    _.CreateBlipsForLine(
                        start + submarine.WorldPosition,
                        end + submarine.WorldPosition,
                        pingSource, transducerPos,
                        pingRadius, prevPingRadius,
                        200.0f, 2.0f, range, 1.0f, passive, 
                        needsToBeInSector: needsToBeInSector);
                    }
                
            
			}

            if (Level.Loaded != null && _.item.CurrentHull != null)//remove the !detecsubwall' so it can function inside sub
            {
                if (Level.Loaded.Size.Y - pingSource.Y < range)
                {
                    _.CreateBlipsForLine(
                        new Vector2(pingSource.X - range, Level.Loaded.Size.Y),
                        new Vector2(pingSource.X + range, Level.Loaded.Size.Y),
                        pingSource, transducerPos,
                        pingRadius, prevPingRadius,
                        250.0f, 150.0f, range, pingStrength, passive, 
                        needsToBeInSector: needsToBeInSector);
                }
                if (pingSource.Y - Level.Loaded.BottomPos < range)
                {
                    _.CreateBlipsForLine(
                        new Vector2(pingSource.X - range, Level.Loaded.BottomPos),
                        new Vector2(pingSource.X + range, Level.Loaded.BottomPos),
                        pingSource, transducerPos,
                        pingRadius, prevPingRadius,
                        500.0f, 150.0f, range, pingStrength, passive, 
                        needsToBeInSector: needsToBeInSector);
                }

                List<Voronoi2.VoronoiCell> cells = Level.Loaded.GetCells(pingSource, 7);
                foreach (Voronoi2.VoronoiCell cell in cells)
                {
                    foreach (Voronoi2.GraphEdge edge in cell.Edges)
                    {
                        if (!edge.IsSolid) { continue; }
                        float cellDot = Vector2.Dot(cell.Center - pingSource, (edge.Center + cell.Translation) - cell.Center);
                        if (cellDot > 0) { continue; }

                        float facingDot = Vector2.Dot(
                            Vector2.Normalize(edge.Point1 - edge.Point2),
                            Vector2.Normalize(cell.Center - pingSource));

                        _.CreateBlipsForLine(
                            edge.Point1 + cell.Translation,
                            edge.Point2 + cell.Translation,
                            pingSource, transducerPos,
                            pingRadius, prevPingRadius,
                            350.0f, 3.0f * (Math.Abs(facingDot) + 1.0f), range, pingStrength, passive,
                            blipType : cell.IsDestructible ? Sonar.BlipType.Destructible : Sonar.BlipType.Default,
                            needsToBeInSector: needsToBeInSector);
                    }
                }
            }

            foreach (Item item in Item.SonarVisibleItems)
            {
                System.Diagnostics.Debug.Assert(_.item.Prefab.SonarSize > 0.0f);
                if (_.item.CurrentHull == null)
                {
                    float pointDist = ((_.item.WorldPosition - pingSource) * displayScale).LengthSquared();
                    if (pointDist > prevPingRadiusSqr && pointDist < pingRadiusSqr)
                    {
                        var blip = new SonarBlip(
                            _.item.WorldPosition + Rand.Vector(_.item.Prefab.SonarSize),
                            MathHelper.Clamp(_.item.Prefab.SonarSize, 0.1f, pingStrength),
                            MathHelper.Clamp(_.item.Prefab.SonarSize * 0.1f, 0.1f, 10.0f));
                        if (!IsVisible(blip)) { continue; }
                        _.sonarBlips.Add(blip);
                    }
                }
            }

            foreach (Character c in Character.CharacterList)
            {
                if (c.AnimController.CurrentHull != null || !c.Enabled) { continue; }
                if (!c.IsUnconscious && c.Params.HideInSonar) { continue; }
                if (_.DetectSubmarineWalls && c.AnimController.CurrentHull == null && _.item.CurrentHull != null) { continue; }

                if (c.AnimController.SimplePhysicsEnabled)
                {
                    float pointDist = ((c.WorldPosition - pingSource) * displayScale).LengthSquared();
                    if (pointDist > _.DisplayRadius * _.DisplayRadius) { continue; }

                    if (pointDist > prevPingRadiusSqr && pointDist < pingRadiusSqr)
                    {
                        var blip = new SonarBlip(
                            c.WorldPosition,
                            MathHelper.Clamp(c.Mass, 0.1f, pingStrength),
                            MathHelper.Clamp(c.Mass * 0.03f, 0.1f, 2.0f));
                        if (!IsVisible(blip)) { continue; }
                        _.sonarBlips.Add(blip);
                        HintManager.OnSonarSpottedCharacter(_.item, c);
                    }
                    continue;
                }

                foreach (Limb limb in c.AnimController.Limbs)
                {
                    if (!limb.body.Enabled) { continue; }

                    float pointDist = ((limb.WorldPosition - pingSource) * displayScale).LengthSquared();
                    if (limb.SimPosition == Vector2.Zero || pointDist > _.DisplayRadius * _.DisplayRadius) { continue; }

                    if (pointDist > prevPingRadiusSqr && pointDist < pingRadiusSqr)
                    {
                        var blip = new SonarBlip(
                            limb.WorldPosition + Rand.Vector(limb.Mass / 10.0f),
                            MathHelper.Clamp(limb.Mass, 0.1f, pingStrength),
                            MathHelper.Clamp(limb.Mass * 0.1f, 0.1f, 2.0f));
                        if (!IsVisible(blip)) { continue; }
                        _.sonarBlips.Add(blip);
                        HintManager.OnSonarSpottedCharacter(_.item, c);
                    }
                }

                
				
            }

            

            bool IsVisible(SonarBlip blip)
            {
                if (!passive && !_.CheckBlipVisibility(blip, transducerPos)) { return false; }
				
				if (!passive && !_.CheckBlipVisibility(blip, transducerPos)) { return false; }
				
                if (needsToBeInSector != null)
                {
                    if (!needsToBeInSector.IsWithinSector(blip.Position)) { return false; }
                }
                return true;
            }
			return false;


           
            
            
        }
	
	public static bool DrawSonar(SpriteBatch spriteBatch, Rectangle rect, Sonar __instance)
		{
			
			Sonar _ = __instance;
            _.displayBorderSize = 0.2f;
            _.center = rect.Center.ToVector2();
            _.DisplayRadius = (rect.Width / 2.0f) * (1.0f - _.displayBorderSize);
            _.DisplayScale = _.DisplayRadius / _.range * _.zoom;

            _.screenBackground?.Draw(spriteBatch, _.center, 0.0f, rect.Width / _.screenBackground.size.X);

             

            if (_.useDirectionalPing)
            {
                _.directionalPingBackground?.Draw(spriteBatch, _.center, 0.0f, rect.Width / _.directionalPingBackground.size.X);//the directional ping background image (green circle)
                if (_.directionalPingButton != null)
                {
                    int buttonSprIndex = 0;
                    if (_.pingDragDirection != null)
                    {
                        buttonSprIndex = 2;
                    }
                    else if (_.MouseInDirectionalPingRing(rect, true))
                    {
                        buttonSprIndex = 1;
                    }

                    _.directionalPingButton[buttonSprIndex]?.Draw(spriteBatch, _.center, MathUtils.VectorToAngle(_.pingDirection), rect.Width / _.directionalPingBackground.size.X);//the directional ping direction control image

                }
            }

            if (_.currentPingIndex != -1)
            {
                var activePing = _.activePings[_.currentPingIndex];
                if (activePing.IsDirectional && _.directionalPingCircle != null)
                {
                    _.directionalPingCircle.Draw(spriteBatch, _.center, Color.White * (1.0f - activePing.State),
                        rotate: MathUtils.VectorToAngle(activePing.Direction),
                        scale: _.DisplayRadius / _.directionalPingCircle.size.X * activePing.State);//directional ping circle, pure visual effect
                }
                else
                {
                    _.pingCircle.Draw(spriteBatch, _.center, Color.White * (1.0f - activePing.State), 0.0f, (_.DisplayRadius * 2 / _.pingCircle.size.X) * activePing.State);//full circle, pure visual effect
                }
            }

            float signalStrength = 1.0f;
            if (_.UseTransducers)
            {
                signalStrength = 0.0f;
                foreach (Sonar.ConnectedTransducer connectedTransducer in _.connectedTransducers)
                {
                    signalStrength = Math.Max(signalStrength, connectedTransducer.SignalStrength);
                }
            }

            Vector2 transducerCenter = _.GetTransducerPos();// + DisplayOffset;

            if (_.sonarBlips.Count > 0)
            {
                float blipScale = 0.08f * (float)Math.Sqrt(_.zoom) * (rect.Width / 700.0f);
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive);

                foreach (SonarBlip sonarBlip in _.sonarBlips)
                {
                    _.DrawBlip(spriteBatch, sonarBlip, transducerCenter + _.DisplayOffset, _.center, sonarBlip.FadeTimer / 2.0f * signalStrength, blipScale);
                }

                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied);
            }

            if (_.item.Submarine != null && !_.DetectSubmarineWalls)
            {
                transducerCenter += _.DisplayOffset;
                _.DrawDockingPorts(spriteBatch, transducerCenter, signalStrength);
                _.DrawOwnSubmarineBorders(spriteBatch, transducerCenter, signalStrength);
            }
            else//change: doesn't matter detectsubwall or not draw the sub borders
            {
                transducerCenter += _.DisplayOffset;
                _.DrawDockingPorts(spriteBatch, transducerCenter, signalStrength);
                _.DrawOwnSubmarineBorders(spriteBatch, transducerCenter, signalStrength);
            }
            //the directional ping sector indicator
            float directionalPingVisibility = _.useDirectionalPing && _.currentMode == Sonar.Mode.Active ? 1.0f : _.showDirectionalIndicatorTimer;
            if (directionalPingVisibility > 0.0f)
            {
                Vector2 sector1 = MathUtils.RotatePointAroundTarget(_.pingDirection * _.DisplayRadius, Vector2.Zero, MathHelper.ToRadians(SonarMod.NewSectorAngle * 0.5f));
                Vector2 sector2 = MathUtils.RotatePointAroundTarget(_.pingDirection * _.DisplayRadius, Vector2.Zero, MathHelper.ToRadians(-SonarMod.NewSectorAngle * 0.5f));
                _.DrawLine(spriteBatch, Vector2.Zero, sector1, Color.LightCyan * 0.2f * directionalPingVisibility, width: 3);
                _.DrawLine(spriteBatch, Vector2.Zero, sector2, Color.LightCyan * 0.2f * directionalPingVisibility, width: 3);
            }

            if (GameMain.DebugDraw)
            {
                GUI.DrawString(spriteBatch, rect.Location.ToVector2(), _.sonarBlips.Count.ToString(), Color.White);
            }

            _.screenOverlay?.Draw(spriteBatch, _.center, 0.0f, rect.Width / _.screenOverlay.size.X);

            if (signalStrength <= 0.5f)
            {
                _.signalWarningText.Text = TextManager.Get(signalStrength <= 0.0f ? "SonarNoSignal" : "SonarSignalWeak");
                _.signalWarningText.Color = signalStrength <= 0.0f ? _.negativeColor : _.warningColor;
                _.signalWarningText.Visible = true;
                return false;
            }
            else
            {
                _.signalWarningText.Visible = false;
            }

            foreach (AITarget aiTarget in AITarget.List)
            {
                if (aiTarget.InDetectable) { continue; }
                if (aiTarget.SonarLabel.IsNullOrEmpty() || aiTarget.SoundRange <= 0.0f) { continue; }

                if (Vector2.DistanceSquared(aiTarget.WorldPosition, transducerCenter) < aiTarget.SoundRange * aiTarget.SoundRange)
                {
                    _.DrawMarker(spriteBatch,
                        aiTarget.SonarLabel.Value,
                        aiTarget.SonarIconIdentifier,
                        aiTarget,
                        aiTarget.WorldPosition, transducerCenter,
                        _.DisplayScale, _.center, _.DisplayRadius * 0.975f);
                }
            }

            if (GameMain.GameSession == null) { return false; }

            if (Level.Loaded != null)
            {
                if (Level.Loaded.StartLocation?.Type is { ShowSonarMarker: true })
                {
                    _.DrawMarker(spriteBatch,
                        Level.Loaded.StartLocation.DisplayName.Value,
                        (Level.Loaded.StartOutpost != null ? "outpost" : "location").ToIdentifier(),
                        "startlocation",
                        Level.Loaded.StartExitPosition, transducerCenter,
                        _.DisplayScale, _.center, _.DisplayRadius);
                }

                if (Level.Loaded is { EndLocation.Type.ShowSonarMarker: true, Type: LevelData.LevelType.LocationConnection })
                {
                    _.DrawMarker(spriteBatch,
                        Level.Loaded.EndLocation.DisplayName.Value,
                        (Level.Loaded.EndOutpost != null ? "outpost" : "location").ToIdentifier(),
                        "endlocation",
                        Level.Loaded.EndExitPosition, transducerCenter,
                        _.DisplayScale, _.center, _.DisplayRadius);
                }

                for (int i = 0; i < Level.Loaded.Caves.Count; i++)
                {
                    var cave = Level.Loaded.Caves[i];
                    if (cave.MissionsToDisplayOnSonar.None()) { continue; }
                    _.DrawMarker(spriteBatch,
                        Sonar.caveLabel.Value,
                        "cave".ToIdentifier(),
                        "cave" + i,
                        cave.StartPos.ToVector2(), transducerCenter,
                        _.DisplayScale, _.center, _.DisplayRadius);
                }
            }

            int missionIndex = 0;
            foreach (Mission mission in GameMain.GameSession.Missions)
            {
                int i = 0;
                foreach ((LocalizedString label, Vector2 position) in mission.SonarLabels)
                {
                    if (!string.IsNullOrEmpty(label.Value))
                    {
                        _.DrawMarker(spriteBatch,
                            label.Value,
                            mission.SonarIconIdentifier,
                            "mission" + missionIndex + ":" + i,
                            position, transducerCenter,
                            _.DisplayScale, _.center, _.DisplayRadius * 0.95f);
                    }
                    i++;
                }
                missionIndex++;
            }

            if (_.HasMineralScanner && _.UseMineralScanner && _.CurrentMode == Sonar.Mode.Active && _.MineralClusters != null &&
                (_.item.CurrentHull == null || !_.DetectSubmarineWalls))
            {
                foreach (var c in _.MineralClusters)
                {
                    var unobtainedMinerals = c.resources.Where(i => i != null && i.GetComponent<Holdable>() is { Attached: true });
                    if (unobtainedMinerals.None()) { continue; }
                    if (!_.CheckResourceMarkerVisibility(c.center, transducerCenter)) { continue; }
                    var i = unobtainedMinerals.FirstOrDefault();
                    if (i == null) { continue; }

                    bool disrupted = false;
                    foreach ((Vector2 disruptPos, float disruptStrength) in _.disruptedDirections)
                    {
                        float dot = Vector2.Dot(Vector2.Normalize(c.center - transducerCenter), disruptPos);
                        if (dot > 1.0f - disruptStrength)
                        {
                            disrupted = true;
                            break;
                        }
                    }
                    if (disrupted) { continue; }

                    _.DrawMarker(spriteBatch,
                        i.Name, "mineral".ToIdentifier(), "mineralcluster" + i,
                        c.center, transducerCenter,
                        _.DisplayScale, _.center, _.DisplayRadius * 0.95f,
                        onlyShowTextOnMouseOver: true);
                }
            }

            foreach (Submarine sub in Submarine.Loaded)
            {
                if (!sub.ShowSonarMarker) { continue; }
                if (_.connectedSubs.Contains(sub)) { continue; }//connectedSubs.Contains(sub) basically means it's our sub
                if (Level.Loaded != null && sub.WorldPosition.Y > Level.Loaded.Size.Y) { continue; }

                if (_.item.Submarine != null || Character.Controlled != null)
                {
                    //hide enemy team
                    if (sub.TeamID == CharacterTeamType.Team1 && (_.item.Submarine?.TeamID == CharacterTeamType.Team2 || Character.Controlled?.TeamID == CharacterTeamType.Team2))
                    {
                        continue;
                    }
                    else if (sub.TeamID == CharacterTeamType.Team2 && (_.item.Submarine?.TeamID == CharacterTeamType.Team1 || Character.Controlled?.TeamID == CharacterTeamType.Team1))
                    {
                        continue;
                    }
                }

                _.DrawMarker(spriteBatch,
                    sub.Info.DisplayName.Value,
                    (sub.Info.HasTag(SubmarineTag.Shuttle) ? "shuttle" : "submarine").ToIdentifier(),
                    sub,
                    sub.WorldPosition, transducerCenter, 
                    _.DisplayScale, _.center, _.DisplayRadius * 0.95f);
            }

            if (GameMain.DebugDraw)
            {
                var steering = _.item.GetComponent<Steering>();
                steering?.DebugDrawHUD(spriteBatch, transducerCenter, _.DisplayScale, _.DisplayRadius, _.center);
            }

            //get the angle of the target velocity
            if (lockDirectionalSonar)
            {
                Vector2 targetVelocityUpdate = _.item.GetComponent<Steering>().targetVelocity;
                if (targetVelocityUpdate != null)
                {
                    // Adjusting for game engine's coordinate system and angle orientation
                    steeringangleUpdate = MathHelper.ToDegrees((float)Math.Atan2(-targetVelocityUpdate.Y, targetVelocityUpdate.X));

                    // Depending on how your game engine treats angles, you might need to adjust this
                    steeringangleUpdate = (steeringangleUpdate + 360) % 360; // Normalize angle between 0 and 360

                    //DebugConsole.NewMessage($"Steering Angle: {steeringangleUpdate}", Color.White);

                    float customAngleRadians = MathHelper.ToRadians(steeringangleUpdate);
                    Vector2 customDirection = new Vector2((float)Math.Cos(customAngleRadians), (float)Math.Sin(customAngleRadians));
                    _.pingDirection = customDirection;
                }
            }

            //for future torpedo use
            //Vector2 pingDragDirection = (Vector2)pingDragDirectionField.GetValue(__instance);
            //DebugConsole.NewMessage($"pingDragDirection: {pingDragDirection}", Color.White);
            

            return false;
        }
        /* not used
        public static bool MouseInDirectionalPingRing(Sonar __instance, Rectangle rect, bool onButton)
        {
            Sonar _ = __instance;
            if (!_.useDirectionalPing || _.directionalPingButton == null) { return false; }

            float endRadius = rect.Width / 2.0f;
            float startRadius = endRadius - _.directionalPingButton[0].size.X * rect.Width / _.screenBackground.size.X;

            Vector2 center = rect.Center.ToVector2();

            float dist = Vector2.DistanceSquared(PlayerInput.MousePosition,center);
            
            bool retVal = (dist >= startRadius*startRadius) && (dist < endRadius*endRadius);
            if (onButton)
            {
                float pingAngle = MathUtils.VectorToAngle(_.pingDirection);
                float mouseAngle = MathUtils.VectorToAngle(Vector2.Normalize(PlayerInput.MousePosition - center));
                retVal &= Math.Abs(MathUtils.GetShortestAngle(mouseAngle, pingAngle)) < MathHelper.ToRadians(Sonar.DirectionalPingSector * 0.5f);
            }

            return retVal;
            return false;
        }*/


        //use this to change the directional ping angle
        public static bool CheckBlipVisibility(SonarBlip blip, Vector2 transducerPos, Sonar __instance, ref bool __result)
        {
            Sonar _ = __instance;
             float newDotProduct = SonarMod.NewDotProduct;
        
            Vector2 pos = (blip.Position - transducerPos) * _.DisplayScale;
            pos.Y = -pos.Y;

            float posDistSqr = pos.LengthSquared();
            if (posDistSqr > _.DisplayRadius * _.DisplayRadius)
            {
                blip.FadeTimer = 0.0f;
                __result = false;
                return false;
            }

            Vector2 dir = pos / (float)Math.Sqrt(posDistSqr);
            if (_.currentPingIndex != -1 && _.activePings[_.currentPingIndex].IsDirectional)

            {
                if (Vector2.Dot(_.activePings[_.currentPingIndex].Direction, dir) < newDotProduct)
                {
                    blip.FadeTimer = 0.0f;
                    __result = false;
                    return false;
                }
            }
            __result = true;
            return false;
        }

        



        public static bool CreateGUI(Sonar __instance)
        {
            Sonar _ = __instance;
            
            
            _.isConnectedToSteering = _.item.GetComponent<Steering>() != null;
            Vector2 size = _.isConnectedToSteering ? Sonar.controlBoxSize : new Vector2(0.46f, 0.4f);

            _.controlContainer = new GUIFrame(new RectTransform(size, _.GuiFrame.RectTransform, Anchor.BottomLeft), "ItemUI");
            if (!_.isConnectedToSteering && !GUI.IsFourByThree())
            {
                _.controlContainer.RectTransform.MaxSize = new Point((int)(380 * GUI.xScale), (int)(300 * GUI.yScale));
            }
            var paddedControlContainer = new GUIFrame(new RectTransform(_.controlContainer.Rect.Size - GUIStyle.ItemFrameMargin, _.controlContainer.RectTransform, Anchor.Center)
            {
                AbsoluteOffset = GUIStyle.ItemFrameOffset
            }, style: null);
            // Based on the height difference to the steering control box so that the elements keep the same size
            float extraHeight = 0.0694f;
            var sonarModeArea = new GUIFrame(new RectTransform(new Vector2(1, 0.4f + extraHeight), paddedControlContainer.RectTransform, Anchor.TopCenter), style: null);
            _.SonarModeSwitch = new GUIButton(new RectTransform(new Vector2(0.2f, 1), sonarModeArea.RectTransform), string.Empty, style: "SwitchVertical")
            {
                UserData = UIHighlightAction.ElementId.SonarModeSwitch,
                Selected = false,
                Enabled = true,
                ClickSound = GUISoundType.UISwitch,
                OnClicked = (button, data) =>
                {
                    button.Selected = !button.Selected;
                    _.CurrentMode = button.Selected ? Sonar.Mode.Active : Sonar.Mode.Passive;
                    _.useDirectionalPing = button.Selected;
                    if (GameMain.Client != null)
                    {
                        _.unsentChanges = true;
                        _.correctionTimer = Sonar.CorrectionDelay;
                    }
                    return true;
                }
            };
            var sonarModeRightSide = new GUIFrame(new RectTransform(new Vector2(0.7f, 0.8f), sonarModeArea.RectTransform, Anchor.CenterLeft)
            {
                RelativeOffset = new Vector2(_.SonarModeSwitch.RectTransform.RelativeSize.X, 0)
            }, style: null);
            _.passiveTickBox = new GUITickBox(new RectTransform(new Vector2(1, 0.45f), sonarModeRightSide.RectTransform, Anchor.TopLeft),
                TextManager.Get("SonarPassive"), font: GUIStyle.SubHeadingFont, style: "IndicatorLightRedSmall")
            {
                UserData = UIHighlightAction.ElementId.PassiveSonarIndicator,
                ToolTip = TextManager.Get("SonarTipPassive"),
                Selected = true,
                Enabled = false
            };
            _.activeTickBox = new GUITickBox(new RectTransform(new Vector2(1, 0.45f), sonarModeRightSide.RectTransform, Anchor.BottomLeft),
                TextManager.Get("SonarActive"), font: GUIStyle.SubHeadingFont, style: "IndicatorLightRedSmall")
            {
                UserData = UIHighlightAction.ElementId.ActiveSonarIndicator,
                ToolTip = TextManager.Get("SonarTipActive"),
                Selected = false,
                Enabled = false
            };
            _.passiveTickBox.TextBlock.OverrideTextColor(GUIStyle.TextColorNormal);
            _.activeTickBox.TextBlock.OverrideTextColor(GUIStyle.TextColorNormal);

            _.textBlocksToScaleAndNormalize.Clear();
            _.textBlocksToScaleAndNormalize.Add(_.passiveTickBox.TextBlock);
            _.textBlocksToScaleAndNormalize.Add(_.activeTickBox.TextBlock);

            _.lowerAreaFrame = new GUIFrame(new RectTransform(new Vector2(1, 0.4f + extraHeight), paddedControlContainer.RectTransform, Anchor.BottomCenter), style: null);
            var zoomContainer = new GUIFrame(new RectTransform(new Vector2(1, 0.45f), _.lowerAreaFrame.RectTransform, Anchor.TopCenter), style: null);
            var zoomText = new GUITextBlock(new RectTransform(new Vector2(0.18f, 0.9f), zoomContainer.RectTransform, Anchor.CenterLeft){RelativeOffset = new Vector2(-0.03f, 0)},
                TextManager.Get("hertz"), font: GUIStyle.SubHeadingFont, textAlignment: Alignment.CenterRight);
            _.textBlocksToScaleAndNormalize.Add(zoomText);
            _.zoomSlider = new GUIScrollBar(new RectTransform(new Vector2(0.5f, 0.8f), zoomContainer.RectTransform, Anchor.CenterLeft)
            {
                RelativeOffset = new Vector2(0.45f, 0)
            }, barSize: 0.15f, isHorizontal: true, style: "DeviceSlider")
            {
                OnMoved = (scrollbar, scroll) =>
                {
                    SonarMod.NewSectorAngle = MathHelper.Lerp(120f, 15f, scroll);
                    SonarMod.hertz = MathHelper.Lerp(SonarMod.minHertzValue, SonarMod.maxHertzValue, _.zoomSlider.BarScroll);
                    if (GameMain.Client != null)
                    {
                        _.unsentChanges = true;
                        _.correctionTimer = Sonar.CorrectionDelay;
                    }
                    return true;
                }
            };
            var digitalBackground = new GUIFrame(new RectTransform(new Vector2(0.3f, 1.01f), zoomContainer.RectTransform, Anchor.Center){RelativeOffset = new Vector2(-0.2f, 0)}, style: "DigitalFrameDark");

            var hertzText = new GUITextBlock(new RectTransform(new Vector2(0.9f, 0.95f), digitalBackground.RectTransform, Anchor.Center), 
                "", font: GUIStyle.DigitalFont, textColor: GUIStyle.TextColorDark)
            {
                TextAlignment = Alignment.CenterRight,
                //ToolTip = TextManager.Get("SonarHertzTip"), // Update tooltip as necessary
                TextGetter = () => (SonarMod.hertz / 1000f).ToString("F0")// Display the current hertz value
            };


            new GUIFrame(new RectTransform(new Vector2(0.8f, 0.01f), paddedControlContainer.RectTransform, Anchor.Center), style: "HorizontalLine")
            { 
                UserData = "horizontalline" 
            };

            var directionalModeFrame = new GUIFrame(new RectTransform(new Vector2(1, 0.45f), _.lowerAreaFrame.RectTransform, Anchor.BottomCenter), style: null)
            {
                UserData = UIHighlightAction.ElementId.DirectionalSonarFrame
            };
            

                _.directionalModeSwitch = new GUIButton(new RectTransform(new Vector2(0.3f, 0.8f), directionalModeFrame.RectTransform, Anchor.CenterLeft), string.Empty, style: "SwitchHorizontal")
                {
                    OnClicked = (button, data) =>
                    {
                        if (_.item.GetComponent<Steering>() == null) { return false; }
                        lockDirectionalSonar = !lockDirectionalSonar;
                        button.Selected = lockDirectionalSonar;
                        if (GameMain.Client != null)
                        {
                            _.unsentChanges = true;
                            _.correctionTimer = Sonar.CorrectionDelay;
                            
                        }

                        return true;
                    }
                };
                var directionalModeSwitchText = new GUITextBlock(new RectTransform(new Vector2(0.7f, 1), directionalModeFrame.RectTransform, Anchor.CenterRight),
                    TextManager.Get("LockDirectionalSonar"), GUIStyle.TextColorNormal, GUIStyle.SubHeadingFont, Alignment.CenterLeft);
                _.textBlocksToScaleAndNormalize.Add(directionalModeSwitchText);
            
            if (_.HasMineralScanner)
            {
                _.AddMineralScannerSwitchToGUI();
            }
            else
            {
                _.mineralScannerSwitch = null;
            }

            _.GuiFrame.CanBeFocused = false;
            
            GUITextBlock.AutoScaleAndNormalize(_.textBlocksToScaleAndNormalize);

            _.sonarView = new GUICustomComponent(new RectTransform(Vector2.One * 0.7f, _.GuiFrame.RectTransform, Anchor.BottomRight, scaleBasis: ScaleBasis.BothHeight),
                (spriteBatch, guiCustomComponent) => { _.DrawSonar(spriteBatch, guiCustomComponent.Rect); }, null);

            _.signalWarningText = new GUITextBlock(new RectTransform(new Vector2(1.0f, 0.25f), _.sonarView.RectTransform, Anchor.Center, Pivot.BottomCenter),
                "", _.warningColor, GUIStyle.LargeFont, Alignment.Center);

            // Setup layout for nav terminal
            if (_.isConnectedToSteering || _.RightLayout)
            {
                _.controlContainer.RectTransform.AbsoluteOffset = Point.Zero;
                _.controlContainer.RectTransform.RelativeOffset = Sonar.controlBoxOffset;
                _.controlContainer.RectTransform.SetPosition(Anchor.TopRight);
                _.sonarView.RectTransform.ScaleBasis = ScaleBasis.Smallest;
                if (_.HasMineralScanner) { _.PreventMineralScannerOverlap(); }
                _.sonarView.RectTransform.SetPosition(Anchor.CenterLeft);
                _.sonarView.RectTransform.Resize(Sonar.GUISizeCalculation);
                GUITextBlock.AutoScaleAndNormalize(_.textBlocksToScaleAndNormalize);
            }
            else if (GUI.RelativeHorizontalAspectRatio > 0.75f)
            {
                _.sonarView.RectTransform.RelativeOffset = new Vector2(0.13f * GUI.RelativeHorizontalAspectRatio, 0);
                _.sonarView.RectTransform.SetPosition(Anchor.BottomRight);
            }
            var handle = _.GuiFrame.GetChild<GUIDragHandle>();
            if (handle != null)
            {
                handle.RectTransform.Parent = _.controlContainer.RectTransform;
                handle.RectTransform.Resize(Vector2.One);
                handle.RectTransform.SetAsFirstChild();
            }
            return false;
        }
    }
}