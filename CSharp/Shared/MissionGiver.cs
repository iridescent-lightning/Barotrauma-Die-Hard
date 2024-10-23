using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Barotrauma.Items.Components;
using Barotrauma.Networking;
using Barotrauma.Extensions;
using Barotrauma;
using Networking;

namespace BarotraumaDieHard
{
    class ForceMission : ItemComponent
    {
		
		
        public override void OnItemLoaded()
        {
            
            base.OnItemLoaded();

            if (!(GameMain.GameSession.GameMode is CampaignMode))
            {
                DebugConsole.ThrowError("Cannot force a mission out side of campaign mode.");
                return;
            }

            // Log all available mission prefabs for debugging (optional)
            /*foreach (var missionPrefab in MissionPrefab.Prefabs)
            {
                DebugConsole.NewMessage($"Available mission: {missionPrefab.Identifier}", Color.Green);
            }*/

            // Assuming you have an identifier for the specific mission you want to load
            
            Identifier specificMissionId = "BeaconMissionDieHard".ToIdentifier(); 

            // Find the specific mission prefab by identifier
            var missionPrefabToLoad = MissionPrefab.Prefabs[specificMissionId];

            if (missionPrefabToLoad != null)
            {
                // Start a coroutine to safely start the mission after the current update cycle. Otherwise the initiation will crash the game when trying to spawn human charcter.
                CoroutineManager.StartCoroutine(StartMissionAfterDelay(missionPrefabToLoad));
            }
            else
            {
                DebugConsole.ThrowError($"Mission with ID {specificMissionId} not found in available mission prefabs.");
            }
        }

        public ForceMission(Item item, ContentXElement element)
            : base(item, element)
        {

            IsActive = true;
        }


        private static IEnumerable<CoroutineStatus> StartMissionAfterDelay(MissionPrefab missionPrefabToLoad)
        {
            // Wait for one frame to avoid modifying collections during updates
            yield return CoroutineStatus.Running;

            // Retrieve current map locations
            Location startLocation = GameMain.GameSession.StartLocation;
            Location endLocation = GameMain.GameSession.EndLocation ?? startLocation; // Fallback to startLocation if no destination is selected

            // Create the mission instance using the locations and the main submarine
            Mission specificMission = missionPrefabToLoad.Instantiate(new[] { startLocation, endLocation }, Submarine.MainSub);

            // Log the mission type for debugging
            DebugConsole.NewMessage(specificMission.GetType().ToString());

            // Check for specific mission types and trigger appropriate entity generation
            if (specificMission is PirateMission)
            {
                GenerateEnemySub();
            }
            if (specificMission is BeaconMission)
            {
                GenerateBeacon();
            }
            if (specificMission is EscortMission escortMission)
            {
                // escortMission.StartMissionSpecific(Level.Loaded);
            }
            if (missionPrefabToLoad.RequireWreck)
            {
                GenerateWreck();
            }
            if (missionPrefabToLoad.RequireRuin)
            {
                GenerateRuin();
            }

            // Add the specific mission to the missions list so it shows in the tab screen
            GameMain.GameSession.missions.Add(specificMission);

            // Add extra mission?
            if (GameMain.GameSession?.GameMode is CampaignMode campaignMode)
            {
                
                campaignMode.AddExtraMissions(Level.Loaded.LevelData);
            }

            // Start the specific mission
            int prevEntityCount = Entity.GetEntities().Count;
            DebugConsole.NewMessage(prevEntityCount.ToString());
            

            // Ensure that clients do not instantiate entities themselves
            if (GameMain.NetworkMember != null && GameMain.NetworkMember.IsClient && Entity.GetEntities().Count != prevEntityCount)
            {
                DebugConsole.ThrowError(
                    $"Entity count has changed after starting a mission ({specificMission}) as a client. " +
                    "The clients should not instantiate entities themselves when starting the mission, " +
                    "but instead the server should inform the client of the spawned entities using Mission.ServerWriteInitial.");
            }

            // Show the start message for the specific mission on the client side
        #if CLIENT
            if (specificMission.Prefab.ShowStartMessage)
            {
                var messageBox = new GUIMessageBox(
                    RichString.Rich(specificMission.Prefab.IsSideObjective 
                        ? TextManager.AddPunctuation(':', TextManager.Get("sideobjective"), specificMission.Name) 
                        : specificMission.Name), 
                    RichString.Rich(specificMission.Description), 
                    Array.Empty<LocalizedString>(), 
                    type: GUIMessageBox.Type.InGame, 
                    icon: specificMission.Prefab.Icon)
                {
                    IconColor = specificMission.Prefab.IconColor,
                    UserData = "missionstartmessage"
                };
            }
        #endif               

            // Ensure the server sends mission initialization data to all clients
        #if SERVER
            foreach (Client client in GameMain.Server.ConnectedClients)
            {
                IWriteMessage msg = new WriteOnlyMessage();
                specificMission.ServerWriteInitial(msg, client);
                NetUtil.SendClient(msg, client.Connection, DeliveryMethod.Reliable);
                
            }
            specificMission.Start(Level.Loaded);
        #endif

            // GameMain.GameSession.GameMode.Start(); // This reset the game as a zoom in effect at the begining of the round.
            
            
            

            yield return CoroutineStatus.Success;
        }




