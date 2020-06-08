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
        private static List<DbParameter> GetParameters<T>(T item)
        {
            return DataDefinition.FromType(typeof(T)).GenerateParameters(item);
        }

        public static async Task<int> ExecuteAsync(this DbConnection dbConnection, string sql, CommandType dbCommmandType = CommandType.Text, params DbParameter[] parameters)
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
                    command.CommandType = dbCommmandType;
                    command.Parameters.AddRange(parameters);
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

        public static async Task<TOut> ExecuteValueAsync<TOut>(this DbConnection dbConnection, string sql, CommandType dbCommmandType = CommandType.Text, params DbParameter[] parameters)
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
                    command.CommandType = dbCommmandType;
                    command.Parameters.AddRange(parameters);
                    return (TOut)await command.ExecuteScalarAsync();
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

        public static async Task<TOut> ExecuteOneAsync<TOut>(this DbConnection dbConnection, string sql, CommandType dbCommmandType = CommandType.Text, params DbParameter[] parameters)
            where TOut : new()
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
                    command.CommandType = dbCommmandType;
                    command.Parameters.AddRange(parameters);
                    using (DbDataReader rdr = await command.ExecuteReaderAsync())
                    {
                        try
                        {
                            List<(int index, DataField datafield)> resultFields = GetFieldCache<TOut>(sql, rdr);

                            if (await rdr.ReadAsync())
                            {
                                TOut item = new TOut();
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

        public static async Task<List<TOut>> ExecuteManyAsync<TOut>(this DbConnection dbConnection, string sql, CommandType dbCommmandType = CommandType.Text, params DbParameter[] parameters)
            where TOut : new()
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
                    command.CommandType = dbCommmandType;
                    command.Parameters.AddRange(parameters);

                    using (DbDataReader rdr = await command.ExecuteReaderAsync())
                    {
                        try
                        {
                            List<(int index, DataField datafield)> resultFields = GetFieldCache<TOut>(sql, rdr);

                            List<TOut> results = new List<TOut>();
                            while (await rdr.ReadAsync())
                            {
                                TOut item = new TOut();
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

        public static async Task<TOut> ExecuteOneAsync<TIn, TOut>(this DbConnection dbConnection, string sql, TIn input, CommandType dbCommmandType = CommandType.Text)
            where TOut : new()
        {
            return await dbConnection.ExecuteOneAsync<TOut>(sql, dbCommmandType, GetParameters(input).ToArray());
        }

        public static async Task<List<TOut>> ExecuteManyAsync<TIn, TOut>(this DbConnection dbConnection, string sql, TIn input, CommandType dbCommmandType = CommandType.Text)
            where TOut : new()
        {
            return await dbConnection.ExecuteManyAsync<TOut>(sql, dbCommmandType, GetParameters(input).ToArray());
        }

        public static async Task<TIn> SelectOneAsync<TIn>(this DbConnection dbConnection, TIn input)
            where TIn : new()
        {
            DataDefinition def = DataDefinition.FromType(input.GetType());
            return await dbConnection.ExecuteOneAsync<TIn>(def.GenerateSelectSQL(), CommandType.Text, GetParameters(input).ToArray());
        }

        public static async Task<List<TIn>> SelectManyAsync<TIn>(this DbConnection dbConnection, TIn input)
            where TIn : new()
        {
            DataDefinition def = DataDefinition.FromType(input.GetType());
            return await dbConnection.ExecuteManyAsync<TIn>(def.GenerateSelectSQL(), CommandType.Text, GetParameters(input).ToArray());
        }

        public static async Task<TOut> SelectOneAsync<TIn, TOut>(this DbConnection dbConnection, TIn input)
            where TOut : new()
        {
            DataDefinition def = DataDefinition.FromType(input.GetType());
            return await dbConnection.ExecuteOneAsync<TOut>(def.GenerateSelectSQL(), CommandType.Text, GetParameters(input).ToArray());
        }

        public static async Task<List<TOut>> SelectManyAsync<TIn, TOut>(this DbConnection dbConnection, TIn input)
            where TOut : new()
        {
            DataDefinition def = DataDefinition.FromType(input.GetType());
            return await dbConnection.ExecuteManyAsync<TOut>(def.GenerateSelectSQL(), CommandType.Text, GetParameters(input).ToArray());
        }

        public static async Task<int> InsertAsync<Tin>(this DbConnection dbConnection, Tin item, string sql = null, CommandType dbCommandType = CommandType.Text)
        {
            DataDefinition def = DataDefinition.FromType(typeof(Tin));
            return await dbConnection.ExecuteAsync(sql ?? def.GenerateInsertSQL(), dbCommandType, def.GenerateParameters(item).ToArray());
        }

        public static async Task<int> UpdateAsync<Tin>(this DbConnection dbConnection, Tin item, string sql = null, CommandType dbCommandType = CommandType.Text)
        {
            DataDefinition def = DataDefinition.FromType(typeof(Tin));
            return await dbConnection.ExecuteAsync(sql ?? def.GenerateUpdateSQL(), dbCommandType, def.GenerateParameters(item).ToArray());
        }

        public static async Task<int> DeleteAsync<Tin>(this DbConnection dbConnection, Tin item, string sql = null, CommandType dbCommandType = CommandType.Text)
        {
            DataDefinition def = DataDefinition.FromType(typeof(Tin));
            return await dbConnection.ExecuteAsync(sql ?? def.GenerateDeleteSQL(), dbCommandType, def.GenerateParameters(item).ToArray());
        }
    }
}
