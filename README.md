# JPCodes.ORM
Object Relational Mapper using Attributes

Combine the speed of [fast-member](https://github.com/mgravell/fast-member) with the ease of C# Attributes to control SQL generation and interpretation.

Example Class
```
//Required if using SQL generation
[MySqlDataRecord]
public class User
{
    //Set a property as Key. This makes it a WHERE parameter in sql generation
    [DataField(IsKey = true)]
    public int UserID { get; set; }

    //Change the default field name
    [DataField(FieldName = "first_name")]
    public string FirstName { get; set; }

    //FieldName matches PropertyName
    public string LastName { get; set; }
}
```

Usage:
```
//Insert the model into the database
Task.Run(async () => 
{
    using (MySqlConnection connection = new MySqlConnection("ConnString"))
    {
        await connection.InsertAsync(data);
        //await connection.UpdateAsync(data);
        //await connection.DeleteAsync(data);
    }
}).Wait();

//Select a model from the database
User model = Task.Run(async () =>
{
    using (MySqlConnection connection = new MySqlConnection("ConnString"))
    {
        return await connection.ExecuteOneAsync<User>("SELECT TOP 1 * FROM dbo.[User]");
    }
}).Result;

//Select multiple models from the database
List<User> models = Task.Run(async () =>
{
    using (MySqlConnection connection = new MySqlConnection("ConnString"))
    {
        return await connection.ExecuteManyAsync<User>("SELECT * FROM dbo.[User]");
    }
}).Result;
```
