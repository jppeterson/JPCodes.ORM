using Org.BouncyCastle.Bcpg;
using System;
using System.Collections.Generic;
using System.Text;

namespace JPCodes.ORM.Examples
{
    [MySqlTable("user")]
    public class User
    {
        [Field("user_id")]
        public virtual int UserID { get; set; }

        [Field("first_name")]
        public virtual string FirstName { get; set; }

        [Field("last_name")]
        public virtual string LastName { get; set; }

        [Field("created")]
        public virtual DateTime Created { get; set; }
    }

    public class UserInsert : User
    {
        [FieldIgnore]
        public override int UserID => 0;

        [Field("created")]
        public override DateTime Created => DateTime.UtcNow;
    }

    public class UserPKSelect : User
    {
        [FieldWhere("user_id")]
        public int UserIDWhere { get; set; }
    }

    public class UserFirstNameSelect : User
    {
        [FieldWhere("first_name")]
        public string FirstNameWhere { get; set; }
    }
}
