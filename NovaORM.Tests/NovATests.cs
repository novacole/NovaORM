using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NovaORM.Tests.Models;
using System.Collections.Generic;

namespace NovaORM.Tests
{
    [TestClass]
    public class NovATests
    {
        
        [TestMethod]
        public void GetListFromQueryTest()
        {
            //Arrange
            var db = new NovA("Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename='|DataDirectory|\\TestDB.mdf';Integrated Security=True;Connect Timeout=30");
            //Act
            var people = db.GetListFromQuery("select * from Person");
            //Assert
            Assert.AreEqual(people.Count, 10);
        }

        [TestMethod]
        public void GetListTest()
        {
            //Arrange
            var db = new NovA("Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename='|DataDirectory|\\TestDB.mdf';Integrated Security=True;Connect Timeout=30");
            //Act
            var people = db.GetList<Person>();
            //Assert
            Assert.AreEqual(people.Count, 10);
        }
        [TestMethod]
        public void InsertTest()
        {
            //Arrange
            var db = new NovA("Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename='|DataDirectory|\\TestDB.mdf';Integrated Security=True;Connect Timeout=30");
            //Act
            var person = new Person() { FirstName = "Henry", LastName = "Franklin" };
            db.InsertWithValues(person);
            var people = db.GetList<Person>();
            //Assert
            Assert.AreEqual(people.Count, 11);
        }
        [TestMethod]
        public void UpdateTest()
        {
            //Arrange
            var db = new NovA("Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename='|DataDirectory|\\TestDB.mdf';Integrated Security=True;Connect Timeout=30");
            //Act
            var person = db.GetList<Person>().Find(x => x.FirstName == "Henry");
            person.FirstName = "Hendrick";
            db.UpdateWithValues<Person>(person);
            person = db.GetList<Person>().Find(x => x.FirstName == "Hendrick");
            
            //Assert
            Assert.AreEqual(person.FirstName, "Hendrick");
        }
        [TestMethod]
        public void DeleteTest()
        {
            //Arrange
            var db = new NovA("Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename='|DataDirectory|\\TestDB.mdf';Integrated Security=True;Connect Timeout=30");
            //Act
            db.DeleteFromWhere("Person", "LastName ='Franklin'");
            var people = db.GetList<Person>();
            //Assert
            Assert.AreEqual(people.Count, 10);
        }
       
    }
}
