﻿using Barotrauma.Networking;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Barotrauma.Items.Components;
﻿using Barotrauma.Extensions;
using Barotrauma.Networking;
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



namespace SonarBlipMod
{
    class SonarBlipMod : IAssemblyPlugin
    {
        public Harmony harmony;
		

		public void Initialize()
		{
		  
		
		var originalCreateBlipsForSubmarineWalls = typeof(Sonar).GetMethod("CreateBlipsForSubmarineWalls", BindingFlags.NonPublic | BindingFlags.Instance);
		var prefixCreateBlipsForSubmarineWalls = typeof(SonarBlipMod).GetMethod("CreateBlipsForSubmarineWallsPrefix", BindingFlags.Public | BindingFlags.Static);//flags must match

		harmony.Patch(originalCreateBlipsForSubmarineWalls, new HarmonyMethod(prefixCreateBlipsForSubmarineWalls), null);
					
		}

		public void OnLoadCompleted() { }
		public void PreInitPatching() { }

		public void Dispose()
		{
		  harmony.UnpatchAll();
		  harmony = null;
		}
		
	
	public static bool CreateBlipsForSubmarineWallsPrefix(Submarine sub, Vector2 pingSource, Vector2 transducerPos, float pingRadius, float prevPingRadius, float range, bool passive, Sonar __instance)
        {
			//Sonar _ = __instance;
			
            foreach (Structure structure in Structure.WallList)
            {
                if (structure.Submarine != sub) { continue; }
                CreateBlips(structure.IsHorizontal, structure.WorldPosition, structure.WorldRect);
            }
            foreach (var door in Door.DoorList)
            {
                if (door.Item.Submarine != sub || door.IsOpen) { continue; }
                CreateBlips(door.IsHorizontal, door.Item.WorldPosition, door.Item.WorldRect, Sonar.BlipType.Door);
            }

            void CreateBlips(bool isHorizontal, Vector2 worldPos, Rectangle worldRect, Sonar.BlipType blipType = Sonar.BlipType.Default)
            {
                Vector2 point1, point2;
                if (isHorizontal)
                {
                    point1 = new Vector2(worldRect.X, worldPos.Y);
                    point2 = new Vector2(worldRect.Right, worldPos.Y);
                }
                else
                {
                    point1 = new Vector2(worldPos.X, worldRect.Y);
                    point2 = new Vector2(worldPos.X, worldRect.Y - worldRect.Height);
                }
                Sonar.CreateBlipsForLine(
                    point1,
                    point2,
                    pingSource, transducerPos,
                    pingRadius, prevPingRadius, 50.0f, 5.0f, range, 2.0f, passive, blipType);
            }
			return false;
        }
		
	}
}