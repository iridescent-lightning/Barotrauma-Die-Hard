using System;
using Barotrauma;

namespace BarotraumaDieHard
{
    partial class HelloWorld : ACsMod
    {
        public HelloWorld()
        {
            DebugConsole.NewMessage("Hello world!");
        }

        public override void Stop()
        {
            DebugConsole.NewMessage("Goodbye world!");
        }
    }
}