        private static void GenerateRuin()
        {
            // Get the size of the level (as a Point)
            Point levelSize = Level.Loaded.Size;

            // Construct a rectangle representing the level bounds
            Rectangle levelBounds = new Rectangle(0, 0, levelSize.X, levelSize.Y);

            // Define a variable for the ruin position
            Point ruinPos;
            bool isValidPosition = false;

            // Loop to find a valid ruin position that's not inside a wall
            do
            {
                // Randomly choose a position within the level bounds
                int ruinPosX = Rand.Range(levelBounds.X, levelBounds.Right, Rand.RandSync.ServerAndClient);
                int ruinPosY = Rand.Range(levelBounds.Y, levelBounds.Bottom, Rand.RandSync.ServerAndClient);

                // Create the Point for ruinPos
                ruinPos = new Point(ruinPosX, ruinPosY);

                // Convert ruinPos (Point) to worldPosition (Vector2)
                Vector2 worldPosition = new Vector2(ruinPos.X, ruinPos.Y);

                // Check if the position is inside a wall
                isValidPosition = !Level.Loaded.IsPositionInsideWall(worldPosition);

            } while (!isValidPosition);  // Keep looping until a valid position is found

            // Set other parameters for the ruin generation
            bool mirror = false;                    // No mirroring
            bool requireMissionReadyRuin = false;   // Does not need to be mission-ready

            // Generate the ruin at the valid position
            Level.Loaded.GenerateRuin(ruinPos, mirror, requireMissionReadyRuin);

        }

		private static void GenerateBeacon()
        {
            // Need to get some randomness here.
            Level.Loaded.LevelData.ForceBeaconStation = new SubmarineInfo("Content/Map/BeaconStations/BeaconStation_AlienResearch.sub");
            
            Level.Loaded.CreateBeaconStation();
            Level.Loaded.DisconnectBeaconStationWires(0.6f);
            Level.Loaded.DamageBeaconStationDevices(0.6f);
            Level.Loaded.DamageBeaconStationWalls(0.6f);

        }

        private static void GenerateWreck()
        {
            
            // This can create wrecks. Seems by order. Because I always got the same two wrecks.
            Level.Loaded.CreateWrecks();
            
        }

        private static void GenerateEnemySub()
        {
            var enemySubmarineInfo = new SubmarineInfo("Content/Submarines/Humpback.sub");

            if (enemySubmarineInfo != null)
            {
                DebugConsole.NewMessage("Attempting to spawn enemy submarine.");
                Submarine.MainSubs[1] = new Submarine(enemySubmarineInfo, true);
                DebugConsole.NewMessage($"Enemy submarine spawned: {Submarine.MainSubs[1] != null}");
            }
            else
            {
                DebugConsole.ThrowError("Enemy submarine info is null, cannot spawn submarine.");
            }

        }
		
		
		

    }
}
