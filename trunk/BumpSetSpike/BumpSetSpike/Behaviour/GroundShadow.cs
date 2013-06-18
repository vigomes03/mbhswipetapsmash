using System;
using MBHEngine.Behaviour;
using Microsoft.Xna.Framework;
using MBHEngine.GameObject;
using MBHEngine.Math;
using System.Diagnostics;
using BumpSetSpikeContentDefs;
using Microsoft.Xna.Framework.Input.Touch;
using MBHEngine.Render;
using MBHEngine.Debug;
using System.Collections.Generic;

namespace BumpSetSpike.Behaviour
{
    /// <summary>
    /// A shadow object that just follows an object but locked to the ground.
    /// </summary>
    class GroundShadow : MBHEngine.Behaviour.Behaviour
    {
        /// <summary>
        /// Tells this behaviour who to follow.
        /// </summary>
        public class SetTargetMessage : MBHEngine.Behaviour.BehaviourMessage
        {
            /// <summary>
            /// The object to follow.
            /// </summary>
            public GameObject mTarget_In;

            /// <summary>
            /// See parent.
            /// </summary>
            public override void Reset()
            {
                mTarget_In = null;
            }
        };

        /// <summary>
        /// The object to follow.
        /// </summary>
        private GameObject mTarget;

        /// <summary>
        /// Constructor which also handles the process of loading in the Behaviour
        /// Definition information.
        /// </summary>
        /// <param name="parentGOH">The game object that this behaviour is attached to.</param>
        /// <param name="fileName">The file defining this behaviour.</param>
        public GroundShadow(GameObject parentGOH, String fileName)
            : base(parentGOH, fileName)
        {
        }

        /// <summary>
        /// Call this to initialize a Behaviour with data supplied in a file.
        /// </summary>
        /// <param name="fileName">The file to load from.</param>
        public override void LoadContent(String fileName)
        {
            base.LoadContent(fileName);
        }

        /// <summary>
        /// See parent.
        /// </summary>
        /// <param name="gameTime"></param>
        public override void PostUpdate(GameTime gameTime)
        {
            if (null != mTarget)
            {
                // Follow the target but only in the X.
                mParentGOH.pPosX = mTarget.pPosition.X;
            }
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
            if (msg is SetTargetMessage)
            {
                SetTargetMessage temp = (SetTargetMessage)msg;

                mTarget = temp.mTarget_In;
            }
        }
    }
}
