﻿#nullable enable
using Barotrauma.Extensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.Xna.Framework.Input;


using Barotrauma;
using HarmonyLib;
using System.Globalization;
using System.Reflection;

using Barotrauma.Networking;





using Barotrauma.Items.Components;



namespace MiniMapMod
{
    class MiniMapMod : IAssemblyPlugin
    {
        public Harmony harmony;
		
        private static bool highlightWalls = false;

		public void Initialize()
		{
		    harmony = new Harmony("MiniMapMod");

			
		


        var originalDrawSubmarine = typeof(MiniMap).GetMethod("DrawSubmarine", BindingFlags.NonPublic | BindingFlags.Instance);
		var prefixDrawSubmarine = typeof(MiniMapMod).GetMethod("DrawSubmarine", BindingFlags.Public | BindingFlags.Static);

		harmony.Patch(originalDrawSubmarine, new HarmonyMethod(prefixDrawSubmarine), null);
        }

		public void OnLoadCompleted() { }
		public void PreInitPatching() { }

		public void Dispose()
		{
		  harmony.UnpatchAll();
		  harmony = null;
		}
		
		
	
		public static bool DrawSubmarine(SpriteBatch spriteBatch, MiniMap __instance)
		{
			DebugConsole.NewMessage("start");
			if (__instance.item.Submarine == null || Level.Loaded == null) { return false; }
			List<Vector2> hullPoints = __instance.item.Submarine.HullVertices;
			Rectangle miniMapBounds = __instance.GuiFrame.Rect; // Get the bounds of the mini-map
			Rectangle subBounds = __instance.item.Submarine.Borders; // Get the bounds of the submarine

			// Calculate scale factors for both width and height
			float scaleX = miniMapBounds.Width / (float)subBounds.Width;
			float scaleY = miniMapBounds.Height / (float)subBounds.Height;
			float scale = Math.Min(scaleX, scaleY); // Use the smaller scale factor to maintain aspect ratio

			float manualScaleFactor = 0.8f; // Additional manual scale factor to adjust the size
			scale *= manualScaleFactor; // Apply the manual scale factor

			// Center the submarine in the mini-map
			Vector2 subCenter = new Vector2(subBounds.X + subBounds.Width / 2, subBounds.Y + subBounds.Height / 2);
			Vector2 miniMapCenter = new Vector2(miniMapBounds.X + miniMapBounds.Width / 2, miniMapBounds.Y + miniMapBounds.Height / 2);
			Vector2 centerOffset = miniMapCenter - (subCenter * scale);
			centerOffset.Y += (subBounds.Height * scale); // Move the drawing down

			float lineWidth = 10 * scale; // Scale the line width

			// Convert the list of points into rectangles (as arrays of Vector2)
			Vector2[][] subHullVertices = new Vector2[hullPoints.Count][];

			for (int i = 0; i < hullPoints.Count; i++)
			{
				Vector2 start = FarseerPhysics.ConvertUnits.ToDisplayUnits(hullPoints[i]) * scale;
				start += centerOffset; // Apply the centering offset

				Vector2 end = FarseerPhysics.ConvertUnits.ToDisplayUnits(hullPoints[(i + 1) % hullPoints.Count]) * scale;
				end += centerOffset;

				// Calculate the rectangle for this segment
				Vector2 edge = end - start;
				float length = edge.Length();
				float angle = (float)Math.Atan2(edge.Y, edge.X);
				Matrix rotate = Matrix.CreateRotationZ(angle);

				subHullVertices[i] = new[]
				{
					start + Vector2.Transform(new Vector2(length, -lineWidth), rotate),
					end + Vector2.Transform(new Vector2(-length, -lineWidth), rotate),
					end + Vector2.Transform(new Vector2(-length, lineWidth), rotate),
					start + Vector2.Transform(new Vector2(length, lineWidth), rotate),
				};
			}

			foreach (Vector2[] hullVertex in subHullVertices)
			{
				// Calculate the center point to draw a line from X to Y
				Vector2 point1 = hullVertex[1] + (hullVertex[2] - hullVertex[1]) / 2;
				Vector2 point2 = hullVertex[0] + (hullVertex[3] - hullVertex[0]) / 2;
				GUI.DrawLine(spriteBatch, point1, point2, (highlightWalls ? GUIStyle.Orange * 0.6f : Color.DarkCyan * 0.3f), width: 10);
				if (GameMain.DebugDraw)
				{
					GUI.DrawRectangle(spriteBatch, hullVertex, Color.Red);
				}
			}

			return false;
		}



            

        
	}

    


    
}