using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using GomLib;
using MySql.Data.MySqlClient;

namespace PugTools {
  public class SQLInitStore {
    public String InitBegin = "";
    public String InitEnd = "";
    internal SQLData SqlData;
    public String Table = "";

    public void OutputCreationSQL() {
      String defaultQuery = File.ReadAllText("SQL Files\\default_create.sql");

      String columnTypes =
        String.Join(
          Environment.NewLine,
          SqlData.SQLProperties.Select(x => String.Format("  `{0}` {1},", x.Name, x.Type))
        );
      String priunikey =
        SqlData.SQLProperties.Where(x => x.IsPrimaryKey).Select(x => x.Name).First();
      String keyString = "  PRIMARY KEY (`{0}`),\r\n  UNIQUE KEY `id_UNIQUE` (`{0}`)";
      String priString = String.Format(keyString, priunikey);
      String oldString = String.Format(keyString, String.Format("{0}`, `version", priunikey));

      List<String> columnNames = SqlData.SQLProperties.Select(x => x.Name).ToList();
      String columns = String.Join("`, `", columnNames);
      String oldColumns = String.Join("`, OLD.`", columnNames);

      /*
       * {0} = table name
       * {1} = primarykey
       * {2} = column names
       * {3} = Old.column names
       */
      String trigger =
        String.Format(
          @"((NOT EXISTS(SELECT 1 FROM {0}_old_versions WHERE `{1}` =  OLD.`{1}` AND `Hash` = "
          + @"OLD.`Hash`)) AND (NOT (OLD.`Hash` = NEW.`Hash`))) THEN INSERT INTO `{0}_old_versions`"
          + @" (`version`, `{2}`, `Hash`) VALUES (OLD.`current_version`, OLD.`{3}`, OLD.`Hash`)",
          Table,
          priunikey,
          columns,
          oldColumns
        );

      String indexString =
        String.Join(
          Environment.NewLine,
          SqlData.SQLProperties.Where(x => x.AddIndex).Select(x =>
            String.Format(
              "ALTER TABLE `{0}` ADD INDEX `{1}` (`{1}`);",
              Table,
              x.Name
            )
          ).ToList().Union(
          SqlData.SQLProperties.Where(x => x.AddFullTextIndex).Select(x =>
            String.Format(
              "ALTER TABLE `{0}` ADD FULLTEXT INDEX `{1}` (`{1}`);",
              Table,
              x.Name
            )
          ).ToList())
        );

      /*
       * {0} = table name
       * {1} = column types minus version and hash
       * {2} = regular primary unique key statements
       * {3} = trigger if statement
       * {4} = old_versions primary unique key statements
       * {5} = AddIndex statements
       */
      String creationQuery =
        String.Format(
          defaultQuery,
          Table,
          columnTypes,
          priString,
          trigger,
          oldString,
          indexString
        );

      Tools.WriteFile(
        creationQuery,
        String.Format("SQL Creation Files\\{0}_create.sql", Table),
        false
      );
    }
    public SQLInitStore(String b, String e) {
      InitBegin = b;
      InitEnd = e;
    }
    public SQLInitStore(String name, Object obj) {
      Table = name;
      // This has been  redesigned to elminate most of the grunt work to make new sql outputs.
      SqlData = new SQLData();

      if (obj is GomLib.Models.GameObject @object) {
        // not implemented yet
        SqlData = @object.SQLInfo();
      } else if (obj is GomLib.Models.PseudoGameObject object1) {
        // returns the SQLInfo related to the type. It's just a list of SQLProperties right now.
        SqlData = object1.SQLInfo();
      }

      // Use LINQ to suck all the sql column names into a list.
      List<String> names =
        SqlData.SQLProperties.Select(x => x.Name).ToList();

      // Join the name list together and create a basic insert query for the type
      InitBegin =
        String.Format(
          @"USE `tor_dump`; INSERT INTO `{0}` (`current_version`, `previous_version`, `first_seen`"
          + @", `{1}`, `Hash`) VALUES ",
          name,
          string.Join("`, `", names)
        );

      // Same thing, but slightly reversed. Use linq to take the name list and turn it into a 
      // formatted string for each row, then join those lines together with with a newline. It's 
      // better to use the String.Join/Format options so you're not spawning a billion new strings 
      // like when you + them together.
      InitEnd =
        String.Format(
          @"ON DUPLICATE KEY UPDATE `previous_version` = IF((@update_record := (`Hash` <> "
          + @"VALUES(`Hash`))), `current_version`, `previous_version`), `current_version` = "
          + @"IF(@update_record, VALUES(`current_version`), `current_version`), {0} `Hash` = "
          + @"IF(@update_record, VALUES(`Hash`), `Hash`);",
          String.Join(
            Environment.NewLine,
            names.Select(x => string.Format(
              "`{0}` = IF(@update_record, VALUES(`{0}`), `{0}`),",
              x)
            )
          )
        );
    }
  }

