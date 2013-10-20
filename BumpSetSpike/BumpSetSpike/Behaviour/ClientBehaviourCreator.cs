using System;
using MBHEngine.Behaviour;
using MBHEngine.GameObject;

namespace BumpSetSpike.Behaviour
{
    /// <summary>
    /// This class will be used to allow us to create client side Behaviours.
    /// </summary>
    public class ClientBehaviourCreator : BehaviourCreator
    {
        /// <summary>
        /// Helper function for creating behaviours based on strings of matching names.
        /// </summary>
        /// <param name="go">The game object that this behaviour is being attached to.</param>
        /// <param name="behaviourType">The name of the behaviour class we are creating.</param>
        /// <param name="fileName">The name of the file containing the behaviour definition.</param>
        /// <returns>The newly created behaviour.</returns>
        MBHEngine.Behaviour.Behaviour BehaviourCreator.CreateBehaviourByName(GameObject go, String behaviourType, String fileName)
        {
            switch (behaviourType)
            {
                case "BumpSetSpike.Behaviour.Player":
                    {
                        return new Player(go, fileName);
                    }
                case "BumpSetSpike.Behaviour.Partner":
                    {
                        return new Partner(go, fileName);
                    }
                case "BumpSetSpike.Behaviour.Opponent":
                    {
                        return new Opponent(go, fileName);
                    }
                case "BumpSetSpike.Behaviour.Ball":
                    {
                        return new Ball(go, fileName);
                    }
                case "BumpSetSpike.Behaviour.GroundShadow":
                    {
                        return new GroundShadow(go, fileName);
                    }
                case "BumpSetSpike.Behaviour.MainMenu":
                    {
                        return new MainMenu(go, fileName);
                    }
                case "BumpSetSpike.Behaviour.GameOver":
                    {
                        return new GameOver(go, fileName);
                    }
                case "BumpSetSpike.Behaviour.NewHighScore":
                    {
                        return new NewHighScore(go, fileName);
                    }
                case "BumpSetSpike.Behaviour.PointDisplay":
                    {
                        return new PointDisplay(go, fileName);
                    }
                case "BumpSetSpike.Behaviour.HitCountDisplay":
                    {
                        return new HitCountDisplay(go, fileName);
                    }
                case "BumpSetSpike.Behaviour.Wobble":
                    {
                        return new Wobble(go, fileName);
                    }
                case "BumpSetSpike.Behaviour.Button":
                    {
                        return new Button(go, fileName);
                    }
                case "BumpSetSpike.Behaviour.EffectEmitter":
                    {
                        return new EffectEmitter(go, fileName);
                    }
                case "BumpSetSpike.Behaviour.ScoreSummary":
                    {
                        return new ScoreSummary(go, fileName);
                    }
                case "BumpSetSpike.Behaviour.FSM.FSMPauseScreen":
                    {
                        return new FSM.FSMPauseScreen(go, fileName);
                    }
                default:
                    {
                        return null;
                    }
            }
        }
    }
}
