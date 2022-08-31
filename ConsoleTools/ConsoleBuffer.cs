using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleTools
{
   public class ConsoleBuffer
   {
      private const string COLOR_RESET = "\u001b[0m";
      private const string COLOR_UNDERLINE = "\u001b[4m";
      private const string COLOR_INVERT = "\u001b[7m";

      private const string FG_BLACK = "\u001b[30m";
      private const string FG_DARKRED = "\u001b[31m";
      private const string FG_DARKGREEN = "\u001b[32m";
      private const string FG_DARKYELLOW = "\u001b[33m";
      private const string FG_DARKBLUE = "\u001b[34m";
      private const string FG_DARKMAGENTA = "\u001b[35m";
      private const string FG_DARKCYAN = "\u001b[36m";
      private const string FG_GRAY = "\u001b[37m";
      private const string FG_DARKGRAY = "\u001b[30;1m";
      private const string FG_RED = "\u001b[31;1m";
      private const string FG_GREEN = "\u001b[32;1m";
      private const string FG_YELLOW = "\u001b[33;1m";
      private const string FG_BLUE = "\u001b[34;1m";
      private const string FG_MAGENTA = "\u001b[35;1m";
      private const string FG_CYAN = "\u001b[36;1m";
      private const string FG_WHITE = "\u001b[37;1m";

      private const string BG_BLACK = "\u001b[40m";
      private const string BG_DARKRED = "\u001b[41m";
      private const string BG_DARKGREEN = "\u001b[42m";
      private const string BG_DARKYELLOW = "\u001b[43m";
      private const string BG_DARKBLUE = "\u001b[44m";
      private const string BG_DARKMAGENTA = "\u001b[45m";
      private const string BG_DARKCYAN = "\u001b[46m";
      private const string BG_GRAY = "\u001b[47m";
      private const string BG_DARKGRAY = "\u001b[40;1m";
      private const string BG_RED = "\u001b[41;1m";
      private const string BG_GREEN = "\u001b[42;1m";
      private const string BG_YELLOW = "\u001b[43;1m";
      private const string BG_BLUE = "\u001b[44;1m";
      private const string BG_MAGENTA = "\u001b[45;1m";
      private const string BG_CYAN = "\u001b[46;1m";
      private const string BG_WHITE = "\u001b[47;1m";

      public const char CornerTopLeft = '┌';
      public const char CornerTopLeftDbl = '╔';
      public const char CornerTopRight = '┐';
      public const char CornerTopRightDbl = '╗';
      public const char CornerBottomLeft = '└';
      public const char CornerBottomLeftDbl = '╚';
      public const char CornerBottomRight = '┘';
      public const char CornerBottomRightDbl = '╝';
      public const char VerticalLine = '│';
      public const char VerticalLineDbl = '║';
      public const char HorizontalLine = '─';
      public const char HorizontalLineDbl = '═';
      public const char TSectionDown = '┬';
      public const char TSectionDownDbl = '╦';
      public const char TSectionUp = '┴';
      public const char TSectionUpDbl = '╩';
      public const char TSectionLeft = '┤';
      public const char TSectionLeftDbl = '╣';
      public const char TSectionRight = '├';
      public const char TSectionRightDbl = '╠';
      public const char CrossSection = '┼';
      public const char CrossSectionDbl = '╬';

      // ┌─┬─┐
      // │ │ │ 
      // ├─┼─┤
      // │ │ │
      // └─┴─┘
      // ╔═╦═╗
      // ║ ║ ║
      // ╠═╬═╣
      // ║ ║ ║ 
      // ╚═╩═╝

      //║═╔╗╚╝╬╩╦╣╠
      //│─┌┐└┘┼┴┬┤├

      public int Width { get; private set; }
      public int Height { get; private set; }

      private char[,] buffer;
      private ConsoleColor[,] fgs;
      private ConsoleColor[,] bgs;

      public int CursorLeft { get; private set; } = 0;
      public int CursorTop { get; private set; } = 0;

      public ConsoleBuffer(int width, int height)
      {
         this.Width = width;
         this.Height = height;

         buffer = new char[width, height];
         fgs = new ConsoleColor[width, height];
         bgs = new ConsoleColor[width, height];

         for (int x = 0; x < width; x++)
         {
            for (int y = 0; y < height; y++)
            {
               fgs[x, y] = ConsoleColor.Gray;
               bgs[x, y] = ConsoleColor.Black;
            }
         }
      }

      public void SetCursorPosition(int x, int y)
      {
         if (x < 0 || x >= Width) throw new ArgumentOutOfRangeException(nameof(x));
         if (y < 0 || y >= Height) throw new ArgumentOutOfRangeException(nameof(y));

         CursorLeft = x;
         CursorTop = y;
      }

      public void WriteLine(string text)
      {
         Write(text);
         if (CursorTop < Height - 1) CursorTop++;
      }

      public void WriteLine(char c, int count = 1)
      {
         Write(c, count);
         if (CursorTop < Height - 1) CursorTop++;
      }

      public void Write(string text)
      {
         var x = CursorLeft;
         var y = CursorTop;

         foreach (var c in text)
         {
            if (x == Width)
            {
               x = 0;
               y++;

               if (y == Height) break;
            }

            buffer[x, y] = c;
            fgs[x, y] = ForegroundColor;
            bgs[x, y] = BackgroundColor;

            x++;
         }

         CursorLeft = x;
         CursorTop = y;
      }

      public void Write(char c, Rectangle rect)
      {
         for (int x = rect.Left; x < rect.Right; x++)
         {
            for (int y = rect.Top; y < rect.Bottom; y++)
            {
               buffer[x, y] = c;
               fgs[x, y] = ForegroundColor;
               bgs[x, y] = BackgroundColor;
            }
         }
      }

      public void Write(char c, int count = 1)
      {
         var x = CursorLeft;
         var y = CursorTop;

         for (int i = 0; i < count; i++)
         {
            if (x == Width)
            {
               x = 0;
               y++;

               if (y == Height) break;
            }

            buffer[x, y] = c;
            fgs[x, y] = ForegroundColor;
            bgs[x, y] = BackgroundColor;

            x++;
         }

         CursorLeft = x;
         CursorTop = y;
      }

      public void ClearRect(Rectangle rect)
      {
         for (int y = rect.Top; y < rect.Bottom; y++)
         {
            SetCursorPosition(rect.Left, y);
            Write('\0', rect.Width);
         }
      }

      public void DrawRect(Rectangle rect, bool doubleLines)
      {
         SetCursorPosition(rect.Left, rect.Top);

         if (doubleLines)
         {
            Write(CornerTopLeftDbl);
            Write(HorizontalLineDbl, rect.Width - 2);
            Write(CornerTopRightDbl);
         }
         else
         {
            Write(CornerTopLeft);
            Write(HorizontalLine, rect.Width - 2);
            Write(CornerTopRight);
         }

         SetCursorPosition(rect.Left, rect.Bottom - 1);

         if (doubleLines)
         {
            Write(CornerBottomLeftDbl);
            Write(HorizontalLineDbl, rect.Width - 2);
            Write(CornerBottomRightDbl);
         }
         else
         {
            Write(CornerBottomLeft);
            Write(HorizontalLine, rect.Width - 2);
            Write(CornerBottomRight);
         }

         for (int y = rect.Top + 1; y < rect.Bottom - 1; y++)
         {
            SetCursorPosition(rect.Left, y);
            if (doubleLines) Write(VerticalLineDbl);
            else Write(VerticalLine);

            SetCursorPosition(rect.Right - 1, y);
            if (doubleLines) Write(VerticalLineDbl);
            else Write(VerticalLine);
         }
      }

      public ConsoleColor ForegroundColor { get; set; } = ConsoleColor.Gray;
      public ConsoleColor BackgroundColor { get; set; } = ConsoleColor.Black;

      private void AppendColor(ConsoleColor fg, ConsoleColor bg, StringBuilder sb)
      {
         sb.Append(COLOR_RESET);

         switch (bg)
         {
            case ConsoleColor.Black: sb.Append(BG_BLACK); break;
            case ConsoleColor.Blue: sb.Append(BG_BLUE); break;
            case ConsoleColor.Cyan: sb.Append(BG_CYAN); break;
            case ConsoleColor.DarkBlue: sb.Append(BG_DARKBLUE); break;
            case ConsoleColor.DarkCyan: sb.Append(BG_DARKCYAN); break;
            case ConsoleColor.DarkGray: sb.Append(BG_DARKGRAY); break;
            case ConsoleColor.DarkGreen: sb.Append(BG_DARKGREEN); break;
            case ConsoleColor.DarkMagenta: sb.Append(BG_DARKMAGENTA); break;
            case ConsoleColor.DarkRed: sb.Append(BG_DARKRED); break;
            case ConsoleColor.DarkYellow: sb.Append(BG_DARKYELLOW); break;
            case ConsoleColor.Gray: sb.Append(BG_GRAY); break;
            case ConsoleColor.Green: sb.Append(BG_GREEN); break;
            case ConsoleColor.Magenta: sb.Append(BG_MAGENTA); break;
            case ConsoleColor.Red: sb.Append(BG_RED); break;
            case ConsoleColor.White: sb.Append(BG_WHITE); break;
            case ConsoleColor.Yellow: sb.Append(BG_YELLOW); break;
         }

         switch (fg)
         {
            case ConsoleColor.Black: sb.Append(FG_BLACK); break;
            case ConsoleColor.Blue: sb.Append(FG_BLUE); break;
            case ConsoleColor.Cyan: sb.Append(FG_CYAN); break;
            case ConsoleColor.DarkBlue: sb.Append(FG_DARKBLUE); break;
            case ConsoleColor.DarkCyan: sb.Append(FG_DARKCYAN); break;
            case ConsoleColor.DarkGray: sb.Append(FG_DARKGRAY); break;
            case ConsoleColor.DarkGreen: sb.Append(FG_DARKGREEN); break;
            case ConsoleColor.DarkMagenta: sb.Append(FG_DARKMAGENTA); break;
            case ConsoleColor.DarkRed: sb.Append(FG_DARKRED); break;
            case ConsoleColor.DarkYellow: sb.Append(FG_DARKYELLOW); break;
            case ConsoleColor.Gray: sb.Append(FG_GRAY); break;
            case ConsoleColor.Green: sb.Append(FG_GREEN); break;
            case ConsoleColor.Magenta: sb.Append(FG_MAGENTA); break;
            case ConsoleColor.Red: sb.Append(FG_RED); break;
            case ConsoleColor.White: sb.Append(FG_WHITE); break;
            case ConsoleColor.Yellow: sb.Append(FG_YELLOW); break;
         }
      }

      public void Draw()
      {
         var sb = new StringBuilder();
         var lastFg = ConsoleColor.Gray;
         var lastBg = ConsoleColor.Black;

         AppendColor(lastFg, lastBg, sb);

         for (int y = 0; y < Height; y++) 
         {
            for (int x = 0; x < Width; x++)
            {
               var fg = fgs[x, y];
               var bg = bgs[x, y];

               if (fg != lastFg || bg != lastBg)
               {
                  if (fg != lastFg) lastFg = fg;
                  if (bg != lastBg) lastBg = bg;

                  AppendColor(lastFg, lastBg, sb);
               }

               if (buffer[x, y] == '\0') sb.Append(' ');
               else sb.Append(buffer[x, y]);
            }
         }

         Console.SetCursorPosition(0, 0);
         using (var s = Console.OpenStandardOutput())
         {
            var buf = Console.OutputEncoding.GetBytes(sb.ToString());
            s.Write(buf, 0, buf.Length);
            s.Flush();
         }
      }

      public void Resize(int newWidth, int newHeight)
      {
         var newBuffer = new char[newWidth, newHeight];
         var newFgs = new ConsoleColor[newWidth, newHeight];
         var newBgs = new ConsoleColor[newWidth, newHeight];

         for (int x = 0; x < Math.Min(newWidth, Width); x++)
         {
            for (int y = 0; y < Math.Min(newHeight, Height); y++)
            {
               newBuffer[x, y] = buffer[x, y];
               newFgs[x, y] = fgs[x, y];
               newBgs[x, y] = bgs[x, y];
            }
         }

         buffer = newBuffer;
         fgs = newFgs;
         bgs = newBgs;

         this.Width = newWidth;
         this.Height = newHeight;
      }

      public static Dictionary<string, IList<string>> ParseArguments(string[] args, bool caseSensitive = true, string[]? argsNoValue = null, string[]? argsMultiValue = null)
      {
         var ret = new Dictionary<string, IList<string>>();

         if (args != null && args.Length != 0)
         {
            var hsNoValue = new HashSet<string>(caseSensitive || argsNoValue == null ? argsNoValue ?? new string[0] : from s in argsNoValue select s?.ToLower());
            var hsMultiValue = new HashSet<string>(caseSensitive || argsMultiValue == null ? argsMultiValue ?? new string[0] : from s in argsMultiValue select s?.ToLower());

            var lastParam = string.Empty;

            foreach (var s in args)
            {
               if (s.StartsWith("-") || s.StartsWith("/"))
               {
                  if (s.StartsWith("--")) lastParam = s.Substring(2);
                  else lastParam = s.Substring(1);

                  if (!caseSensitive) lastParam = lastParam.ToLower();
                  if (!ret.ContainsKey(lastParam)) ret[lastParam] = new List<string>();

                  if (hsNoValue.Contains(lastParam)) lastParam = string.Empty;
               }
               else
               {
                  if (!ret.ContainsKey(lastParam)) ret[lastParam] = new List<string>();
                  ret[lastParam].Add(s);
                  if (!hsMultiValue.Contains(lastParam)) lastParam = string.Empty;
               }
            }
         }

         return ret;
      }
   }
}
