using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleTools
{
   public class DataTableView : IConsoleControl
   {
      private ConsoleBuffer buffer;
      internal DataTable? dt;
      public Rectangle Bounds { get; set; }

      private Point firstVisibleCell = new Point(0, 0);
      private Point selectedCell = new Point(0, 0);

      public Point SelectedCell { get { return new Point(selectedCell.X, selectedCell.Y); } }

      private int columnWidth = 15;
      public virtual int ColumnWidth
      {
         get { return columnWidth; }
         set
         {
            columnWidth = value;
            DrawControl();
         }
      }

      private bool drawHeader = true;
      public virtual bool DrawHeader
      {
         get { return drawHeader; }
         set
         {
            drawHeader = value;
            DrawControl();
         }
      }

      private bool drawDetails = true;
      public virtual bool DrawDetails
      {
         get { return drawDetails; }
         set
         {
            drawDetails = value;
            DrawControl();
         }
      }

      private bool drawColumnDividers = true;
      public virtual bool DrawColumnDividers
      {
         get { return drawColumnDividers; }
         set
         {
            drawColumnDividers = value;
            DrawControl();
         }
      }

      internal bool ActAsList { get; set; } = false;

      public DataTableView(DataTable? dt, Rectangle bounds, ConsoleBuffer buffer)
      {
         this.dt = dt;
         this.Bounds = bounds;
         this.buffer = buffer;
         DrawControl();
      }

      public void DrawControl()
      {
         buffer.ForegroundColor = ConsoleColor.Gray;
         buffer.BackgroundColor = ConsoleColor.Black;
         buffer.Write('\0', Bounds);

         if (dt != null)
         {
            #region Header
            if (DrawHeader)
            {
               buffer.SetCursorPosition(Bounds.Left, Bounds.Top);
               buffer.BackgroundColor = ConsoleColor.Black;

               for (int i = firstVisibleCell.X; i < dt.Columns.Count; i++)
               {
                  var spaceLeft = ColumnWidth;
                  if (buffer.CursorLeft + spaceLeft > Bounds.Right) spaceLeft = Bounds.Right - buffer.CursorLeft;

                  var col = dt.Columns[i];
                  var n = col.Caption;
                  if (n.Length > spaceLeft) n = n.Substring(0, spaceLeft);

                  buffer.ForegroundColor = ConsoleColor.Yellow;
                  buffer.Write(n);

                  for (int j = 0; j < spaceLeft - n.Length; j++)
                     buffer.Write(" ");

                  if (buffer.CursorLeft == Bounds.Right) break;

                  buffer.ForegroundColor = ConsoleColor.Gray;
                  if (DrawColumnDividers) buffer.Write(ConsoleBuffer.VerticalLine);
               }
            }
            #endregion

            #region Rows
            buffer.SetCursorPosition(Bounds.Left, Bounds.Top + (DrawHeader ? 1 : 0));
            buffer.BackgroundColor = ConsoleColor.Black;
            buffer.ForegroundColor = ConsoleColor.Gray;

            for (int y = firstVisibleCell.Y; y < dt.Rows.Count; y++)
            {
               var dr = dt.Rows[y];

               for (int x = firstVisibleCell.X; x < dt.Columns.Count; x++)
               {
                  var spaceLeft = ColumnWidth;
                  if (buffer.CursorLeft + spaceLeft > Bounds.Right) spaceLeft = Bounds.Right - buffer.CursorLeft;

                  if (x == selectedCell.X && y == selectedCell.Y)
                  {
                     buffer.ForegroundColor = ConsoleColor.Black;
                     buffer.BackgroundColor = ConsoleColor.Gray;
                  }
                  else
                  {
                     buffer.BackgroundColor = ConsoleColor.Black;
                     buffer.ForegroundColor = ConsoleColor.Gray;
                  }

                  var v = dr[x];
                  var n = (v == null || v == DBNull.Value ? (ActAsList ? String.Empty : "NULL") : v.ToString());
                  if (n!.Length > spaceLeft) n = n.Substring(0, spaceLeft);

                  buffer.Write(n);

                  for (int j = 0; j < spaceLeft - n.Length; j++)
                     buffer.Write(' ');

                  if (buffer.CursorLeft == Bounds.Right) break;

                  if (DrawColumnDividers)
                  {
                     if (x == selectedCell.X && y == selectedCell.Y)
                     {
                        buffer.BackgroundColor = ConsoleColor.Black;
                        buffer.ForegroundColor = ConsoleColor.Gray;
                     }

                     buffer.Write(ConsoleBuffer.VerticalLine);
                  }
               }

               if (buffer.CursorTop > Bounds.Bottom - (DrawDetails ? 3 : 2)) break;
               buffer.SetCursorPosition(Bounds.Left, buffer.CursorTop + 1);
            }
            #endregion

            #region Details
            if (DrawDetails && dt.Rows.Count > selectedCell.Y && dt.Columns.Count > selectedCell.X && selectedCell.Y >= 0 && selectedCell.X >= 0)
            {
               buffer.SetCursorPosition(Bounds.Left, Bounds.Bottom - 1);
               buffer.BackgroundColor = ConsoleColor.Black;
               buffer.ForegroundColor = ConsoleColor.Blue;

               var val = dt.Rows[selectedCell.Y][selectedCell.X];
               var t = dt.Columns[selectedCell.X].Caption + "|" + (selectedCell.Y + 1).ToString() + ": " + (val == null || val == DBNull.Value ? "NULL" : val.ToString()?.Replace("\r", string.Empty).Replace("\n", string.Empty));
               if (t.Length > Bounds.Width) t = t.Substring(0, Bounds.Width);

               buffer.Write(t);

               for (int j = 0; j < Bounds.Width - t.Length; j++)
                  buffer.Write(" ");
            }
            #endregion
         }
      }

      public void SetDataTable(DataTable dt)
      {
         this.dt = dt;
         selectedCell = new Point(0, 0);
      }

      public void HandleKey(ConsoleKeyInfo keyInfo)
      {
         if (dt != null)
         {
            if (keyInfo.Key == ConsoleKey.LeftArrow && selectedCell.X > 0)
            {
               selectedCell.X--;
               if (selectedCell.X < firstVisibleCell.X) firstVisibleCell.X--;

               DrawControl();
            }
            else if (keyInfo.Key == ConsoleKey.RightArrow && selectedCell.X < dt.Columns.Count - 1)
            {
               selectedCell.X++;
               if (selectedCell.X >= firstVisibleCell.X + Bounds.Width / (ColumnWidth + (DrawColumnDividers ? 1 : 0))) firstVisibleCell.X++;

               if (ActAsList && selectedCell.X == dt.Columns.Count - 1 && dt.Rows[selectedCell.Y][selectedCell.X] == DBNull.Value)
               {
                  for (int i = selectedCell.Y - 1; i >= 0; i--)
                  {
                     if (dt.Rows[i][selectedCell.X] != DBNull.Value)
                     {
                        selectedCell.Y = i;
                        break;
                     }
                  }
               }

               DrawControl();
            }
            else if (keyInfo.Key == ConsoleKey.UpArrow && selectedCell.Y > 0)
            {
               selectedCell.Y--;
               if (selectedCell.Y < firstVisibleCell.Y) firstVisibleCell.Y--;
               DrawControl();
            }
            else if (keyInfo.Key == ConsoleKey.DownArrow && selectedCell.Y < dt.Rows.Count - 1 && !(ActAsList && selectedCell.X == dt.Columns.Count - 1 && dt.Rows[selectedCell.Y + 1][selectedCell.X] == DBNull.Value))
            {
               selectedCell.Y++;
               if (selectedCell.Y > firstVisibleCell.Y + Bounds.Height - (DrawDetails ? 3 : 2) + (DrawHeader ? 0 : 1)) firstVisibleCell.Y++;

               DrawControl();
            }
            else if (ActAsList)
            {
               if (keyInfo.Key == ConsoleKey.DownArrow && selectedCell.X < dt.Columns.Count - 1)
               {
                  selectedCell.X++;
                  selectedCell.Y = 0;
                  DrawControl();
               }
               else if (keyInfo.Key == ConsoleKey.UpArrow && selectedCell.X > 0)
               {
                  selectedCell.X--;
                  selectedCell.Y = dt.Rows.Count - 1;
                  DrawControl();
               }
            }
         }
      }
   }
}
