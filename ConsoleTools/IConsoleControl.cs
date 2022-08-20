using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleTools
{
   public interface IConsoleControl
   {
      Rectangle Bounds { get; set; }
      void HandleKey(ConsoleKeyInfo keyInfo);
      void DrawControl();
   }
}
