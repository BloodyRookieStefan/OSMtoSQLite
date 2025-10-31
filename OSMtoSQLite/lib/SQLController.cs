/* 
PSEUDOCODE / PLAN:
- Add XML documentation comments for the SQLController class and all its internal members.
- For each field, method and private helper method provide:
  - <summary> describing purpose.
  - <param> tags for parameters.
  - <returns> where applicable.
  - <remarks> or <exception> where helpful (e.g. file/IO or DB operations).
- Preserve existing implementation exactly; only add XML comments and keep file formatting.
- Place a short high-level pseudocode block (this comment) at the top of the file for traceability.
*/

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
    /// <summary>
    /// Provides methods to create a SQLite database file and persist OSM elements (nodes, ways, relations)
    /// into the database. Methods operate on in-memory collections and perform direct SQL INSERT commands.
    /// </summary>
    internal class SQLController
    {
        /// <summary>
        /// Template used to create the SQLite connection string. The single format parameter is the file path.
        /// </summary>
        private static string DatabaseConnectionStringTemplate = "Data Source={0};Version=3;";

        /// <summary>
        /// The current SQLite connection string created after database creation.
        /// This is set by <see cref="CreateDatabase(string)"/>.
        /// </summary>
        private static string DatabaseConnectionString;

        /// <summary>
        /// Create a new SQLite database file and initialize the schema used by this application.
        /// </summary>
        /// <param name="loc">Full path to the SQLite database file to create. If directories in the path
        /// do not exist they will be created. If a file already exists at this path it will be deleted.</param>
        /// <remarks>
        /// This method:
        /// - Ensures the directory exists.
        /// - Deletes any existing file at <paramref name="loc"/>.
        /// - Creates a new SQLite file and opens a connection to create the following tables:
        ///   NODES, WAYS, RELATIONS, TAGS, NODEREFS, MEMBERS.
        /// The method sets <see cref="DatabaseConnectionString"/> for later use by data import methods.
        /// </remarks>
        /// <exception cref="IOException">Propagates IO exceptions from file system operations (directory creation, deletion).</exception>
        /// <exception cref="SQLiteException">Propagates SQLite exceptions when creating the file or executing SQL statements.</exception>
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

        /// <summary>
        /// Persist lists of OSM elements (nodes, ways, relations) into the currently configured SQLite database.
        /// </summary>
        /// <param name="nodes">List of <see cref="Node"/> structures to insert into the NODES table.</param>
        /// <param name="ways">List of <see cref="Way"/> structures to insert into the WAYS table.</param>
        /// <param name="relations">List of <see cref="Relation"/> structures to insert into the RELATIONS table.</param>
        /// <remarks>
        /// This method opens a connection using <see cref="DatabaseConnectionString"/>, begins a local transaction,
        /// and calls helper methods to insert nodes, ways and relations along with their associated tags, node references,
        /// and relation members. The transaction is committed once all inserts complete.
        /// </remarks>
        /// <exception cref="InvalidOperationException">Thrown if <see cref="DatabaseConnectionString"/> has not been initialized.</exception>
        /// <exception cref="SQLiteException">Propagates SQLite exceptions raised during command execution.</exception>
        internal static void SendDataToSQL(List<Node> nodes, List<Way> ways, List<Relation> relations)
        {
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
        }

        /// <summary>
        /// Insert node records and their associated tags into the database using the provided command.
        /// </summary>
        /// <param name="nodes">List of nodes to insert. If empty, no operation is performed.</param>
        /// <param name="sqCommand">Active <see cref="SQLiteCommand"/> configured with an open connection and, optionally, a transaction.</param>
        /// <remarks>
        /// This method builds raw SQL INSERT statements and calls <see cref="SQLiteCommand.ExecuteNonQuery"/> for each row.
        /// Warning: current implementation uses string interpolation to build SQL; it does not parameterize values.
        /// </remarks>
        private static void SendNodes(List<Node> nodes, SQLiteCommand sqCommand)
        {
            // Do nothing when no nodes
            if (nodes.Count != 0)
            {
                // Run trough sub lists
                foreach (var node in nodes)
                {
                    sqCommand.CommandText = $"INSERT INTO NODES (ID, VERSION, TIMESTAMP,  LAT, LONG) VALUES ({node.ID}, \"{node.VERSION}\", \"{node.TIMESTAMP}\", \"{node.LAT}\", \"{node.LONG}\")";
                    sqCommand.ExecuteNonQuery();

                    // Runt trough node tags
                    foreach (var tag in node.TAGS)
                    {
                        sqCommand.CommandText = $"INSERT INTO TAGS (CONNECTID, KEY, VALUE) VALUES ({node.ID}, \"{tag.KEY}\", \"{tag.VALUE}\")";
                        sqCommand.ExecuteNonQuery();
                    }
                }
            }
        }

        /// <summary>
        /// Insert way records, their tags and node references into the database using the provided command.
        /// </summary>
        /// <param name="ways">List of ways to insert. If empty, no operation is performed.</param>
        /// <param name="sqCommand">Active <see cref="SQLiteCommand"/> configured with an open connection and, optionally, a transaction.</param>
        /// <remarks>
        /// For each way this method inserts a WAYS row, then inserts related entries into TAGS and NODEREFS.
        /// Like <see cref="SendNodes"/>, the implementation uses inline string concatenation for SQL statements.
        /// </remarks>
        private static void SendWays(List<Way> ways, SQLiteCommand sqCommand)
        {
            // Do nothing when no nodes
            if (ways.Count != 0)
            {
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
        }

        /// <summary>
        /// Insert relation records, their tags and members into the database using the provided command.
        /// </summary>
        /// <param name="relations">List of relations to insert. If empty, no operation is performed.</param>
        /// <param name="sqCommand">Active <see cref="SQLiteCommand"/> configured with an open connection and, optionally, a transaction.</param>
        /// <remarks>
        /// For each relation this method inserts a RELATIONS row, then inserts related entries into TAGS and MEMBERS.
        /// </remarks>
        private static void SendRelations(List<Relation> relations, SQLiteCommand sqCommand)
        {
            // Do nothing when no relations
            if (relations.Count != 0)
            {
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
}
