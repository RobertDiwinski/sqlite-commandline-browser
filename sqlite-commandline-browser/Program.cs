using Microsoft.Data.Sqlite;
using System.Data;
using System.Text;
using ConsoleTools;
using System.Reflection;

namespace sqlite_commandline_browser
{
   internal class Program
   {
      private enum AppState { Tables, Query } 

      private static SqliteConnection? conn;
      private static IConsoleControl? currentView;
      private static ConsoleBuffer? buffer;
      private static AppState state = AppState.Tables;
      private static List<QueryView> queryViews = new List<QueryView>();
      private static int queryViewIndex = 0;

      static void Main(string[] args)
      {
         try
         {
            var dic = ConsoleBuffer.ParseArguments(args, false, new string[] { "c", "create", "h", "help" });

            if (dic.ContainsKey("h") || dic.ContainsKey("help"))
            {
               PrintHelp();
               return;
            }

            string? dbPath = null;

            if (dic.ContainsKey(string.Empty) && dic[string.Empty].Count == 1)
               dbPath = dic[string.Empty][0];

            if (string.IsNullOrEmpty(dbPath))
            {
               Console.WriteLine("No database file specified");
               return;
            }

            IList<string>? password = null;
            if (!dic.TryGetValue("p", out password)) dic.TryGetValue("password", out password);

            var mustSetPassword = !File.Exists(dbPath) && !string.IsNullOrEmpty(password?[0]);
            OpenDatabase(dbPath, dic.ContainsKey("c"), !mustSetPassword ? password?[0] : null);

            if (mustSetPassword) SetPassword(password?[0]);

            Console.OutputEncoding = Encoding.UTF8;
            Console.InputEncoding = Encoding.UTF8;
            Console.CursorVisible = false;
            buffer = new ConsoleBuffer(Console.WindowWidth, Console.WindowHeight);

            ResizeLoop();
            UpdateView();

            for (; ; )
            {
               buffer.Draw();
               var key = Console.ReadKey(true);

               currentView?.HandleKey(key);

               if (key.Key == ConsoleKey.F10) break;
               else if (key.Key == ConsoleKey.F1)
               {
                  state = AppState.Tables;
                  UpdateView();
               }
               else if (key.Key == ConsoleKey.Enter)
               {
                  if (currentView != null && currentView is ListView)
                  {
                     var lv = currentView as ListView;

                     if (lv != null)
                     {
                        state = AppState.Query;
                        var tblName = lv.Items[lv.SelectedIndex];
                        CreateNewQueryViewFromTableName(tblName);
                        UpdateView();
                     }
                  }
               }
               else if (key.Key == ConsoleKey.F2)
               {
                  CreateNewQueryView(null);
                  DrawMenuAndTitle();
               }
               else if (key.Key == ConsoleKey.F3 && state == AppState.Query && queryViewIndex > 0)
               {
                  queryViewIndex--;
                  currentView = queryViews[queryViewIndex];
                  UpdateView();
               }
               else if (key.Key == ConsoleKey.F4 && state == AppState.Query && queryViewIndex < queryViews.Count - 1)
               {
                  queryViewIndex++;
                  currentView = queryViews[queryViewIndex];
                  UpdateView();
               }
               else if (key.Key == ConsoleKey.F8 && state == AppState.Query)
               {
                  queryViews.RemoveAt(queryViewIndex);
                  if (queryViewIndex >= queryViews.Count) queryViewIndex = queryViews.Count - 1;
                  
                  if (queryViews.Count == 0) state = AppState.Tables;
                  else currentView = queryViews[queryViewIndex];

                  UpdateView();
               }
            }
         }
         catch (Exception)
         {
            throw;
         }
         finally
         {
            Console.CursorVisible = true;
            Console.ResetColor();
            Console.Clear();
            conn?.Close();
         }
      }

      private static void PrintHelp()
      {
         var executable = Assembly.GetEntryAssembly()?.Location;
         if (!string.IsNullOrEmpty(executable)) executable = System.IO.Path.GetFileName(executable);
         if (string.IsNullOrEmpty(executable)) executable = "sqlite-commandline-browser";

         Console.WriteLine(executable + " [parameters] /path/to/sqlite.db");
         Console.WriteLine();
         Console.WriteLine("Parameters:");
         Console.WriteLine("-h|--help:     Print this help");
         Console.WriteLine("-c|--create:   Create database if it does not exist");
         Console.WriteLine("-p|--password: Set password for database");
      }

      private static void OpenDatabase(string file, bool create, string? password = null)
      {
         var builder = new SqliteConnectionStringBuilder { Mode = create ? SqliteOpenMode.ReadWriteCreate : SqliteOpenMode.ReadWrite, DataSource = file };
         if (!string.IsNullOrEmpty(password)) builder.Password = password;
         var connStr = builder.ToString();

         conn = new SqliteConnection(connStr);
         conn.Open();
      }

