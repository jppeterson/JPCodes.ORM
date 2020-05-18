using System;
using System.Collections.Generic;
using System.Text;

namespace JPCodes.ORM
{
    /// <summary>
    /// Set a table name to be different than the class name.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class DataRecordAttribute : Attribute
    {
        public string TableName { get; set; }
    }

    /// <summary>
    /// Indicates a group of fields are unique in the database.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class UniqueFieldGroupAttribute : Attribute
    {
        public string[] Fields { get; private set; }
    }

    /// <summary>
    /// Set a DisplayName and FieldName to be different than the property name.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class DataFieldAttribute : Attribute
    {
        public string DisplayName { get; private set; }
        public string FieldName { get; private set; }
    }

    /// <summary>
    /// Indicates a field is a primary key in the datasource.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class PrimaryKeyAttribute : DataFieldAttribute { }

    /// <summary>
    /// Indicates a field is unique in the datasource.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class UniqueFieldAttribute : DataFieldAttribute { }
}
