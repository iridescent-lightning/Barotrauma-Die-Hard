using Barotrauma;
using Barotrauma.Items.Components;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;

namespace AIObjectiveGetWrenchMod
{
    class AIObjectiveGetWrench : AIObjectiveGetItem
    {
        private static readonly Identifier WrenchIdentifier = "wrench".ToIdentifier();

        public AIObjectiveGetWrench(Character character, AIObjectiveManagerMod.AIObjectiveManager objectiveManager, float priorityModifier = 1) 
            : base(character, WrenchIdentifier, objectiveManager, equip: true, priorityModifier: priorityModifier)
        {
        }

        public override void Act(float deltaTime)
        {
            // Implement custom logic here if needed, or use the base behavior
            base.Act(deltaTime);
        }
    }
}
