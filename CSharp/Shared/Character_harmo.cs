﻿using Barotrauma.Networking;
using FarseerPhysics;
using Microsoft.Xna.Framework;
using System;
using Barotrauma.IO;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Barotrauma.Items.Components;
using FarseerPhysics.Dynamics;
using Barotrauma.Extensions;
using System.Collections.Immutable;
using Barotrauma.Abilities;
using System.Diagnostics;
#if SERVER
using System.Text;
#endif


using Barotrauma;
using HarmonyLib;
using System.Globalization;
using System.Reflection;
using HullModNamespace;


namespace CharacterModNamespace
{
    class CharacterMod : IAssemblyPlugin
    {
        public Harmony harmony;
		
        public static bool hasZoomed = false;

		public void Initialize()
		{
		    harmony = new Harmony("CharacterMod");

			
			
            var originalUpdateOxygen = typeof(Character).GetMethod("UpdateOxygen", BindingFlags.NonPublic | BindingFlags.Instance);
            var postfixUpdateOxygen = new HarmonyMethod(typeof(CharacterMod).GetMethod(nameof(UpdateOxygenPostfix), BindingFlags.Public | BindingFlags.Static));
            harmony.Patch(originalUpdateOxygen, postfixUpdateOxygen, null);
        }

		public void OnLoadCompleted() { }
		public void PreInitPatching() { }

		public void Dispose()
		{
		  harmony.UnpatchAll();
		  harmony = null;
		}
		
		private static float escapedTime;
        private static float updateTimer = 1.0f;
		
		public static void UpdateOxygenPostfix(Character __instance, float deltaTime)
		{
			
				
			if (__instance == null) { return; }

			if (__instance.CurrentHull == null) { return; }
			
			if (!__instance.IsDead && __instance.UseHullOxygen)
			{
				HullMod.AddGas(__instance.CurrentHull, "CO2", 1f, deltaTime);
			
				if (HullMod.GetGas(__instance.CurrentHull, "CO2")  > 600f)
				{
					__instance.CharacterHealth.ApplyAffliction(__instance.AnimController.MainLimb, AfflictionPrefab.Prefabs["co_poisoning"].Instantiate(1f * deltaTime));
				}
				if (HullMod.GetGas(__instance.CurrentHull, "CO") > 400f)
				{
					__instance.CharacterHealth.ApplyAffliction(__instance.AnimController.MainLimb, AfflictionPrefab.Prefabs["co_poisoning"].Instantiate(5f * deltaTime));
				}
				if (HullMod.GetGas(__instance.CurrentHull, "CL") > 200f)
				{
					__instance.CharacterHealth.ApplyAffliction(__instance.AnimController.MainLimb, AfflictionPrefab.Prefabs["chlorine_poisoning"].Instantiate(0.1f * deltaTime));
				}
			}


			if (HullMod.GetGas(__instance.CurrentHull, "Temperature") < 278.15f)
			{
				__instance.CharacterHealth.ApplyAffliction(__instance.AnimController.MainLimb, AfflictionPrefab.Prefabs["coldwater"].Instantiate(0.15f * deltaTime));
			}
			else if (HullMod.GetGas(__instance.CurrentHull, "Temperature") > 323.15f)
			{
				__instance.CharacterHealth.ApplyAffliction(__instance.AnimController.MainLimb, AfflictionPrefab.Prefabs["burn"].Instantiate((HullMod.GetGas(__instance.CurrentHull, "Temperature") - 318.15f) * deltaTime * 2f));
			}
			else if (HullMod.GetGas(__instance.CurrentHull, "Temperature") > 293.15f)
			{
				__instance.CharacterHealth.ApplyAffliction(__instance.AnimController.MainLimb, AfflictionPrefab.Prefabs["coldwater"].Instantiate(-0.5f * deltaTime));
			}
			escapedTime = 0;
			
		}


		
        

		
        
	}
    
}