      // https://docs.microsoft.com/de-de/dotnet/standard/data/sqlite/encryption?tabs=netcore-cli
      private static void SetPassword(string? password)
      {
         var cmd = conn?.CreateCommand();

         if (cmd != null)
         {
            cmd.CommandText = "SELECT quote($password);";
            cmd.Parameters.AddWithValue("$password", password ?? string.Empty);
            var quotedNewPassword = (string?)cmd.ExecuteScalar();

            cmd.CommandText = "PRAGMA rekey = " + quotedNewPassword;
            cmd.Parameters.Clear();
            cmd.ExecuteNonQuery();
         }
      }

      private static void DrawTablesList()
      {
         if (buffer != null)
         {
            var dt = ExecuteQuery("SELECT tbl_name FROM sqlite_master WHERE type='table' ORDER BY tbl_name;");

            var lv = new ListView(new Rectangle(0, 1, buffer.Width, buffer.Height - 2), buffer);
            lv.BeginUpdate();

            if (dt != null)
            {
               foreach (var dr in dt.Rows.Cast<DataRow>())
               {
                  var val = dr["tbl_name"] != DBNull.Value ? Convert.ToString(dr["tbl_name"]) : string.Empty;
                  lv.Items.Add(val!);
               }
            }

            currentView = lv;
            lv.EndUpdate();
         }
      }

      private static void UpdateView()
      {
         DrawMenuAndTitle();
         if (state == AppState.Tables) DrawTablesList();
         else currentView?.DrawControl();
      }

      private static void CreateNewQueryViewFromTableName(string tablename)
      {
         var cols = ExecuteQuery(string.Format("SELECT name FROM pragma_table_info('{0}')", tablename));

         if (cols != null)
         {
            var sb = new StringBuilder();
            foreach (var dr in cols.Rows.Cast<DataRow>())
            {
               if (sb.Length != 0)
               {
                  sb.AppendLine(",");
                  sb.Append("       ");
               }

               sb.Append("[");
               sb.Append(Convert.ToString(dr["name"]));
               sb.Append("]");
            }

            sb.AppendLine();

            CreateNewQueryView(string.Format("SELECT {0}FROM {1}", sb.ToString(), tablename));
         }
      }

      private static void DrawTitle(string text)
      {
         if (buffer != null)
         {
            buffer.SetCursorPosition(0, 0);
            buffer.ForegroundColor = ConsoleColor.Gray;
            buffer.BackgroundColor = ConsoleColor.Black;

            buffer.Write(ConsoleBuffer.HorizontalLine, buffer.Width);

            buffer.ForegroundColor = ConsoleColor.Yellow;
            buffer.SetCursorPosition((buffer.Width - text.Length) / 2, 0);
            buffer.Write(text);
         }
      }

      private static void CreateNewQueryView(string? query)
      {
         if (buffer != null)
         {
            var qv = new QueryView(query, ExecuteQuery, ExecuteNonQuery, new Rectangle(0, 1, buffer.Width, buffer.Height - 2), buffer);
            currentView = qv;
            queryViews.Add(qv);
            queryViewIndex = queryViews.Count - 1;
            state = AppState.Query;
         }
      }

      private static void ResizeLoop()
      {
         Task.Run(() =>
         {
            var width = Console.WindowWidth;
            var height = Console.WindowHeight;

            for (; ; )
            {
               var w2 = Console.WindowWidth;
               var h2 = Console.WindowHeight;

               if (w2 != width || h2 != height)
               {
                  buffer?.Resize(w2, h2);

                  foreach (var qv in queryViews)
                  {
                     if (qv != currentView)
                     {
                        var nw = w2 - width + qv.Bounds.Width;
                        var nh = h2 - height + qv.Bounds.Height;

                        qv.Bounds = new Rectangle(qv.Bounds.Left, qv.Bounds.Top, nw, nh);
                     }
                  }

                  if (currentView != null)
                  {
                     var nw = w2 - width + currentView.Bounds.Width;
                     var nh = h2 - height + currentView.Bounds.Height;

                     currentView.Bounds = new Rectangle(currentView.Bounds.Left, currentView.Bounds.Top, nw, nh);
                     currentView.DrawControl();
                  }

                  DrawMenuAndTitle();
                  buffer?.Draw();

                  width = w2;
                  height = h2;
               }

               Thread.Sleep(250);
            }
         });
      }

