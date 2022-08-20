using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleTools
{
   public struct Point
   {
      public int X { get; set; }
      public int Y { get; set; }

      public Point()
      {
         X = 0;
         Y = 0;
      }

      public Point(int x, int y)
      {
         this.X = x;
         this.Y = y;
      }
   }
}
