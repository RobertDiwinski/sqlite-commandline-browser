using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleTools
{
   public class ListView : IConsoleControl
   {
      private DataTableView dtv;
      public ObservableCollection<string> Items { get; } = new ObservableCollection<string>();
      public Rectangle Bounds { get => dtv.Bounds; set => dtv.Bounds = value; }

      private bool updating = false;

      public ListView(Rectangle bounds, ConsoleBuffer buffer)
      {
         dtv = new DataTableView(null, bounds, buffer) { DrawColumnDividers = true, DrawDetails = false, DrawHeader = false, ActAsList = true };
         Items.CollectionChanged += Items_CollectionChanged;
      }

      private void Items_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
      {
         DrawControl();
      }

      public int SelectedIndex
      {
         get
         {
            var cell = dtv.SelectedCell;
            return cell.X * (dtv?.dt?.Rows.Count ?? 0) + cell.Y;
         }
      }

      public void BeginUpdate() => updating = true;
      public void EndUpdate()
      {
         updating = false;
         DrawControl();
      }

      public void DrawControl()
      {
         if (!updating)
         {
            var dt = new DataTable();
            var colCount = (int)Math.Ceiling((double)Items.Count / (double)Bounds.Height);

            for (int i = 0; i < colCount; i++)
               dt.Columns.Add("Col" + i.ToString(), typeof(string));

            var row = 0;
            var col = 0;
            var len = 0;

            var oo = new object[Math.Min(Bounds.Height, Items.Count)][];

            foreach (var it in Items)
            {
               if (oo[row] == null)
               {
                  oo[row] = new object[colCount];

                  for (int i = 0; i < colCount; i++)
                     oo[row][i] = DBNull.Value;
               }

               len = Math.Max(len, it?.Length ?? 0);
               oo[row][col] = it!;
               row++;

               if (row >= Bounds.Height)
               {
                  col++;
                  row = 0;
               }
            }

            foreach (var o in oo)
               dt.Rows.Add(o);

            dtv.ColumnWidth = len;
            dtv.dt = dt;
            dtv.DrawControl();
         }
      }

      public void HandleKey(ConsoleKeyInfo keyInfo)
      {
         dtv.HandleKey(keyInfo);
      }
   }
}
