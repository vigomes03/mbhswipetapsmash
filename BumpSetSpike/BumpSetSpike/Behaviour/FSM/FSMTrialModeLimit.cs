using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MBHEngine.GameObject;

namespace BumpSetSpike.Behaviour.FSM
{
    /// <summary>
    /// Pause menu.
    /// </summary>
    class FSMTrialModeLimit : MBHEngine.Behaviour.FiniteStateMachine
    {
        /// <summary>
        /// See parent.
        /// </summary>
        /// <param name="parentGOH"></param>
        /// <param name="fileName"></param>
        public FSMTrialModeLimit(GameObject parentGOH, String fileName)
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

            AddState(new StateEmpty(), "StateEmpty");
            AddState(new StateTrialModeLimitRoot(), "StateTrialModeLimitRoot");
            AddState(new StateTrialModeLimitGameplay(), "StateTrialModeLimitGameplay");
        }

        /// <summary>
        /// See parent.
        /// </summary>
        /// <param name="msg"></param>
        public override void OnMessage(ref MBHEngine.Behaviour.BehaviourMessage msg)
        {
            base.OnMessage(ref msg);

            if (msg is HitCountDisplay.TrialScoreLimitReachedMessage)
            {
                if (GetCurrentState() is StateEmpty)
                {
                    AdvanceToState("StateTrialModeLimitRoot");
                }
            }
        }
    }
}