  internal partial class Tools {
    private protected static MySqlConnection _conn;
    private protected static MySqlDataReader _reader;

    public static String BaseQuery { get; set; }
    public static String BaseStr { get; set; }
    static protected MySqlConnection Conn {
      get => _conn;
      set => _conn = value;
    }
    public static String EndQuery { get; set; }
    public static Dictionary<String, SQLInitStore> InitTable { get; set; } =
      new Dictionary<String, SQLInitStore> {
        { "Abilities", new SQLInitStore(
            "ability",
            new GomLib.Models.Ability()
          )
        },
        { "AchCategories", new SQLInitStore(
            "achcategories",
            new GomLib.Models.AchievementCategory()
          )
        },
        { "Achievements", new SQLInitStore(
            "achievement",
            new GomLib.Models.Achievement()
          )
        },
        { "CodexEntries", new SQLInitStore(
            "codex",
            new GomLib.Models.Codex()
          )
        },
        { "Items", new SQLInitStore(
            "item",
            new GomLib.Models.Item()
          )
        },
        { "Schematics", new SQLInitStore(
            "schematic",
            new GomLib.Models.Schematic()
          )
        },
        { "Quests", new SQLInitStore(
            "mission",
            new GomLib.Models.Quest()
          )
        },
        { "Tooltip", new SQLInitStore(
            "tooltip",
            new GomLib.Models.Tooltip()
          )
        },
        { "ItemAppearances", new SQLInitStore(
            "itemappearance",
            new GomLib.Models.ItemAppearance()
          )
        },
        { "Talents", new SQLInitStore(
            "talent",
            new GomLib.Models.Talent()
          )
        },
        { "Npcs", new SQLInitStore(
            "npc",
            new GomLib.Models.Npc()
          )
        },
        { "NewCompanions", new SQLInitStore(
            "companion",
            new GomLib.Models.NewCompanion()
          )
        }
      };
    public static String PatchVersion { get; set; } = "";
    static protected MySqlDataReader Reader {
      get => _reader;
      set => _reader = value;
    }
    public static List<String> StoredQueryValues { get; set; }

