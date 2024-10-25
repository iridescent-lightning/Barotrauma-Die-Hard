#nullable enable

using Barotrauma.IO;
using Barotrauma.Items.Components;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Xml.Linq;
using Barotrauma.Networking;
using Barotrauma.Extensions;


using Barotrauma;
#if CLIENT
using Barotrauma.Lights;
using Microsoft.Xna.Framework.Graphics;
#endif
using Barotrauma.Extensions;
using System.Reflection;


using HarmonyLib;
using Barotrauma.Items.Components;


namespace BarotraumaDieHard
{
    class GameSessionDieHard : IAssemblyPlugin
    {
        public Harmony harmony;
        
        public void Initialize()
        {
            harmony = new Harmony("GameSessionDieHard");

            harmony.Patch(
                original: typeof(GameSession).GetMethod("EndRound"),
                postfix: new HarmonyMethod(typeof(GameSessionDieHard).GetMethod(nameof(EndRound)))
            );


            var originalStartRound = typeof(GameSession).GetMethod("StartRound", BindingFlags.Public | BindingFlags.Instance, null, 
                new Type[] { typeof(LevelData), typeof(bool), typeof(SubmarineInfo), typeof(SubmarineInfo) }, null);

            var postfixStartRound = new HarmonyMethod(typeof(GameSessionDieHard).GetMethod(nameof(StartRound), BindingFlags.Public | BindingFlags.Static));
            
            harmony.Patch(originalStartRound, null, postfixStartRound);
        }

        public void OnLoadCompleted() { }
        public void PreInitPatching() { }

        public void Dispose()
        {
            harmony.UnpatchAll();
            harmony = null;
        }

#if CLIENT
        public static Texture2D texture;

        public static Sprite customSprite;
#endif

        public static void StartRound(LevelData levelData, bool mirrorLevel, SubmarineInfo startOutpost, SubmarineInfo endOutpost)
        {

#if CLIENT
                string modTexturePath = "%ModDir%/Items/Containers/containers_opened.png";
                ContentPackage modPackage = ContentPackageManager.AllPackages.FirstOrDefault(p => p.Name == "Barotrauma Die Hard");
                ContentPath contentPath = ContentPath.FromRaw(modPackage, modTexturePath);

                texture = Sprite.LoadTexture(contentPath.FullPath);
                if (texture != null && !texture.IsDisposed)
                {
                    // Define the source rectangle (which part of the texture you want to use)
                    // You can use the entire texture or a part of it
                    Rectangle sourceRect = new Rectangle(0, 0, 149, 360);

                    // Define the origin point (optional, for rotation or scaling)
                    Vector2 origin = new Vector2(sourceRect.Width / 4, sourceRect.Height / 4); // Set origin to center

                    // Create a sprite using the loaded texture
                    customSprite = new Sprite(texture, sourceRect, origin);

                    Sprite.AddToList(customSprite);
                }
#endif

        }

        
        public static void EndRound(string endMessage, CampaignMode.TransitionType transitionType, TraitorManager.TraitorResults? traitorResults)
        {

           TurretDieHard.ResetOriginalReloadValue();
           TurretDieHard.ClearReloadDictionary();

            SonarMod.ResetOriginalSonarRange();
            SonarMod.ClearSonarRangeDictionary();


            ReactorDieHard.ClearRactorySecondContainerDictionary();
    

        } 
    }
}
