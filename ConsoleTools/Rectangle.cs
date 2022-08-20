using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleTools
{
   public struct Rectangle
   {
      public int Left { get; set; }
      public int Top { get; set; }
      public int Width { get; set; }
      public int Height { get; set; }
      public int Right { get { return Left + Width; } }
      public int Bottom { get { return Top + Height; } }

      public Rectangle() 
      {
         Left = 0;
         Top = 0;
         Width = 0;
         Height = 0;
      }

      public Rectangle(int x, int y, int width, int height)
      {
         this.Left = x;
         this.Top = y;
         this.Width = width;
         this.Height = height;
      }
   }
}
