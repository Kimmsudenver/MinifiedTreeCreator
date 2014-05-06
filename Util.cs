using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinifiedTreeCreator
{
    /// <summary>
    /// Simple static utilities class
    /// </summary>
    public static class Util
    {
        /// <summary>
        /// Scales a the value to fit within a range of values.
        /// </summary>
        /// <param name="min">The original min</param>
        /// <param name="max">The original max</param>
        /// <param name="minOut">The resulting min</param>
        /// <param name="maxOut">The resulting max</param>
        /// <param name="val">The value to convert</param>
        /// <returns></returns>
        internal static Int32 Scale(int min, int max, int minOut, int maxOut, int val)
        {
            return ((maxOut - minOut) * (val - min)) / (max - min) + minOut;
        }
        
        /// <summary>
        /// Returns true if the passed in object can be converted to a numeric data type.
        /// </summary>
        /// <param name="Expression"></param>
        /// <returns></returns>
        internal static bool IsNumeric(System.Object Expression)
        {
            if(Expression == null || Expression is DateTime)
                return false;

            if(Expression is Int16 || Expression is Int32 || Expression is Int64 || Expression is Decimal || Expression is Single || Expression is Double || Expression is Boolean)
                return true;
   
            try 
            {
                if(Expression is string)
                    Double.Parse(Expression as string);
                else
                    Double.Parse(Expression.ToString());
                return true;
            } catch {} // just dismiss errors but return false
            return false;
        }
    }
}
