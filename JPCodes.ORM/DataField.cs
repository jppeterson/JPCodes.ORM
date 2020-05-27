using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace JPCodes.ORM
{
    public class DataField
    {
        public DataDefinition ParentDefinition { get; set; }
        public bool IsKey { get; set; }
        public Type DataType { get; set; }
        public Type SafeDataType { get; set; }
        public string FieldName { get; set; }
        public string PropertyName { get; set; }
        public string DisplayName { get; set; }

        public void Set(object instance, object value)
        {
            ParentDefinition.Accessor[instance, PropertyName] = value;
        }

        public object Get(object instance)
        {
            return ParentDefinition.Accessor[instance, PropertyName];
        }
    }
}
