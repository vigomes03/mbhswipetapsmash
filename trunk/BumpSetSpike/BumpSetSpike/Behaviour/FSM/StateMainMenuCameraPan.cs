using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MBHEngine.Behaviour;
using MBHEngine.GameObject;
using MBHEngine.Input;
using BumpSetSpike.Gameflow;
using Microsoft.Xna.Framework.Input.Touch;
using MBHEngine.Math;
using MBHEngineContentDefs;
using MBHEngine.Render;
using Microsoft.Xna.Framework;

namespace BumpSetSpike.Behaviour.FSM
{
    class StateMainMenuCameraPan : MBHEngine.StateMachine.FSMState
    {
        /// <summary>
        /// We don't have a great way of knowing when the camera has reached its desination, so
        /// instead we just start a timer that takes the same amount of time and wait for it to
        /// expire.
        /// </summary>
        private StopWatch mWatch;

        /// <summary>
        /// Preallocated to avoid GC.
        /// </summary>
        private Player.OnGameRestartMessage mGameRestartMsg;

        /// <summary>
        /// Constructor.
        /// </summary>
        public StateMainMenuCameraPan()
            : base()
        {
            mGameRestartMsg = new Player.OnGameRestartMessage();
        }

        /// <summary>
        /// Called once when the state starts.  This is a chance to do things that should only happen once
        /// during a particular state.
        /// </summary>
        public override void OnBegin()
        {
            base.OnBegin();

            // Make the timer last the same amount of time it will take the camera to reach
            // its destination.
            mWatch = StopWatchManager.pInstance.GetNewStopWatch();
            mWatch.pLifeTime = CameraManager.pInstance.pNumBlendFrames;
            mWatch.pIsPaused = false;

            // Move down to the gameplay camera position.
            CameraManager.pInstance.pTargetPosition = new Vector2(0, -30.0f);
        }

        /// <summary>
        /// Called repeatedly until it returns a valid new state to transition to.
        /// </summary>
        /// <returns>Identifier of a state to transition to.  This is the same name passed into AddState 
        /// in the owning FiniteStateMachine.</returns>
        public override string OnUpdate()
        {
            // Once the timer expires the camera should be in place and the game can start.
            if (mWatch.IsExpired())
            {
                mGameRestartMsg.Reset();
                GameObjectManager.pInstance.BroadcastMessage(mGameRestartMsg, pParentGOH);
                GameObjectManager.pInstance.pCurUpdatePass = BehaviourDefinition.Passes.GAME_PLAY;

                TutorialManager.pInstance.StartTutorial();

                GameObjectManager.pInstance.Remove(pParentGOH);
            }

            return base.OnUpdate();
        }

        /// <summary>
        /// Called once when leaving this state.  Called the frame after the Update which returned
        /// a valid state to transition to.  This is a chance to do any clean up needed.
        /// </summary>
        public override void OnEnd()
        {
            base.OnEnd();
        }

        /// <summary>
        /// The main interface for communicating between behaviours.  Using polymorphism, we
        /// define a bunch of different messages deriving from BehaviourMessage.  Each behaviour
        /// can then check for particular upcasted messahe types, and either grab some data 
        /// from it (set message) or store some data in it (get message).
        /// </summary>
        /// <param name="msg">The message being communicated to the behaviour.</param>
        public override void OnMessage(ref BehaviourMessage msg)
        {
            base.OnMessage(ref msg);
        }
    }
}
