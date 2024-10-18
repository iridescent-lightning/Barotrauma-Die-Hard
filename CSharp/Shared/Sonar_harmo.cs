﻿using Barotrauma.Networking;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Barotrauma.Items.Components;


using System.Reflection;


using Barotrauma;
using HarmonyLib;
using System.Globalization;

using Networking;

namespace SonarMod
{
    partial class SonarMod : IAssemblyPlugin
    {
        public Harmony harmony;
		
		private static float PingFrequency = 0.5f;//how fast a ping spreads
		private static float timeSinceLastPing = 5.1f;
		private static float timeSinceLastDirectionalPing = 5.1f;

        //get user input for the directional ping
        private static float minAfflictionStrength = 0.1f;
        private static float maxAfflictionStrength = 1.0f;

		public void Initialize()
		{
		    harmony = new Harmony("SonarMod");

            

		    harmony.Patch(
					original: typeof(Sonar).GetMethod("Update"),
					prefix: new HarmonyMethod(typeof(SonarMod).GetMethod("Update"))
					);
#if CLIENT
            //creategui has to be patched first. or game crash without error message
        var originalCreateGUI = typeof(Sonar).GetMethod("CreateGUI", BindingFlags.NonPublic | BindingFlags.Instance);
        var prefixCreateGUI = typeof(SonarMod).GetMethod("CreateGUI", BindingFlags.Public | BindingFlags.Static);
        harmony.Patch(originalCreateGUI, new HarmonyMethod(prefixCreateGUI), null);

        

        var originalDrawSonar = typeof(Sonar).GetMethod("DrawSonar", BindingFlags.NonPublic | BindingFlags.Instance);
		var prefixDrawSonar = typeof(SonarMod).GetMethod("DrawSonar", BindingFlags.Public | BindingFlags.Static);

		harmony.Patch(originalDrawSonar, new HarmonyMethod(prefixDrawSonar), null);


		var originalPing = typeof(Sonar).GetMethod("Ping", BindingFlags.NonPublic | BindingFlags.Instance);
		var prefixPing = typeof(SonarMod).GetMethod("PingPrefix", BindingFlags.Public | BindingFlags.Static);

		harmony.Patch(originalPing, new HarmonyMethod(prefixPing), null);
		
		


        /*var originalMouseInPingRing = typeof(Sonar).GetMethod("MouseInDirectionalPingRing", BindingFlags.NonPublic | BindingFlags.Instance);
        var prefixMouseInPingRing = typeof(SonarMod).GetMethod("MouseInDirectionalPingRing", BindingFlags.Public | BindingFlags.Static);
        harmony.Patch(originalMouseInPingRing, new HarmonyMethod(prefixMouseInPingRing), null);
    */
		var originalCheckDirectVisibili = typeof(Sonar).GetMethod("CheckBlipVisibility", BindingFlags.NonPublic | BindingFlags.Instance);
        var prefixCheckDirectVisibili = typeof(SonarMod).GetMethod("CheckBlipVisibility", BindingFlags.Public | BindingFlags.Static);
        harmony.Patch(originalCheckDirectVisibili, new HarmonyMethod(prefixCheckDirectVisibili), null);
#endif
		
		}

		public void OnLoadCompleted() { }
		public void PreInitPatching() 
        { 
#if SERVER
            NetUtil.Register(NetEvent.APPLY_SONAR_PING_DAMAGE, OnReceiveSonarPingApplyDamageMessage);
#endif
        }

		public void Dispose()
		{
		  harmony.UnpatchAll();
		  harmony = null;
		}
		


