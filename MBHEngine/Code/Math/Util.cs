using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace MBHEngine.Math
{
    /// <summary>
    /// Helper Math functions that don't fit into a particular class.
    /// </summary>
    public class Util
    {
        /// <summary>
        /// If you took a 2D array and converted it to a contiguous 1D array, this function would find 
        /// the corrisponding index from the 2D array in the 1D array.
        /// </summary>
        /// <param name="width">The width of 2D array we are mapping from.</param>
        /// <param name="pos">The X and Y indicies of the 2D array we are mapping from.</param>
        /// <returns>And index in a 1D array of equivalent size of the 2D array we are mapping from.</returns>
        static public Int32 Map2DTo1DArray(Int32 width, Vector2 pos)
        {
            return width * (Int32)pos.Y + (Int32)pos.X;
        }

        static public Vector2 GetArcVelocity(Vector2 source, Vector2 destination, Single speed, Single gravity)
        {
            Vector2 vel = destination - source;

            Single xDist = vel.X;

            Single yDist = vel.Y;

            vel.Normalize();

            Single time = System.Math.Abs(xDist / speed);

            Single timeHalf = time * 0.5f;

            Single yVel = timeHalf * gravity;

            vel.X = vel.X * speed;
            vel.Y = -yVel;

            return vel;
        }
    }
}