    #region SQL Functions
    /// <summary>
    /// <para>Call this function with the value you want to add to the batch insert queue.</para>
    /// 
    /// Example:
    /// </summary>
    /// <example><code>
    /// sqlAddTransactionValue("(100, 'Name 1', 'Value 1', 'Other 1')");
    /// </code></example>
    public void SqlAddTransactionValue(String value) {
      if (_sql) {
        StoredQueryValues.Add(value);
        // This can be enabled if the queries are getting too big.
        if (StoredQueryValues.Count > 10000) {
          AddToList1("Query limit reached. Stopping to insert values into to mysql table");
          SqlExecTransaction();
          AddToList1("Done.");
          /*
          if (chkBuildCompare.Checked) {
            // On the fly inserts are too slow with this method. Dumping to file instead and we'll 
            // do a import from file.
            sqlExecTransaction();
          } else {
            WriteFile(String.Join(",", storedQueryValues) + ",", baseStr + ".sql", true);
            storedQueryValues = new List<string>();
          }
          */
        }
      }
    }
    public void SqlCreate() {
      // Items Table
      String create = File.ReadAllText("SQL Files\\item_create.sql");
      SqlExec(create);

      // Abilities
      create = File.ReadAllText("SQL Files\\abilities_create.sql");
      SqlExec(create);

      AddToList1("Database Tables created.");
    }
    public MySqlDataReader SqlExec(String info) {
      if (_sql) {
        String mysqlString =
          String.Format("SERVER={0};", txtSqlAddress.Text)
          + String.Format("DATABASE={0};", txtSqlName.Text)
          + String.Format("UID={0};", txtSqlUsername.Text)
          + String.Format("PASSWORD={0};", txtSqlPassword.Text);

        Conn = new MySqlConnection(mysqlString);
        Conn.Open();

        MySqlCommand command = Conn.CreateCommand();
        command.CommandText = info;
        Reader = command.ExecuteReader();

        Conn.Close();

        return Reader;
      }

      return null;
    }
    /// <summary>
    /// This function is for executing batch inserts into a Mysql DB - This is all wrong now
    /// 
    /// <para>The baseQuery will be something like: "INSERT INTO example (example_id, name, value, 
    /// other_value) VALUES"</para>
    /// 
    /// values will be in this format:
    /// <code>
    /// {
    ///   "(100, 'Name 1', 'Value 1', 'Other 1')",
    ///   "(101, 'Name 2', 'Value 2', 'Other 2')",
    ///   "(102, 'Name 3', 'Value 3', 'Other 3')",
    ///   "(103, 'Name 4', 'Value 4', 'Other 4')"
    /// }
    /// </code>
    /// </summary>
    public void SqlExecTransaction() {
      if (_sql) {
        String mysqlString =
          String.Format("SERVER={0};", txtSqlAddress.Text)
          + String.Format("DATABASE={0};", txtSqlName.Text)
          + String.Format("UID={0};", txtSqlUsername.Text)
          + String.Format("PASSWORD={0};", txtSqlPassword.Text)
          + "default command timeout=0; Allow User Variables = True";

        Conn = new MySqlConnection(mysqlString);
        Conn.Open();

        MySqlCommand command = Conn.CreateCommand();
        MySqlTransaction trans;
        trans = Conn.BeginTransaction();
        command.Connection = Conn;
        command.Transaction = trans;

        try {
          command.CommandText = BaseQuery + string.Join(",", StoredQueryValues) + EndQuery;
          // command.Parameters.AddWithValue("@update_record", false);
          command.ExecuteNonQuery();
          trans.Commit();
        }
        catch (Exception ex) {
          Debug.WriteLine(ex);

          try {
            trans.Rollback();
          }
          catch (MySqlException mse) {
            Debug.WriteLine(mse);
          }
        }

        Conn.Close();
        Conn.Dispose();

        StoredQueryValues = new List<String>();
      }
    }
    public void SqlLoadFromFile(String file) {
      String mysqlString =
        String.Format("SERVER={0};", txtSqlAddress.Text)
        + String.Format("DATABASE={0};", txtSqlName.Text)
        + String.Format("UID={0};", txtSqlUsername.Text)
        + String.Format("PASSWORD={0};", txtSqlPassword.Text)
        + "default command timeout=0;";

      Conn = new MySqlConnection(mysqlString);
      Conn.Open();

      MySqlScript script =
        new MySqlScript(Conn, File.ReadAllText(file)); // Config.ExtractPath + file));
      script.Execute();
      Conn.Close();
    }
    public static String SqlSani(String str) {
      if (str == null) return "";
      else return MySqlHelper.EscapeString(str);
    }
    /// <summary>
    /// Call this function to flush out any remaining stored values in the multiple insert queue,
    /// and clear the baseQuery.
    /// </summary>
    public void SqlTransactionsFlush() {
      if (_sql) {
        if (StoredQueryValues.Count > 0) {
          /*
          if (chkBuildCompare.Checked) {
            // On the fly inserts are too slow with this method. Dumping to file instead and we'll
            // do an import from file.
            sqlExecTransaction();
          } else {
            WriteFile(String.Join(",", storedQueryValues), baseStr + ".sql", true);
            storedQueryValues = new List<String>();
          }
          */
        }
        // WriteFile(";" + endQuery, baseStr + ".sql", true);
        AddToList1("Finalizing inserts/updates to mysql table");
        SqlExecTransaction();
        AddToList1("Done.");
        BaseStr = "";
        BaseQuery = "";
        EndQuery = "";

        /*
        String msg = "Rebuild the table completely? This can take a very long time.";
        if (MessageBox.Show("Mysql", msg, MessageBoxButtons.YesNo) 
            == System.Windows.Forms.DialogResult.Yes) {
          Thread oSqlLoadFromFile = new Thread(new ParameterizedThreadStart(sqlLoadFromFile));
          oSqlLoadFromFile.Start(baseStr + ".sql");
        }
        */
      }
    }
    /// <summary>
    /// Call this function with your base query to initialize the batch insert queue.
    /// </summary>
    /// <example><code>
    /// sqlTransactionsInitialize(
    ///   "INSERT INTO example (example_id, name, value, other_value) VALUES"
    /// );
    /// </code></example>
    public void SqlTransactionsInitialize(String bStr, String endStr) {
      if (_sql) {
        StoredQueryValues = new List<String>();
        // if (chkBuildCompare.Checked)
        // {
        BaseQuery = bStr;
        EndQuery = endStr;
        // } else {
        //   baseStr = bStr;
        //   String fullQuery = File.ReadAllText(baseStr + ".sql");
        //   fullQuery = fullQuery.Replace("`tor_dump`", "`" + textBox4.Text + "`");
        //   Int32 index = 
        //     fullQuery.IndexOf("INSERT INTO `" + baseStr + "` VALUES ") + 22 + baseStr.Length;
        //   baseQuery = fullQuery.Substring(0, index);
        //   endQuery = fullQuery.Substring(index);
        //   WriteFile(baseQuery, baseStr + ".sql", false);
        // }
      }
    }
    private static String ToSQL(Object obj) {
      if (obj is GomLib.Models.GameObject @object) return @object.ToSQL(PatchVersion);
      else if (obj is GomLib.Models.PseudoGameObject object1) return object1.ToSQL(PatchVersion);

      return "";
    }
    #endregion
  }
}
