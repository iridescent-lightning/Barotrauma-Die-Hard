/*using Barotrauma;
using Barotrauma.Items.Components;
using Barotrauma.Extensions;
using Microsoft.Xna.Framework;

namespace BarotraumaDieHard
{
    class ShipIssueWorkerControlSonar : ShipIssueWorker
    {
        private Sonar sonar;

        public ShipIssueWorkerControlSonar(ShipCommandManager manager, Order order, Sonar sonar) : base(manager, order)
        {
            this.sonar = sonar;
        }

        public override void CalculateImportanceSpecific()
        {
            if (shipCommandManager.NavigationState == ShipCommandManager.NavigationStates.Inactive) { return; }
            if (TargetItemComponent is Powered { HasPower: false }) { return; }
            if (TargetItem.Condition <= 0f) { return; }

            Importance = 70f;
            DebugConsole.NewMessage("Commencing order");
        }
    }
}
*/