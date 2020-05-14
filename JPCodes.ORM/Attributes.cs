using System;
using System.Collections.Generic;
using System.Text;

namespace JPCodes.ORM
{
    [AttributeUsage(AttributeTargets.Class)]
    public class DataRecordAttribute : Attribute
    {
        public string TableName { get; set; }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class UniqueFieldGroupAttribute : Attribute
    {
        public string[] Fields { get; private set; }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class DataFieldAttribute : Attribute
    {
        public string DisplayName { get; private set; }
        public string FieldName { get; private set; }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class PrimaryKeyAttribute : DataFieldAttribute { }

    [AttributeUsage(AttributeTargets.Property)]
    public class UniqueFieldAttribute : DataFieldAttribute { }
}