         public static bool Update(float deltaTime, Camera cam, Sonar __instance)
        {

			Sonar _ = __instance;
            _.UpdateOnActiveEffects(deltaTime);

            if (_.UseTransducers)
            {
                foreach (Sonar.ConnectedTransducer transducer in _.connectedTransducers)//directly use Sonar. to get the field that is not in the context if __instance doesn't work.
                {
                    transducer.DisconnectTimer -= deltaTime;
                }
                _.connectedTransducers.RemoveAll(t => t.DisconnectTimer <= 0.0f);
            }

            for (var pingIndex = 0; pingIndex < _.activePingsCount; ++pingIndex)
            {
                _.activePings[pingIndex].State += deltaTime * PingFrequency;
            }

            if (_.currentMode == Sonar.Mode.Active && !_.useDirectionalPing)
            {
				timeSinceLastPing += deltaTime;
				
                if ((_.Voltage >= _.MinVoltage) &&
                    (!_.UseTransducers || _.connectedTransducers.Count > 0))
                {
                    if (_.currentPingIndex != -1)
                    {
                        var activePing = _.activePings[_.currentPingIndex];
                        if (activePing.State > 1.0f)
                        {
                            _.aiPingCheckPending = true;
                            _.currentPingIndex = -1;
                        }
                    }
                    if (_.currentPingIndex == -1 && _.activePingsCount < _.activePings.Length && timeSinceLastPing >= 5f)
                    {
						timeSinceLastPing = 0.0f;//reset
                        _.currentPingIndex = _.activePingsCount++;
                        if (_.activePings[_.currentPingIndex] == null)
                        {
                            _.activePings[_.currentPingIndex] = new Sonar.ActivePing();
                        }
                        _.activePings[_.currentPingIndex].IsDirectional = _.useDirectionalPing;
                        _.activePings[_.currentPingIndex].Direction = _.pingDirection;
                        _.activePings[_.currentPingIndex].State = 0.0f;
                        _.activePings[_.currentPingIndex].PrevPingRadius = 0.0f;
                        if (_.item.AiTarget != null)
                        {
                            _.item.AiTarget.SectorDegrees = _.useDirectionalPing ? Sonar.DirectionalPingSector : 360.0f;
                            _.item.AiTarget.SectorDir = new Vector2(_.pingDirection.X, -_.pingDirection.Y);
                        }
                        _.item.Use(deltaTime);
                    }
                }
                else
                {
                    _.aiPingCheckPending = false;
                }
            }
            //todo the ai target check should be revised to match the new directional ping
            if (_.currentMode == Sonar.Mode.Active && _.useDirectionalPing)
            {
				timeSinceLastDirectionalPing += deltaTime;
				
                if ((_.Voltage >= _.MinVoltage) &&
                    (!_.UseTransducers || _.connectedTransducers.Count > 0))
                {
                    if (_.currentPingIndex != -1)
                    {
                        var activePing = _.activePings[_.currentPingIndex];
                        if (activePing.State > 1.0f)
                        {
                            _.aiPingCheckPending = true;
                            _.currentPingIndex = -1;
                        }
                    }
                    if (_.currentPingIndex == -1 && _.activePingsCount < _.activePings.Length && timeSinceLastDirectionalPing >= 3.0f)
                    {
						timeSinceLastDirectionalPing = 0.0f;//reset
                        _.currentPingIndex = _.activePingsCount++;
                        if (_.activePings[_.currentPingIndex] == null)
                        {
                            _.activePings[_.currentPingIndex] = new Sonar.ActivePing();
                        }
                        _.activePings[_.currentPingIndex].IsDirectional = _.useDirectionalPing;
                        _.activePings[_.currentPingIndex].Direction = _.pingDirection;
                        _.activePings[_.currentPingIndex].State = 0.0f;
                        _.activePings[_.currentPingIndex].PrevPingRadius = 0.0f;
                        if (_.item.AiTarget != null)
                        {
                            _.item.AiTarget.SectorDegrees = _.useDirectionalPing ? Sonar.DirectionalPingSector : 360.0f;
                            _.item.AiTarget.SectorDir = new Vector2(_.pingDirection.X, -_.pingDirection.Y);
                        }
                        _.item.Use(deltaTime);
                    }
                }
                else
                {
                    _.aiPingCheckPending = false;
                }
            }

            for (var pingIndex = 0; pingIndex < _.activePingsCount;)
            {
                if (_.item.AiTarget != null)
                {
                    float range = MathUtils.InverseLerp(_.item.AiTarget.MinSoundRange, _.item.AiTarget.MaxSoundRange, _.Range * _.activePings[pingIndex].State / _.zoom);
                    _.item.AiTarget.SoundRange = Math.Max(_.item.AiTarget.SoundRange, MathHelper.Lerp(_.item.AiTarget.MinSoundRange, _.item.AiTarget.MaxSoundRange, range));
                }
                if (_.activePings[pingIndex].State > 1.0f)
                {
                    var lastIndex = --_.activePingsCount;
                    var oldActivePing = _.activePings[pingIndex];
                    _.activePings[pingIndex] = _.activePings[lastIndex];
                    _.activePings[lastIndex] = oldActivePing;
                    if (_.currentPingIndex == lastIndex)
                    {
                        _.currentPingIndex = pingIndex;
                    }
                }
                else
                {
                    ++pingIndex;
                }
            }
            

#if CLIENT            
            foreach (Character c in Character.CharacterList)
            {
                Vector2 pingSource = _.item.WorldPosition;
                if (__instance.CurrentMode == Sonar.Mode.Active && __instance.useDirectionalPing)
                {
                    if (c.AnimController.CurrentHull != null || !c.Enabled) { continue; }
                    if (!c.IsUnconscious && c.Params.HideInSonar) { continue; }
                    if (_.DetectSubmarineWalls && c.AnimController.CurrentHull == null && _.item.CurrentHull != null) { continue; }

                    float newDotProduct = SonarMod.NewDotProduct;
                    Vector2 pingDirection = __instance.pingDirection; // Accessing the ping direction
                    pingDirection.Y = -pingDirection.Y;
                    foreach (Character target in Character.CharacterList)
                    {
                        if (!target.InWater || target.IsDead || target.CharacterHealth == null) { continue; }

                        float pointDist = ((target.WorldPosition - pingSource) * 1f).LengthSquared();
                        
                            Vector2 dirToTarget = Vector2.Normalize(target.WorldPosition - pingSource);
                            // Check if the target is within the directional ping sector
                            if (Vector2.Dot(dirToTarget, pingDirection) >= newDotProduct)
                            {
                                float currentHertz = SonarMod.hertz; // Make sure this reflects the slider value
                                float afflictionStrength = MathHelper.Lerp(SonarMod.minAfflictionStrength, SonarMod.maxAfflictionStrength, (currentHertz - SonarMod.minHertzValue) / (maxHertzValue - SonarMod.minHertzValue));

                                if (target.IsHuman)
                                {target.CharacterHealth.ApplyAffliction(target.AnimController.MainLimb, AfflictionPrefab.Prefabs["sonardamage"].Instantiate(afflictionStrength * 0.1f));}
                                //DebugConsole.NewMessage($"SonarMod: {target.Name} DamageReceived: " + $"{afflictionStrength}", Color.White);
                                if (GameMain.Client != null)
                                {
                                    _.unsentChanges = true;
                                    _.correctionTimer = Sonar.CorrectionDelay;
                                    SendApplyDamageMessage(target.ID, "sonardamage", afflictionStrength * 0.1f);
                                }
                                
                            }
                        
                    }
                }
            }
#endif
            return false;
        }

			
        
#if CLIENT
        private static void SendApplyDamageMessage(ushort characterId, string afflictionType, float afflictionStrength) 
        {
        IWriteMessage msg = NetUtil.CreateNetMsg(NetEvent.APPLY_SONAR_PING_DAMAGE);
        msg.WriteUInt16(characterId);
        msg.WriteString(afflictionType);
        msg.WriteSingle(afflictionStrength);
        NetUtil.SendServer(msg, DeliveryMethod.Reliable);
        }
#endif
        private void OnReceiveSonarPingApplyDamageMessage(object[] args) 
        {
        IReadMessage msg = (IReadMessage)args[0];
        ushort characterId = msg.ReadUInt16();
        string afflictionType = msg.ReadString();
        float afflictionStrength = msg.ReadSingle();

        Character target = Entity.FindEntityByID(characterId) as Character;
        if (target != null && !target.IsDead) {
            AfflictionPrefab afflictionPrefab = AfflictionPrefab.Prefabs[afflictionType];
            if (afflictionPrefab != null) {
                target.CharacterHealth.ApplyAffliction(target.AnimController.MainLimb, afflictionPrefab.Instantiate(afflictionStrength));
                }
            }
        }
    }
}    
    
    
