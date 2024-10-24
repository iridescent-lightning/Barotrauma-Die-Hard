
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
            //Sprite customSprite = LoadCustomSprite("%ModDir%/Items/InventoryIconAtlas.png"); // Adjust the path and filename

                // Draw the custom sprite
                //Vector2 drawPosition = new Vector2(__instance.DrawPosition.X, -__instance.DrawPosition.Y); // Adjust position if needed
                //customSprite.Draw(spriteBatch, drawPosition, Color.White); 
                //DebugConsole.NewMessage(ContentXElement.ContentPath("%ModDir/Items/InventoryIconAtlas.png"));
        }

        /*private static Sprite LoadCustomSprite(string spritePath)
        {
            // Load your sprite using the ContentManager
            var texture = GameMain.Instance.Content.Load<Sprite>(spritePath);
            return new Sprite(texture);
        }*/
        public static Sprite? LoadCustomSprite(string spriteFileName)
    {
        // Assuming "MyMod" is the name of your mod
        string modName = "Barotrauma Die Hard";
        
        // Get the content packages
        var packages = ContentPackageManager.AllPackages;

        // Find your mod package
        ContentPackage? myModPackage = packages.FirstOrDefault(p => p.Name.Equals(modName, StringComparison.OrdinalIgnoreCase));

        if (myModPackage == null)
        {
            Console.WriteLine($"Could not find mod package: {modName}");
            return null;
        }

        // Create a ContentPath for the sprite
        ContentPath spritePath = ContentPath.FromRaw(myModPackage, $"{spriteFileName}"); // Adjust the relative path as necessary

        // Use the ContentPath to load the sprite
        if (!spritePath.IsPathNullOrEmpty())
        {
            string fullPath = spritePath.FullPath;

            var texture = GameMain.Instance.Content.Load<Sprite>(fullPath);
            return new Sprite(texture);

        }

         // Throw an exception if sprite could not be loaded
    throw new Exception($"Sprite could not be loaded from path: {spritePath.Value}");
        
    }

    }
}