      private static void DrawMenuAndTitle()
      {
         if (buffer != null)
         {
            if (state == AppState.Tables) DrawTitle(" Tables ");
            else if (state == AppState.Query) DrawTitle(" Query ");

            buffer.SetCursorPosition(0, buffer.Height - 1);
            buffer.ForegroundColor = ConsoleColor.Gray;
            buffer.BackgroundColor = ConsoleColor.Black;
            buffer.Write('\0', buffer.Width);

            buffer.SetCursorPosition(0, buffer.Height - 1);
            buffer.ForegroundColor = ConsoleColor.Gray;
            buffer.BackgroundColor = ConsoleColor.Black;
            buffer.Write("F1");

            buffer.ForegroundColor = ConsoleColor.Black;
            buffer.BackgroundColor = ConsoleColor.Gray;
            buffer.Write("Tables");

            buffer.ForegroundColor = ConsoleColor.Gray;
            buffer.BackgroundColor = ConsoleColor.Black;
            buffer.Write(" F2");

            buffer.ForegroundColor = ConsoleColor.Black;
            buffer.BackgroundColor = ConsoleColor.Gray;
            buffer.Write("New Query");

            if (state == AppState.Tables)
            {
               buffer.ForegroundColor = ConsoleColor.Gray;
               buffer.BackgroundColor = ConsoleColor.Black;
               buffer.Write(" Enter");

               buffer.ForegroundColor = ConsoleColor.Black;
               buffer.BackgroundColor = ConsoleColor.Gray;
               buffer.Write("View Table");
            }
            else if (state == AppState.Query)
            {
               buffer.ForegroundColor = ConsoleColor.Gray;
               buffer.BackgroundColor = ConsoleColor.Black;
               buffer.Write(" F3");

               buffer.ForegroundColor = ConsoleColor.Black;
               buffer.BackgroundColor = ConsoleColor.Gray;
               buffer.Write("Previous");

               buffer.ForegroundColor = ConsoleColor.Gray;
               buffer.BackgroundColor = ConsoleColor.Black;
               buffer.Write(" F4");

               buffer.ForegroundColor = ConsoleColor.Black;
               buffer.BackgroundColor = ConsoleColor.Gray;
               buffer.Write("Next");

               buffer.ForegroundColor = ConsoleColor.Gray;
               buffer.BackgroundColor = ConsoleColor.Black;
               buffer.Write(" F5");

               buffer.ForegroundColor = ConsoleColor.Black;
               buffer.BackgroundColor = ConsoleColor.Gray;
               buffer.Write("Run");

               buffer.ForegroundColor = ConsoleColor.Gray;
               buffer.BackgroundColor = ConsoleColor.Black;
               buffer.Write(" F8");

               buffer.ForegroundColor = ConsoleColor.Black;
               buffer.BackgroundColor = ConsoleColor.Gray;
               buffer.Write("Close");

               buffer.ForegroundColor = ConsoleColor.Gray;
               buffer.BackgroundColor = ConsoleColor.Black;
               buffer.Write(" Tab");

               buffer.ForegroundColor = ConsoleColor.Black;
               buffer.BackgroundColor = ConsoleColor.Gray;
               buffer.Write("Switch Editor/Result");
            }

            buffer.SetCursorPosition(buffer.Width - 7, buffer.Height - 1);
            buffer.ForegroundColor = ConsoleColor.Gray;
            buffer.BackgroundColor = ConsoleColor.Black;
            buffer.Write("F10");

            buffer.ForegroundColor = ConsoleColor.Black;
            buffer.BackgroundColor = ConsoleColor.Gray;
            buffer.Write("Exit");
         }
      }

      private static DataTable? ExecuteQuery(string command)
      {
         var cmd = conn?.CreateCommand();

         if (cmd != null)
         {
            using (cmd)
            {
               cmd.CommandText = command;

               using (var reader = cmd.ExecuteReader())
               {
                  var ret = new DataTable();
                  ret.BeginInit();

                  for (int i = 0; i < reader.FieldCount; i++)
                  {
                     string n = reader.GetName(i);
                     Type t = reader.GetFieldType(i);

                     ret.Columns.Add(n, t);
                  }

                  ret.EndInit();
                  ret.BeginLoadData();

                  while (reader.Read())
                  {
                     List<object> obj = new List<object>();
                     for (int i = 0; i < ret.Columns.Count; i++)
                     {
                        try
                        {
                           object o = reader.GetValue(i);
                           obj.Add(o);
                        }
                        catch (Exception)
                        {
                           obj.Add(System.DBNull.Value);
                        }
                     }

                     ret.Rows.Add(obj.ToArray());
                  }

                  ret.EndLoadData();

                  return ret;
               }
            }
         }

         return null;
      }

      private static int? ExecuteNonQuery(string command)
      {
         var cmd = conn?.CreateCommand();

         if (cmd != null)
         {
            using (cmd)
            {
               cmd.CommandText = command;
               return cmd.ExecuteNonQuery();
            }
         }

         return null;
      }
   }
}
