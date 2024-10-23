using Barotrauma;

namespace BarotraumaDieHard
{
    partial class BeaconMissionDieHard : Mission
    {
        public override bool DisplayAsCompleted => State > 0;
        public override bool DisplayAsFailed => false;
    }
}