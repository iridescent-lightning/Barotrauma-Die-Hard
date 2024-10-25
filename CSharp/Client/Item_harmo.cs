
using Barotrauma.Extensions;
using Barotrauma.Items.Components;
using Barotrauma.MapCreatures.Behavior;
using Barotrauma.Networking;
using FarseerPhysics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text;

using Barotrauma;
using HarmonyLib;
using System.Globalization;
using System.Reflection;// for bindingflags

using System.IO;

namespace BarotraumaDieHard
{
    public class ItemDieHard : IAssemblyPlugin
    {
        public Harmony harmony;
		public void Initialize()
		{
		    harmony = new Harmony("ItemDieHard");

            // Fix for ambiguous match.
            // Use the correct method signature for the Draw method you want to patch
            var originalDraw = typeof(Item).GetMethod("Draw", BindingFlags.Public | BindingFlags.Instance, null, 
                new Type[] { typeof(SpriteBatch), typeof(bool), typeof(bool), typeof(Color?) }, null);

            var postfixDraw = new HarmonyMethod(typeof(ItemDieHard).GetMethod(nameof(Draw), BindingFlags.Public | BindingFlags.Static));
            
            // Patch the original Draw method with your postfix method
            harmony.Patch(originalDraw, null, postfixDraw);
        }

		public void OnLoadCompleted() { }
		public void PreInitPatching() { }

		public void Dispose()
		{
		  harmony.UnpatchAll();
		  harmony = null;
		}

        public static void Draw(SpriteBatch spriteBatch, bool editing, bool back, Color? overrideColor, Item __instance)
        {           
            // sprite name, drawed item, at which tag to daw
            DrawCustomSprite("mediumsteelcabinet_open", "mediumsteelcabinet", "draw_container_open", __instance, spriteBatch);
            DrawCustomSprite("mediumwindowedsteelcabinet_open", "mediumwindowedsteelcabinet", "draw_container_open", __instance, spriteBatch);        
            DrawCustomSprite("steelcabinet_open", "steelcabinet", "draw_container_open", __instance, spriteBatch);
               
            DrawCustomSprite("medcabinet_open", "medcabinet", "draw_container_open", __instance, spriteBatch);
            //DrawCustomSprite("seccabinet_open_0", "securesteelcabinet", "draw_container_open", __instance, spriteBatch);
            DrawCustomSprite("seccabinet_open_1", "securesteelcabinet", "draw_container_open", __instance, spriteBatch);
            DrawCustomSprite("toxiccabinet_open", "toxcabinet", "draw_container_open", __instance, spriteBatch);
            DrawCustomSprite("supplycabinet_open", "suppliescabinet", "draw_container_open", __instance, spriteBatch);












        }   


        private static void DrawCustomSprite(string spriteName, string targetItemIdentifier, string targetItemTag, Item targetItem, SpriteBatch spriteBatch)
        {
            // Try to get the sprite by its name from the dictionary
            if (GameSessionDieHard.customSprites.TryGetValue(spriteName, out Sprite customSprite))
            {
                if (customSprite == null)
                {
                    DebugConsole.NewMessage("disposed!");
                    return;
                }
                // Check if the item matches the specified identifier and tag
                if (targetItem.Prefab.Identifier == targetItemIdentifier && targetItem.HasTag(targetItemTag))
                {
                    // Calculate the draw position. Need to invert Y-axis.
                    Vector2 drawPosition = new Vector2(targetItem.DrawPosition.X, -targetItem.DrawPosition.Y);

                    // Draw the sprite at the adjusted position
                    customSprite.Draw(
                        spriteBatch, 
                        pos: drawPosition, 
                        color: targetItem.GetSpriteColor(), 
                        rotate: targetItem.Rotation, 
                        scale: targetItem.Scale, 
                        origin: customSprite.Origin,
                        depth: targetItem.GetDrawDepth() - 0.01f
                    );
                }
            }
        }


                
        }


        
}
