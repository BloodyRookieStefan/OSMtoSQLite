using OSMConverter.OSMTypes;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using static System.Net.Mime.MediaTypeNames;

namespace OSMConverter.lib
{
    internal class SQLController
    {
        private static string DatabaseConnectionStringTemplate = "Data Source={0};Version=3;";
        private static string DatabaseConnectionString;

        internal static TimeSpan ElapsedTime;                                   // Elapsed time for reading storing data

        /// <summary>
        /// Create database file
        /// </summary>
        /// <param name="loc">Location</param>
        internal static void CreateDatabase(string loc)
        {
            // Create directory if not exist yet
            (new FileInfo(loc)).Directory.Create();
            // Delete old database
            if(File.Exists(loc))
                File.Delete(loc);
            // Create file itself
            SQLiteConnection.CreateFile(loc);

            // Create connection string
            DatabaseConnectionString = string.Format(DatabaseConnectionStringTemplate, loc);

            // Create tables
            SQLiteCommand command;
            string sql;
            using (var connection = new SQLiteConnection(DatabaseConnectionString))
            {
                connection.Open();

                // Create empty NODE table
                sql = "CREATE TABLE \"NODES\"(" +
                    "\"ID\"                 INTEGER NOT NULL UNIQUE, " +
                    "\"VERSION\"            TEXT, " +
                    "\"TIMESTAMP\"          TEXT, " +
                    "\"LAT\"                TEXT, " +
                    "\"LONG\"               TEXT, " +
                    "PRIMARY KEY(\"ID\"));";
                command = new SQLiteCommand(sql, connection);
                command.ExecuteNonQuery();

                // Create empty WAY table
                sql = "CREATE TABLE \"WAYS\"(" +
                    "\"ID\"                 INTEGER NOT NULL UNIQUE, " +
                    "\"VERSION\"            TEXT, " +
                    "\"TIMESTAMP\"          TEXT, " +
                    "PRIMARY KEY(\"ID\"));";
                command = new SQLiteCommand(sql, connection);
                command.ExecuteNonQuery();

                // Create empty RELATION table
                sql = "CREATE TABLE \"RELATIONS\"(" +
                    "\"ID\"                 INTEGER NOT NULL UNIQUE, " +
                    "\"VERSION\"            TEXT, " +
                    "\"TIMESTAMP\"          TEXT, " +
                    "PRIMARY KEY(\"ID\"));";
                command = new SQLiteCommand(sql, connection);
                command.ExecuteNonQuery();

                // Create empty TAG table
                sql = "CREATE TABLE \"TAGS\"(" +
                    "\"CONNECTID\"          INTEGER NOT NULL, " +
                    "\"KEY\"                TEXT, " +
                    "\"VALUE\"              TEXT);";
                command = new SQLiteCommand(sql, connection);
                command.ExecuteNonQuery();

                // Create empty NODE REFERENCE table
                sql = "CREATE TABLE \"NODEREFS\"(" +
                    "\"CONNECTID\"          INTEGER NOT NULL, " +
                    "\"REF\"                INTEGER);";
                command = new SQLiteCommand(sql, connection);
                command.ExecuteNonQuery();

                // Create empty MEMBER table
                sql = "CREATE TABLE \"MEMBERS\"(" +
                    "\"CONNECTID\"          INTEGER NOT NULL, " +
                    "\"TYPE\"               TEXT," +
                    "\"REF\"                TEXT," +
                    "\"ROLE\"               TEXT);";
                command = new SQLiteCommand(sql, connection);
                command.ExecuteNonQuery();

                connection.Close();
            }
        }

