using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MBHEngine.Behaviour;
using MBHEngine.GameObject;
using MBHEngine.Input;
using BumpSetSpike.Gameflow;

namespace BumpSetSpike.Behaviour.FSM
{
    /// <summary>
    /// Just an empty state to wait for the FSM to trigger a new state. Can't just use the base class because it is
    /// abstract.
    /// </summary>
    class StateEmpty : MBHEngine.StateMachine.FSMState
    {
    }
}
