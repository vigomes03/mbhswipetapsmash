using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MBHEngine.GameObject;

namespace BumpSetSpike.Behaviour.FSM
{
    class FSMPauseScreen : MBHEngine.Behaviour.FiniteStateMachine
    {
        public FSMPauseScreen(GameObject parentGOH, String fileName)
            : base(parentGOH, fileName)
        {
        }

        public override void LoadContent(string fileName)
        {
            base.LoadContent(fileName);

            AddState(new StatePauseRoot(), "StatePauseRoot");
        }
    }
}
