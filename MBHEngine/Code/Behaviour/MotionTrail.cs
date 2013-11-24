using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MBHEngine.GameObject;
using Microsoft.Xna.Framework;
using MBHEngineContentDefs;
using MBHEngine.Behaviour;

namespace MBHEngine.Behaviour
{
    public class MotionTrail : MBHEngine.Behaviour.Behaviour
    {
        private List<Vector2> mHistory;
        private Int32 mNumHistory;

        public class GetMotionTrailHistoryMessage : BehaviourMessage
        {
            public List<Vector2> mHistory_Out;

            /// <summary>
            /// See parent.
            /// </summary>
            public override void Reset()
            {
                mHistory_Out = null;
            }
        }

        /// <summary>
        /// Constructor which also handles the process of loading in the Behaviour
        /// Definition information.
        /// </summary>
        /// <param name="parentGOH">The game object that this behaviour is attached to.</param>
        /// <param name="fileName">The file defining this behaviour.</param>
        public MotionTrail(GameObject.GameObject parentGOH, String fileName)
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

            MotionTrailDefinition def = GameObjectManager.pInstance.pContentManager.Load<MotionTrailDefinition>(fileName);

            mNumHistory = def.mNumHistory;

            mHistory = new List<Vector2>(mNumHistory);
        }

        /// <summary>
        /// See parent.
        /// </summary>
        /// <param name="gameTime"></param>
        public override void PostUpdate(Microsoft.Xna.Framework.GameTime gameTime)
        {
            base.PostUpdate(gameTime);

            if (mHistory.Count >= mNumHistory)
            {
                mHistory.RemoveAt(0);
            }

            mHistory.Add(mParentGOH.pPosition);
        }

        /// <summary>
        /// See parent.
        /// </summary>
        public override void OnEnable()
        {
            base.OnEnable();

            // In some cased the motion trail will only be enabled for certain states,
            // and in those cases we want to make sure we aren't showing motion trails
            // that are from some time ago.
            mHistory.Clear();
        }

        /// <summary>
        /// See parent.
        /// </summary>
        /// <param name="msg"></param>
        public override void OnMessage(ref BehaviourMessage msg)
        {
            if (pIsEnabled && msg is GetMotionTrailHistoryMessage)
            {
                GetMotionTrailHistoryMessage temp = (GetMotionTrailHistoryMessage)msg;

                temp.mHistory_Out = mHistory;
            }
        }
    }
}
