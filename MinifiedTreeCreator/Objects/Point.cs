using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinifiedTreeCreator.Objects
{
    class GraphPoint
    {
        #region internal properties
        internal string Name;
        internal double X;
        internal double Y;
        internal Point Point;
        internal Rectangle Rectangle;
        internal List<Segment> Segments;
        #endregion

        #region constructors
        public GraphPoint() 
        {
            Segments = new List<Segment>();
        }
        public GraphPoint(string name, double x, double y)
        {
            Name = name;
            X = x;
            Y = y;
            Segments = new List<Segment>();
        }
        #endregion

        #region override methods
        public override bool Equals(object p)
        {
            if (p is GraphPoint)
            {
                GraphPoint point = (GraphPoint)p;
                return point.X == X && point.Y == Y;
            }
            return false;
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        public override string ToString()
        {
            return String.Format("{0}: ({1}, {2})", Name, X, Y);
        }
        #endregion

        #region internal methods
        /// <summary>
        /// Returns the distance between this point object and the parameter.
        /// </summary>
        /// <param name="p">The point to check.</param>
        /// <returns></returns>
        internal double GetDistance(GraphPoint p)
        {
            return Math.Abs(Math.Sqrt(Math.Pow((X - p.X), 2) + Math.Pow((Y - p.Y), 2)));
        }
        /// <summary>
        /// Public method to begin the recursive check for if a point is connected to this point object.
        /// </summary>
        /// <param name="p">The point to check.</param>
        /// <returns></returns>
        internal bool ConnectedTo(GraphPoint p)
        {
            return ConnectedTo(p, new List<Segment>());
        }
        /// <summary>
        /// Returns true if the physical location of the point on the chart is within the bounds of the passed in x, y coordinate values.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        internal bool IsWithinBounds(int x, int y)
        {
            return x > Rectangle.X && x < Rectangle.X + Rectangle.Width && y > Rectangle.Y && y < Rectangle.Y + Rectangle.Height;
        }
        #endregion

        #region static methods
        /// <summary>
        /// Returns the smaller item based on name.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static GraphPoint Min(GraphPoint a, GraphPoint b)
        {
            if (a.Name.CompareTo(b.Name) <= 0) return a;
            return b;
        }
        /// <summary>
        /// Returns the larger item based on name.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static GraphPoint Max(GraphPoint a, GraphPoint b)
        {
            if (a.Name.CompareTo(b.Name) > 0) return a;
            return b;
        }
        #endregion

        #region private methods
        /// <summary>
        /// Recursive function that checks if the point is connected to this point.
        /// </summary>
        /// <param name="p"></param>
        /// <param name="checkedSegments"></param>
        /// <returns></returns>
        private bool ConnectedTo(GraphPoint p, List<Segment> checkedSegments)
        {
            //the same point, clearly connected
            if (Equals(p)) return true;

            //check all the segments connected to this one
            foreach (Segment s in Segments)
            {
                if (checkedSegments.Contains(s)) continue;//already checked, continue looping
                if (s.Contains(p)) return true;//point is directly connected through a segment
                checkedSegments.Add(s);//segment has been checked, don't try it again.
                if (s.OtherPoint(this).ConnectedTo(p, checkedSegments)) return true;//perform recursion to check if point is connected to the other point on the segment.
            }

            return false;
        }
        #endregion
    }
}
