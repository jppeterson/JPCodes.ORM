using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPCodes.ORM
{
    public static class Extensions
    {
        private static ConcurrentDictionary<string, List<(int IDX, DataField DF)>> _mapCache = new ConcurrentDictionary<string, List<(int IDX, DataField DF)>>();
        private static List<(int IDX, DataField DF)> GetFieldCache<T>(string sql, DbDataReader dbDataReader)
        {
            DataDefinition definition = DataDefinition.FromType(typeof(T));
            if (!_mapCache.TryGetValue(sql, out List<(int index, DataField datafield)> resultFields))
            {
                List<string> fields = Enumerable.Range(0, dbDataReader.FieldCount).Select(IDX => dbDataReader.GetName(IDX).ToUpper()).ToList();

                _mapCache[sql] = resultFields = definition.Fields
                    .Select((datafield, index) =>
                    {
                        int idx = fields.IndexOf(datafield.FieldName.ToUpper());
                        if (idx >= 0)
                        {
                            return (idx, datafield);
                        }
                        return (-1, null);
                    })
                    .Where(CPL => CPL.idx >= 0)
                    .ToList();
            }
            return resultFields;
        }

        public static async Task<int> ExecuteAsync(this DbConnection dbConnection, string sql, CommandType dbCommmandType = CommandType.Text, params (string Name, object Value)[] parameters)
        {
            bool opened = dbConnection.State == ConnectionState.Closed;
            try
            {
                if (opened)
                {
                    await dbConnection.OpenAsync();
                }

                using (DbCommand command = dbConnection.CreateCommand())
                {
                    command.CommandType = dbCommmandType;
                    command.CommandText = sql;
                    foreach ((string name, object value) param in parameters)
                    {
                        DbParameter dbparam = command.CreateParameter();
                        dbparam.ParameterName = param.name;
                        dbparam.Value = param.value;
                        command.Parameters.Add(dbparam);
                    }
                    return await command.ExecuteNonQueryAsync();
                }
            }
            finally
            {
                if (opened)
                {
                    dbConnection.Close();
                }
            }
        }
        public static async Task<T> ExecuteOneAsync<T>(this DbConnection dbConnection, string sql, CommandType dbCommmandType = CommandType.Text, Func<DbDataRecord, T> mapper = null, params (string Name, object Value)[] parameters)
            where T : new()
        {
            bool opened = dbConnection.State == ConnectionState.Closed;
            try
            {
                if (opened)
                {
                    await dbConnection.OpenAsync();
                }

                using (DbCommand command = dbConnection.CreateCommand())
                {
                    command.CommandText = sql;
                    foreach ((string name, object value) in parameters)
                    {
                        DbParameter dbparam = command.CreateParameter();
                        dbparam.ParameterName = name;
                        dbparam.Value = value;
                        command.Parameters.Add(dbparam);
                    }
                    using (DbDataReader rdr = await command.ExecuteReaderAsync())
                    {
                        try
                        {
                            List<(int index, DataField datafield)> resultFields = GetFieldCache<T>(sql, rdr);

                            if (await rdr.ReadAsync())
                            {
                                T item = new T();
                                foreach ((int index, DataField datafield) in resultFields)
                                {
                                    datafield.Set(item, rdr[index]);
                                }
                                return item;
                            }
                            return default;
                        }
                        finally
                        {
                            rdr.Close();
                        }
                    }
                }
            }
            finally
            {
                if (opened)
                {
                    dbConnection.Close();
                }
            }
        }
        public static async Task<List<T>> ExecuteManyAsync<T>(this DbConnection dbConnection, string sql, CommandType dbCommmandType = CommandType.Text, Func<DbDataRecord, T> mapper = null, params (string Name, object Value)[] parameters)
            where T : new()
        {
            bool opened = dbConnection.State == ConnectionState.Closed;
            try
            {
                if (opened)
                {
                    await dbConnection.OpenAsync();
                }

                using (DbCommand command = dbConnection.CreateCommand())
                {
                    command.CommandText = sql;
                    foreach ((string name, object value) in parameters)
                    {
                        DbParameter dbparam = command.CreateParameter();
                        dbparam.ParameterName = name;
                        dbparam.Value = value;
                        command.Parameters.Add(dbparam);
                    }
                    using (DbDataReader rdr = await command.ExecuteReaderAsync())
                    {
                        try
                        {
                            List<(int index, DataField datafield)> resultFields = GetFieldCache<T>(sql, rdr);

                            List<T> results = new List<T>();
                            while (await rdr.ReadAsync())
                            {
                                T item = new T();
                                foreach (var (index, datafield) in resultFields)
                                {
                                    datafield.Set(item, rdr[index]);
                                }
                                results.Add(item);
                            }
                            return results;
                        }
                        finally
                        {
                            rdr.Close();
                        }
                    }
                }
            }
            finally
            {
                if (opened)
                {
                    dbConnection.Close();
                }
            }
        }
    }
}
