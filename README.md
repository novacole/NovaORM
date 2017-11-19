# NovaORM
A Simple ORM written in C#. NovaORM is a thin wrapper over ADO.NET, designed to eliminate most of the boilerplate associated with ADO.net. 

Example

Lets say you have a db table and POCO class ``Person`` 
***

```C# 
class Person 
{
        [IsPrimaryKey]
        public int ID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        
} 
 ```
***
With NovaORM you could do the following:
```C# 
            var db = new NovA("ConnectionString");
            var person = db.GetList<Person>().Find(x => x.FirstName == "Henry");
            person.FirstName = "Hendrick";
            db.UpdateWithValues<Person>(person);
            person = db.GetList<Person>().Find(x => x.FirstName == "Hendrick");
//...
 ```
		
			Or
***
```C#       
            var db = new NovA("ConnectionString");
            var person = new Person() { FirstName = "Henry", LastName = "Franklin" };
            db.InsertWithValues(person);
            var people = db.GetList<Person>();
//...
```
