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
#endif
using Barotrauma.Extensions;


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
        }

        public void OnLoadCompleted() { }
        public void PreInitPatching() { }

        public void Dispose()
        {
            harmony.UnpatchAll();
            harmony = null;
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
