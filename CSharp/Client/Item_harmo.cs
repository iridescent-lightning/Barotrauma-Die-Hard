
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
            Item _ = __instance;
            
            if (GameSessionDieHard.texture != null && !GameSessionDieHard.texture.IsDisposed)
            {
                
                
                if (_.Prefab.Identifier == "mediumsteelcabinet" && _.HasTag("draw_container_open"))
                {
                    ItemContainer itemContainer = _.GetComponent<ItemContainer>();

                    
                      // Create an offset for adjustment
                        Vector2 offset = new Vector2(-73.5f, -180f); // Adjust these values as needed

                        // Calculate the draw position
                        Vector2 drawPosition = new Vector2(_.DrawPosition.X, -_.DrawPosition.Y) + offset; // Apply offset

                        // Draw the sprite at the adjusted position
                        GameSessionDieHard.customSprite.Draw(
                            spriteBatch, 
                            drawPosition, 
                            color: _.GetSpriteColor(), 
                            rotate: 0, 
                            scale: 0.5f, 
                            origin: GameSessionDieHard.customSprite.Origin,
                            depth: _.GetDrawDepth() - 0.1f
                        ); 
                    
                }

                    
                
                
            }

                
        }

        

    }
}
