using FastMember;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace JPCodes.ORM
{
    public class DataDefinition
    {
        #region Instance Members
        public string TableName { get; private set; }
        public Type Type { get; private set; }
        public TypeAccessor Accessor { get; private set; }
        public List<DataField> Fields { get; private set; } = new List<DataField>();
        public IEnumerable<DataField> GetFields(IEnumerable<string> fields)
        {
            return Fields.Where(FLD => fields.Any(STR => STR.Equals(FLD.FieldName, StringComparison.OrdinalIgnoreCase)));
        }
        #endregion

        #region Static Accessors
        private static ConcurrentDictionary<Type, DataDefinition> _definitions = new ConcurrentDictionary<Type, DataDefinition>();
        public static DataDefinition FromType(Type t)
        {
            if (_definitions.TryGetValue(t, out DataDefinition temp))
            {
                return temp;
            }
            else
            {
                DataRecordAttribute drAttribute = t.GetCustomAttribute<DataRecordAttribute>();

                DataDefinition def = new DataDefinition 
                {
                    TableName = drAttribute?.TableName ?? t.Name,
                    Type = t,
                    Accessor = TypeAccessor.Create(t)
                };

                foreach (PropertyInfo info in t.GetProperties())
                {
                    DataFieldAttribute dfAttribute = info.GetCustomAttribute<DataFieldAttribute>();
                    def.Fields.Add(new DataField
                    {
                        ParentDefinition = def,
                        FieldName = dfAttribute?.FieldName ?? info.Name,
                        PropertyName = info.Name,
                        DisplayName = dfAttribute?.DisplayName ?? info.Name,
                        DataType = info.PropertyType,
                        SafeDataType = info.PropertyType.IsConstructedGenericType 
                            ? Nullable.GetUnderlyingType(info.PropertyType) 
                            : info.PropertyType,
                        IsKey = dfAttribute.IsKey,
                    });
                }

                _definitions[t] = def;

                return def;
            }
        }
        public static DataDefinition FromType(string typeName)
        {
            return _definitions.FirstOrDefault(DEF => DEF.Key.Name.Equals(typeName, StringComparison.OrdinalIgnoreCase)).Value;
        }
        #endregion
    }
}
