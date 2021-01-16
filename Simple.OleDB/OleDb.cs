using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.OleDb;
using System.Linq;

namespace Simple.OleDB
{
    /// <summary>
    /// Represents an OleDB provider
    /// </summary>
    public class OleDb
    {
        public string ConnectionString { get; }
        /// <summary>
        /// Creates a new instance
        /// </summary>
        public OleDb(string Provider, string FilePath, string Password)
        {
            DbConnectionStringBuilder builder = new DbConnectionStringBuilder();
            builder.Add("Provider", Provider);
            builder.Add("Data Source", FilePath);
            if (Password != null)
            {
                builder.Add("Database Password", Password);
            }
            ConnectionString = builder.ConnectionString;
        }
        /// <summary>
        /// Creates a new instance
        /// </summary>
        public OleDb(string FilePath, string Password = null)
            : this("Microsoft.ACE.OLEDB.12.0", FilePath, Password)
        { }

        private OleDbConnection getConnection()
        {
            var cnn = new OleDbConnection(ConnectionString);
            cnn.Open();

            return cnn;
        }
        /// <summary>
        /// Gets all table names
        /// </summary>
        /// <returns></returns>
        public IEnumerable< string> GetAllTables()
        {
            using var connection = getConnection();
            string[] restrictions = new string[4];
            restrictions[3] = "Table";

            var userTables = connection.GetSchema("Tables", restrictions);

            for (int i = 0; i < userTables.Rows.Count; i++)
            {
                yield return userTables.Rows[i][2].ToString();
            }
        }
        /// <summary>
        /// Gets schema for the table
        /// </summary>
        public DataTable GetTableSchema(string TableName)
        {
            using var cnn = getConnection();
            using var cmd = cnn.CreateCommand();

            cmd.CommandText = $"SELECT * FROM {TableName} LIMIT 0";

            var reader = cmd.ExecuteReader();

            return reader.GetSchemaTable();
        }
        /// <summary>
        /// Executes an SQL statement and returns the number of rows affected. 
        /// </summary>
        public int ExecuteNonQuery(string Text, object Parameters = null)
        {
            using var cnn = getConnection();
            using var cmd = cnn.CreateCommand();

            cmd.CommandText = Text;
            fillParameters(cmd, Parameters);

            return cmd.ExecuteNonQuery();
        }
        /// <summary>
        /// Executes the query, and returns the first column of the first row in the result set returned by the query
        /// </summary>
        public T ExecuteScalar<T>(string Text, object Parameters)
        {
            using var cnn = getConnection();
            using var cmd = cnn.CreateCommand();

            cmd.CommandText = Text;
            fillParameters(cmd, Parameters);

            var obj = cmd.ExecuteScalar();

            return (T)Convert.ChangeType(obj, typeof(T));
        }
        /// <summary>
        /// Executes the query, and returns a data table with results
        /// </summary>
        public DataTable ExecuteReader(string Text, object Parameters)
        {
            using var cnn = getConnection();
            using var cmd = cnn.CreateCommand();

            cmd.CommandText = Text;
            fillParameters(cmd, Parameters);

            DataTable dt = new DataTable();
            var da = new OleDbDataAdapter(cmd.CommandText, cnn);
            da.Fill(dt);
            return dt;
        }

        private void fillParameters(OleDbCommand cmd, object Parameters)
        {
            if (Parameters == null) return;
            foreach (var p in Parameters.GetType().GetProperties())
            {
                cmd.Parameters.AddWithValue(p.Name, p.GetValue(Parameters));
            }
        }
        /// <summary>
        /// Executes the query and maps the results to an new T
        /// </summary>
        public IEnumerable<T> GetReader<T>(string Text, object Parameters)
        {
            using var cnn = getConnection();
            using var cmd = cnn.CreateCommand();

            cmd.CommandText = Text;
            fillParameters(cmd, Parameters);

            using var reader = cmd.ExecuteReader();

            if (!reader.HasRows) yield break;
            var columns = reader.GetSchemaTable().Rows
                                .Cast<DataRow>()
                                .Select(r => (string)r["ColumnName"])
                                .ToArray();

            while (reader.Read())
            {
                // build new
                object t = Activator.CreateInstance<T>();

                foreach (var p in typeof(T).GetProperties())
                {
                    if (!columns.Contains(p.Name)) continue;

                    if (reader.IsDBNull(p.Name))
                    {
                        p.SetValue(t, null);
                        continue;
                    }
                    if (p.PropertyType == typeof(Guid))
                    {
                        p.SetValue(t, new Guid(reader.GetString(p.Name)));
                        continue;
                    }

                    var objReaderVal = reader.GetValue(p.Name);
                    p.SetValue(t, Convert.ChangeType(objReaderVal, p.PropertyType));
                }
                yield return (T)t;
            }
        }
        /// <summary>
        /// Get all values on the table mapped to T
        /// </summary>
        public IEnumerable<T> GetAll<T>(string TableName)
        {
            using var cnn = getConnection();
            using var cmd = cnn.CreateCommand();

            cmd.CommandText = $"SELECT * FROM {TableName}";

            using var reader = cmd.ExecuteReader();

            if (!reader.HasRows) yield break;
            var columns = reader.GetSchemaTable().Rows
                                .Cast<DataRow>()
                                .Select(r => (string)r["ColumnName"])
                                .ToArray();

            while (reader.Read())
            {
                // build new
                object t = Activator.CreateInstance<T>();

                foreach (var p in typeof(T).GetProperties())
                {
                    if (!columns.Contains(p.Name)) continue;

                    if (reader.IsDBNull(p.Name))
                    {
                        p.SetValue(t, null);
                        continue;
                    }

                    var objReaderVal = reader.GetValue(p.Name);
                    p.SetValue(t, Convert.ChangeType(objReaderVal, p.PropertyType));
                }
                yield return (T)t;
            }
        }
    }
}