        internal static void SendDataToSQL(List<Node> nodes, List<Way> ways, List<Relation> relations)
        {
            // Start time
            DateTime start = DateTime.UtcNow;

            using (var connection = new SQLiteConnection(DatabaseConnectionString))
            {
                connection.Open();

                SQLiteCommand sqCommand = new SQLiteCommand();
                sqCommand.Connection = connection;
                SQLiteTransaction transAction;

                // Start a local transaction
                transAction = connection.BeginTransaction();
                // Assign transaction object for a pending local transaction
                sqCommand.Transaction = transAction;

                SendNodes(nodes, sqCommand);
                SendWays(ways, sqCommand);
                SendRelations(relations, sqCommand);

                transAction.Commit();

                connection.Close();
            }

            // End time
            ElapsedTime = DateTime.UtcNow - start;
        }

        private static void SendNodes(List<Node> nodes, SQLiteCommand sqCommand)
        {
            // Do nothing when no nodes
            if (nodes.Count == 0)
                return;

            // Run trough sub lists
            foreach (var node in nodes)
            {
                sqCommand.CommandText =$"INSERT INTO NODES (ID, VERSION, TIMESTAMP,  LAT, LONG) VALUES ({node.ID}, \"{node.VERSION}\", \"{node.TIMESTAMP}\", \"{node.LAT}\", \"{node.LONG}\")";
                sqCommand.ExecuteNonQuery();

                // Runt trough node tags
                foreach (var tag in node.TAGS)
                {
                    sqCommand.CommandText = $"INSERT INTO TAGS (CONNECTID, KEY, VALUE) VALUES ({node.ID}, \"{tag.KEY}\", \"{tag.VALUE}\")";
                    sqCommand.ExecuteNonQuery();
                }
            }
        }

        private static void SendWays(List<Way> ways, SQLiteCommand sqCommand)
        {
            // Do nothing when no nodes
            if (ways.Count == 0)
                return;

            // Run trough all nodes
            foreach (var way in ways)
            {
                sqCommand.CommandText = $"INSERT INTO WAYS (ID, VERSION, TIMESTAMP) VALUES ({way.ID}, \"{way.VERSION}\", \"{way.TIMESTAMP}\")";
                sqCommand.ExecuteNonQuery();

                // Run trough sub lists
                foreach (var tag in way.TAGS)
                {
                    sqCommand.CommandText = $"INSERT INTO TAGS (CONNECTID, KEY, VALUE) VALUES ({way.ID}, \"{tag.KEY}\", \"{tag.VALUE}\")";
                    sqCommand.ExecuteNonQuery();
                }
                foreach (var nodeRef in way.NODEREFERENCES)
                {
                    sqCommand.CommandText = $"INSERT INTO NODEREFS (CONNECTID, REF) VALUES ({way.ID}, \"{nodeRef.ID}\")";
                    sqCommand.ExecuteNonQuery();
                }
            }
        }

        private static void SendRelations(List<Relation> relations, SQLiteCommand sqCommand)
        {
            // Do nothing when no relations
            if (relations.Count == 0)
                return;

            // Run trough all relations
            foreach (var relation in relations)
            {
                sqCommand.CommandText = $"INSERT INTO RELATIONS (ID, VERSION, TIMESTAMP) VALUES ({relation.ID}, \"{relation.VERSION}\", \"{relation.TIMESTAMP}\")";
                sqCommand.ExecuteNonQuery();

                // Run trough sub lists
                foreach (var tag in relation.TAGS)
                {
                    sqCommand.CommandText = $"INSERT INTO TAGS (CONNECTID, KEY, VALUE) VALUES ({relation.ID}, \"{tag.KEY}\", \"{tag.VALUE}\")";
                    sqCommand.ExecuteNonQuery();
                }
                foreach (var member in relation.MEMBERS)
                {
                    sqCommand.CommandText = $"INSERT INTO MEMBERS (CONNECTID, TYPE, REF, ROLE) VALUES ({relation.ID}, \"{member.TYPE}\", \"{member.REF}\", \"{member.ROLE}\")";
                    sqCommand.ExecuteNonQuery();
                }
            }
        }
    }
}
