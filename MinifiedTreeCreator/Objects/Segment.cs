using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinifiedTreeCreator.Objects
{
    class Segment
    {
        private double? _distance;
        internal Boolean IsRedundant
        {
            get;
            private set;
        }
        internal Boolean IsFinal
        {
            get;
            private set;
        }
        internal GraphPoint A;
        internal GraphPoint B;
        internal double Distance
        {
            get 
            {
                if (_distance.HasValue) return _distance.Value;
                return (double)(_distance = GetDistance());
            }
        }

        #region constructors
        public Segment() { }

        public Segment(GraphPoint a, GraphPoint b)
        {
            A = a;
            B = b;
        }
        #endregion

        #region override methods
        public override bool Equals(object s)
        {
            if (s is Segment)
            {
                Segment seg = (Segment)s;
                return GraphPoint.Min(A, B).Equals(GraphPoint.Min(seg.A, seg.B)) && GraphPoint.Max(A, B).Equals(GraphPoint.Max(seg.A, seg.B));
            }
            return false;
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        #endregion

        #region internal methods
        internal double GetDistance()
        {
            return A.GetDistance(B);
        }

        internal GraphPoint OtherPoint(GraphPoint p)
        {
            if (p.Equals(A)) return B;
            if (p.Equals(B)) return A;
            return null;
        }

        internal bool ConnectedTo(Segment s)
        {
            foreach (Segment seg in A.Segments)
            {
                if (seg.Equals(this)) continue;
                if (seg.Equals(s)) return true;
                if (seg.ConnectedTo(s)) return true;
            }
            foreach (Segment seg in B.Segments)
            {
                if (seg.Equals(this)) continue;
                if (seg.Equals(s)) return true;
                if (seg.ConnectedTo(s)) return true;
            }

            return false;
        }

        internal bool Contains(GraphPoint p)
        {
            return p.Equals(A) || p.Equals(B);
        }

        internal void MakeFinal()
        {
            this.IsFinal = true;
            if (!A.Segments.Contains(this)) A.Segments.Add(this);
            if (!B.Segments.Contains(this)) B.Segments.Add(this);
        }

        internal void MakeRedundant()
        {
            this.IsRedundant = true;
        }
        #endregion
    }
}
