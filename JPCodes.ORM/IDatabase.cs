using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;

namespace JPCodes.ORM
{
    public interface IDatabase
    {
        string ParameterPrefix { get; }
        string EncloseObject(string item);
        DbParameter CreateParameter(string name, object value);
    }
}
