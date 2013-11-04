using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MBHEngine.GameObject;

namespace BumpSetSpike.Behaviour.FSM
{
    /// <summary>
    /// State machine for the main menu and all the sub-screens.
    /// </summary>
    class FSMMainMenu : MBHEngine.Behaviour.FiniteStateMachine
    {
        /// <summary>
        /// See parent.
        /// </summary>
        /// <param name="parentGOH"></param>
        /// <param name="fileName"></param>
        public FSMMainMenu(GameObject parentGOH, String fileName)
            : base(parentGOH, fileName)
        {
        }

        /// <summary>
        /// See parent.
        /// </summary>
        /// <param name="fileName"></param>
        public override void LoadContent(string fileName)
        {
            base.LoadContent(fileName);

            AddState(new StateMainMenuRoot(), "StateMainMenuRoot");
            AddState(new StateMainMenuModeSelect(), "StateMainMenuModeSelect");
            AddState(new StateMainMenuCameraPan(), "StateMainMenuCameraPan");
            AddState(new StateMainMenuCredits(), "StateMainMenuCredits");
        }
    }
}
