using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleTools
{
   public class TextEditor : IConsoleControl
   {
      private ConsoleBuffer buffer;
      private Point cursorPosition = new Point(0, 0);
      private Point firstVisibleCell = new Point(0, 0);

      public TextEditor(Rectangle bounds, ConsoleBuffer buffer)
      {
         this.Bounds = bounds;
         this.buffer = buffer;
      }

      public Rectangle Bounds { get; set; }

      public void DrawControl()
      {
         if (buffer != null)
         {
            buffer.ForegroundColor = ConsoleColor.Gray;
            buffer.BackgroundColor = ConsoleColor.Black;
            buffer.ClearRect(Bounds);

            var spl = GetLines();

            if (cursorPosition.Y >= firstVisibleCell.Y + Bounds.Height)
               firstVisibleCell.Y = cursorPosition.Y - Bounds.Height + 1;
            else if (cursorPosition.Y < firstVisibleCell.Y)
               firstVisibleCell.Y = cursorPosition.Y;

            if (cursorPosition.X >= firstVisibleCell.X + Bounds.Width)
               firstVisibleCell.X = cursorPosition.X - Bounds.Width + 1;
            else if (cursorPosition.X < firstVisibleCell.X)
               firstVisibleCell.X = cursorPosition.X;

            var y = Bounds.Top;

            for (int row = firstVisibleCell.Y; row < spl.Count; row++)
            {
               var line = spl[row];
               if (line.Length > firstVisibleCell.X) line = line.Substring(firstVisibleCell.X);
               else line = string.Empty;

               if (line.Length > Bounds.Width) line = line.Substring(0, Bounds.Width);

               buffer.SetCursorPosition(Bounds.Left, y);

               if (row == cursorPosition.Y && cursorPosition.X >= firstVisibleCell.X && cursorPosition.X < firstVisibleCell.X + Bounds.Width)
               {
                  if (cursorPosition.X - firstVisibleCell.X >= line.Length)
                  {
                     buffer.Write(line);
                     buffer.ForegroundColor = ConsoleColor.Black;
                     buffer.BackgroundColor = ConsoleColor.Gray;
                     buffer.Write('\0');
                     buffer.ForegroundColor = ConsoleColor.Gray;
                     buffer.BackgroundColor = ConsoleColor.Black;
                  }
                  else
                  {
                     var line1 = line.Substring(0, cursorPosition.X - firstVisibleCell.X);
                     var c = line[cursorPosition.X - firstVisibleCell.X];
                     var line2 = line.Substring(cursorPosition.X - firstVisibleCell.X + 1);

                     buffer.Write(line1);
                     buffer.ForegroundColor = ConsoleColor.Black;
                     buffer.BackgroundColor = ConsoleColor.Gray;
                     buffer.Write(c);
                     buffer.ForegroundColor = ConsoleColor.Gray;
                     buffer.BackgroundColor = ConsoleColor.Black;
                     buffer.Write(line2);
                  }
               }
               else buffer.Write(line);
                  
               y++;
               if (y >= Bounds.Bottom) break;
            }
         }
      }

      public bool ReadOnly { get; set; } = false;

      public void HandleKey(ConsoleKeyInfo keyInfo)
      {
         if (keyInfo.Key == ConsoleKey.RightArrow)
         {
            var spl = GetLines();

            if (spl != null && spl.Count > cursorPosition.Y)
            {
               if (cursorPosition.X < spl[cursorPosition.Y].Length) cursorPosition.X++;
               else if (cursorPosition.Y < spl.Count - 1)
               {
                  cursorPosition.Y++;
                  cursorPosition.X = 0;
               }

               DrawControl();
            }
         }
         else if (keyInfo.Key == ConsoleKey.LeftArrow && (cursorPosition.X > 0 || cursorPosition.Y > 0))
         {
            if (cursorPosition.X > 0) cursorPosition.X--;
            else if (cursorPosition.Y > 0)
            {
               var spl = GetLines();

               if (spl != null)
               {
                  cursorPosition.Y--;
                  cursorPosition.X = spl[cursorPosition.Y].Length;
               }
            }

            DrawControl();
         }
         else if (keyInfo.Key == ConsoleKey.UpArrow && (cursorPosition.Y > 0 || cursorPosition.X > 0))
         {
            if (cursorPosition.Y == 0) cursorPosition.X = 0;
            else
            {
               var spl = GetLines();

               if (spl != null)
               {
                  cursorPosition.Y--;
                  if (cursorPosition.X > spl[cursorPosition.Y].Length) cursorPosition.X = spl[cursorPosition.Y].Length;
               }
            }

            DrawControl();
         }
         else if (keyInfo.Key == ConsoleKey.DownArrow)
         {
            var spl = GetLines();

            if (spl != null && (cursorPosition.Y < spl.Count || cursorPosition.X < spl[cursorPosition.Y].Length))
            {
               if (cursorPosition.Y < spl.Count - 1)
               {
                  cursorPosition.Y++;
                  if (cursorPosition.X > spl[cursorPosition.Y].Length) cursorPosition.X = spl[cursorPosition.Y].Length;
               }
               else cursorPosition.X = spl[cursorPosition.Y].Length;

               DrawControl();
            }
         }
         else if (keyInfo.Key == ConsoleKey.Home)
         {
            if (keyInfo.Modifiers == ConsoleModifiers.Control)
            {
               if (cursorPosition.X != 0 || cursorPosition.Y != 0)
               {
                  cursorPosition.X = 0;
                  cursorPosition.Y = 0;

                  DrawControl();
               }

            }
            else if (cursorPosition.X > 0)
            {
               cursorPosition.X = 0;
               DrawControl();
            }
         }
         else if (keyInfo.Key == ConsoleKey.End)
         {
            var spl = GetLines();

            if (spl != null)
            {
               if (keyInfo.Modifiers == ConsoleModifiers.Control)
               {
                  if (cursorPosition.Y < spl.Count - 1 || cursorPosition.X < spl[cursorPosition.Y].Length)
                  {
                     cursorPosition.Y = spl.Count - 1;
                     cursorPosition.X = spl[cursorPosition.Y].Length;
                     DrawControl();
                  }
               }
               else if (cursorPosition.X < spl[cursorPosition.Y].Length)
               {
                  cursorPosition.X = spl[cursorPosition.Y].Length;
                  DrawControl();
               }
            }
         }
         else if (keyInfo.Key == ConsoleKey.PageUp)
         {
            var spl = GetLines();

            if (spl != null && cursorPosition.Y > 0)
            {
               cursorPosition.Y = Math.Max(0, cursorPosition.Y - Bounds.Height);
               if (cursorPosition.X > spl[cursorPosition.Y].Length) cursorPosition.X = spl[cursorPosition.Y].Length;
               DrawControl();
            }
         }
         else if (keyInfo.Key == ConsoleKey.PageDown)
         {
            var spl = GetLines();

            if (spl != null && cursorPosition.Y < spl.Count - 1)
            {
               cursorPosition.Y = Math.Min(spl.Count - 1, cursorPosition.Y + Bounds.Height);
               if (cursorPosition.X > spl[cursorPosition.Y].Length) cursorPosition.X = spl[cursorPosition.Y].Length;
               DrawControl();
            }
         }
         else if (keyInfo.Key == ConsoleKey.Delete && !ReadOnly)
         {
            var spl = GetLines();

            if (spl != null && cursorPosition.Y < spl.Count)
            {
               if (cursorPosition.X < spl[cursorPosition.Y].Length) spl[cursorPosition.Y] = spl[cursorPosition.Y].Remove(cursorPosition.X, 1);
               else if (cursorPosition.Y < spl.Count - 1)
               {
                  spl[cursorPosition.Y] += spl[cursorPosition.Y + 1];
                  spl.RemoveAt(cursorPosition.Y + 1);
               }

               SetLines(spl);
               DrawControl();
            }
         }
         else if (keyInfo.Key == ConsoleKey.Backspace && !ReadOnly)
         {
            var spl = GetLines();

            if (spl != null && (cursorPosition.X > 0 || cursorPosition.Y > 0))
            {
               if (cursorPosition.X > 0)
               {
                  spl[cursorPosition.Y] = spl[cursorPosition.Y].Remove(cursorPosition.X - 1, 1);
                  cursorPosition.X--;
                  SetLines(spl);
               }
               else
               {
                  cursorPosition.Y--;
                  cursorPosition.X = spl[cursorPosition.Y].Length;
                  spl[cursorPosition.Y] += spl[cursorPosition.Y + 1];
                  spl.RemoveAt(cursorPosition.Y + 1);
                  SetLines(spl);
               }

               DrawControl();
            }
         }
         else if (keyInfo.Key == ConsoleKey.Enter && !ReadOnly)
         {
            var spl = GetLines();

            if (spl != null)
            {
               spl.Insert(cursorPosition.Y + 1, spl[cursorPosition.Y].Substring(cursorPosition.X));
               spl[cursorPosition.Y] = spl[cursorPosition.Y].Remove(cursorPosition.X);
               cursorPosition.X = 0;
               cursorPosition.Y++;
               SetLines(spl);
               DrawControl();
            }
         }
         else if (keyInfo.Key == ConsoleKey.K && keyInfo.Modifiers == ConsoleModifiers.Control)
         {
            var spl = GetLines();
            spl.RemoveAt(cursorPosition.Y);
            SetLines(spl);
         }
         else if (!char.IsControl(keyInfo.KeyChar) && !ReadOnly)
         {
            var spl = GetLines();

            if (spl != null)
            {
               spl[cursorPosition.Y] = spl[cursorPosition.Y].Insert(cursorPosition.X, char.ToString(keyInfo.KeyChar));
               SetLines(spl);
               cursorPosition.X++;
               DrawControl();
            }
         }
      }

      private string? text;

      public string? Text
      {
         get { return text; }
         set
         {
            text = value;
            DrawControl();
         }
      }

      private List<string> GetLines()
      {
         var ret = Text?.Split(Environment.NewLine)?.ToList() ?? new List<string>();
         if (ret.Count == 0) ret.Add(string.Empty);

         return ret;
      }

      private void SetLines(IEnumerable<string> lines)
      {
         if (lines != null) Text = string.Join(Environment.NewLine, lines);
         else Text = string.Empty;
      }
   }
}
