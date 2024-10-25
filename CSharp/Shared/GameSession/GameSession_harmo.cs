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
        

        public static Dictionary<string, Sprite> customSprites = new Dictionary<string, Sprite>();


        private static ContentPackage modPackage = ContentPackageManager.AllPackages.FirstOrDefault(p => p.Name == "Barotrauma Die Hard");
#endif

        public static void StartRound(LevelData levelData, bool mirrorLevel, SubmarineInfo startOutpost, SubmarineInfo endOutpost)
        {

#if CLIENT
                
            AddTextureToSpriteList("mediumsteelcabinet_open", "%ModDir%/Items/Containers/containers_opened.png", new Rectangle(0, 0, 149, 360), originPercentage: new Vector2(0.5f, 0.495f));
            AddTextureToSpriteList("mediumwindowedsteelcabinet_open", "%ModDir%/Items/Containers/containers_opened.png", new Rectangle(154, 0, 149, 360), originPercentage: new Vector2(0.52f, 0.495f));
            AddTextureToSpriteList("steelcabinet_open", "%ModDir%/Items/Containers/containers_opened.png", new Rectangle(313, 0, 371, 359), originPercentage: new Vector2(0.48f, 0.485f));
            AddTextureToSpriteList("medcabinet_open", "%ModDir%/Items/Containers/containers_opened.png", new Rectangle(814, 252, 210, 321), originPercentage: new Vector2(0.5f, 0.48f));
            //AddTextureToSpriteList("seccabinet_open_0", "%ModDir%/Items/Containers/containers_opened.png", new Rectangle(905, 14, 105, 160));
            AddTextureToSpriteList("seccabinet_open_1", "%ModDir%/Items/Containers/containers_opened.png", new Rectangle(773, 8, 105, 209), originPercentage: new Vector2(0.5f, 0.42f));
            AddTextureToSpriteList("toxiccabinet_open", "%ModDir%/Items/Containers/containers_opened.png", new Rectangle(684, 426, 111, 155), originPercentage: new Vector2(0.5f, 0.48f));
            AddTextureToSpriteList("supplycabinet_open", "%ModDir%/Items/Containers/containers_opened.png", new Rectangle(827, 621, 189, 132), originPercentage: new Vector2(0.7f, 0.498f));

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



#if CLIENT
        public static void AddTextureToSpriteList(string spriteKey, string filepath, Rectangle sourceRect, Vector2? offset = null, Vector2? originPercentage = null, float rotation = 0)
        {
            string modTexturePath = filepath;
            ContentPath contentPath = ContentPath.FromRaw(modPackage, modTexturePath);

            if (offset == null)
                offset = Vector2.Zero;

            Texture2D texture = Sprite.LoadTexture(contentPath.FullPath);
            if (texture != null && !texture.IsDisposed)
            {
                Sprite newSprite = new Sprite(texture, sourceRect, offset, rotation, null);

                // Calculate origin based on the percentage, defaulting to the center if no percentage is specified
                Vector2 origin = originPercentage.HasValue 
                    ? new Vector2(sourceRect.Width * originPercentage.Value.X, sourceRect.Height * originPercentage.Value.Y) 
                    : new Vector2(sourceRect.Width / 2, sourceRect.Height / 2);
                
                newSprite.Origin = origin;

                // Add the sprite to the dictionary with the specified key
                customSprites[spriteKey] = newSprite;
            }
        }
 

#endif
    }
}
