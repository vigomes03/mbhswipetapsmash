using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using MBHEngineContentDefs;
using MBHEngine.GameObject;
using MBHEngine.Trial;

namespace MBHEngine.Behaviour
{
    /// <summary>
    /// Moves the object based on its forward direction and speed.
    /// </summary>
    public class EnableForTrial : Behaviour
    {
        /// <summary>
        /// Constructor which also handles the process of loading in the Behaviour
        /// Definition information.
        /// </summary>
        /// <param name="parentGOH">The game object that this behaviour is attached to.</param>
        /// <param name="fileName">The file defining this behaviour.</param>
        public EnableForTrial(GameObject.GameObject parentGOH, String fileName)
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

            UpdateStatus();
        }

        /// <summary>
        /// See parent.
        /// </summary>
        /// <param name="msg"></param>
        public override void OnMessage(ref BehaviourMessage msg)
        {
            if (msg is TrialModeManager.OnTrialModeChangedMessage)
            {
                UpdateStatus();
            }
        }

        private void UpdateStatus()
        {
            if (TrialModeManager.pInstance.pIsTrialMode)
            {
                mParentGOH.pDoUpdate = mParentGOH.pDoRender = true;
            }
            else
            {
                mParentGOH.pDoUpdate = mParentGOH.pDoRender = false;
            }
        }
    }
}
