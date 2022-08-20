using ConsoleTools;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sqlite_commandline_browser
{
   public class QueryView : ConsoleTools.IConsoleControl
   {
      private DataTableView dtv;
      private TextEditor txt;
      private TextEditor errorTxt;

      private Func<string, DataTable?>? ExecuteQuery;
      private Func<string, int?>? ExecuteNonQuery;
      private string? query;
      private ConsoleBuffer? buffer;
      private bool resultActive = false;
      private bool viewError = false;

      public Rectangle Bounds { get; set; }

      public void DrawControl()
      {
         if (buffer != null)
         {
            if (dtv != null)
            {
               buffer.ForegroundColor = ConsoleColor.Gray;
               buffer.BackgroundColor = ConsoleColor.Black;

               dtv.Bounds = new Rectangle(Bounds.Left, Bounds.Top + Bounds.Height / 2, Bounds.Width, Bounds.Height / 2);
               buffer.SetCursorPosition(Bounds.Left, dtv.Bounds.Top - 1);
               buffer.ForegroundColor = ConsoleColor.Gray;
               buffer.BackgroundColor = ConsoleColor.Black;
               buffer.Write(ConsoleBuffer.HorizontalLine, Bounds.Width);

               buffer.SetCursorPosition((Bounds.Width - 8) / 2, dtv.Bounds.Top - 1);
               if (resultActive) buffer.ForegroundColor = ConsoleColor.Yellow;
               buffer.Write(" Result ");
               if (!viewError) dtv.DrawControl();

               if (errorTxt != null && viewError)
               {
                  errorTxt.Bounds = dtv.Bounds;
                  errorTxt.DrawControl();
               }

               if (txt != null)
               {
                  txt.Bounds = new Rectangle(Bounds.Left, Bounds.Top, Bounds.Width, dtv.Bounds.Top - 2);
                  txt?.DrawControl();
               }
            }
         }
      }

      public void HandleKey(ConsoleKeyInfo keyInfo)
      {
         if (keyInfo.Key == ConsoleKey.Tab)
         {
            resultActive = !resultActive;
            DrawControl();
         }
         else if (keyInfo.Key == ConsoleKey.F5 && !string.IsNullOrEmpty(txt.Text)) 
         {
            if (txt.Text.Trim().StartsWith("select", StringComparison.OrdinalIgnoreCase)) ExecQuery(txt.Text);
            else ExecNonQuery(txt.Text);
         }
         else
         {
            if (resultActive && !viewError) dtv?.HandleKey(keyInfo);
            else if (resultActive && viewError) errorTxt?.HandleKey(keyInfo);
            else txt?.HandleKey(keyInfo);
         }
      }

      private void ExecQuery(string? command)
      {
         if (!string.IsNullOrEmpty(command))
         {
            try
            {
               viewError = false;
               var dt = ExecuteQuery?.Invoke(command);
               dtv?.SetDataTable(dt!);
               DrawControl();
            }
            catch (Exception ex)
            {
               errorTxt.Text = ex.Message;
               viewError = true;
               DrawControl();
            }
         }
      }

         private void ExecNonQuery(string? command)
      {
         if (!string.IsNullOrEmpty(command))
         {
            viewError = true; // so that the textbox is displayed

            try
            {
               var count = ExecuteNonQuery?.Invoke(command);
               errorTxt.Text = string.Format("{0} row(s) affected.", count ?? -1);
               DrawControl();
            }
            catch (Exception ex)
            {
               errorTxt.Text = ex.Message;
               DrawControl();
            }
         }
      }

      public QueryView(string? query, Func<string, DataTable?> executeQuery, Func<string, int?> executeNonQuery, Rectangle bounds, ConsoleBuffer buffer)
      {
         this.Bounds = bounds;
         this.ExecuteQuery = executeQuery;
         this.ExecuteNonQuery = executeNonQuery;
         this.query = query;
         this.buffer = buffer;

         dtv = new DataTableView(null, new Rectangle(bounds.Left, bounds.Top + bounds.Height / 2, bounds.Width, bounds.Height / 2), buffer);
         ExecQuery(query);

         txt = new TextEditor(new Rectangle(bounds.Left, bounds.Top, bounds.Width, dtv.Bounds.Top - 2), buffer);
         txt.Text = query;

         errorTxt = new TextEditor(dtv.Bounds, buffer);
         errorTxt.ReadOnly = true;

         DrawControl();
      }
   }
}
