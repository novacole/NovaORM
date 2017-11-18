using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Reflection;
using System.Dynamic;
using System.Data;
using System.Linq.Expressions;

namespace NovaORM
{
    public class NovA
    {
        public class IsPrimaryKey : Attribute { }
        #region Private Methods and Fields
        private string ConnectionString { get; set; }
        private string Predicate { get; set; }
        private SqlConnection GetConnection()
        {
            return new SqlConnection(this.ConnectionString);
        }
        private string GetPropertiesAsString<T>(bool ignorePK, bool? parameterize = false)
        {
            T obj = (T)Activator.CreateInstance(typeof(T));
            string listOfProperties = string.Empty;
            if (ignorePK)
                listOfProperties = string.Join(",", obj.GetType().GetProperties().ToList().Where(x => x.GetCustomAttributes(typeof(IsPrimaryKey), false).Count() == 0).Select(x => ((bool)parameterize ? "@" : "") + x.Name));
            else
                listOfProperties = string.Join(",", obj.GetType().GetProperties().ToList().Select(x => ((bool)parameterize ? "@" : "") + x.Name));
            return listOfProperties;
        }

        private string GetPropertiesAsString(IDictionary<string, object> tableObject, bool? parameterize = false)
        {
            string listOfProperties = string.Empty;
            listOfProperties = string.Join(",", tableObject.Keys.Select(x => ((bool)parameterize ? "@" : "") + x));
            return listOfProperties;
        }
        private string GetPropertiesAsStringForUpdate<T>(bool ignorePK, bool? parameterize = false)
        {
            T obj = (T)Activator.CreateInstance(typeof(T));
            string listOfProperties = string.Empty;
            if (ignorePK)
                listOfProperties = string.Join(",", obj.GetType().GetProperties().ToList().Where(x => x.GetCustomAttributes(typeof(IsPrimaryKey), false).Count() == 0).Select(x => x.Name + "=" + ((bool)parameterize ? "@" + x.Name : (x.GetValue(obj, null) == null ? "null" : (x.GetValue(obj, null).ToString())))));
            else
                listOfProperties = string.Join(",", obj.GetType().GetProperties().ToList().Select(x => x.Name + "=" + ((bool)parameterize ? "@" + x.Name : (x.GetValue(obj, null) == null ? "null" : (x.GetValue(obj, null).ToString())))));
            return listOfProperties;
        }
        private string GetPropertiesAsStringForUpdate(IDictionary<string, object> tableObject, bool? parameterize = false)
        {
            string listOfProperties = string.Empty;
            listOfProperties = string.Join(",", tableObject.Select(x => x.Key + "=" + ((bool)parameterize ? "@" + x.Key : ((x.Value == null ? (!(x.Value is string) ? "null" : "") : x.Value.ToString())))));
            return listOfProperties;
        }
        private string GetValuesAsString<T>(T obj, bool? ignorePK = false)
        {
            return string.Join("^", obj.GetType().GetProperties().Where(x => ignorePK.HasValue && ignorePK.Value == true ? x.GetCustomAttributes(typeof(IsPrimaryKey), false).Count() == 0 : x.GetCustomAttributes(typeof(IsPrimaryKey), false).Count() >= 0).Select(x => (x.GetValue(obj, null) == null ? (!(x.GetType().Equals(typeof(string))) ? "null" : "") : (x.GetValue(obj, null).ToString()))).ToList());
        }
        private List<object> GetValuesAsList<T>(T obj, bool? ignorePK = false)
        {
            return obj.GetType().GetProperties().Where(x => ignorePK.HasValue && ignorePK.Value == true ? x.GetCustomAttributes(typeof(IsPrimaryKey), false).Count() == 0 : x.GetCustomAttributes(typeof(IsPrimaryKey), false).Count() >= 0).Select(x => (x.GetValue(obj, null) == null ? (!(x.GetType().Equals(typeof(string))) ? null : "") : (x.GetValue(obj, null)))).ToList();
        }
        private List<object> GetValuesAsList(IDictionary<string, object> tableObject)
        {
            return tableObject.Values.Select(x => ((x == null ? (!(x is string) ? null : "") : x))).ToList();
        }
        private string GetValuesAsString(IDictionary<string, object> tableObject)
        {
            return string.Join("^", tableObject.Values.Select(x => ((x == null ? (!(x is string) ? "null" : "") : x.ToString()))));
        }

        #endregion

        #region Public Methods
        #region Constructors
        public NovA(string connectionString)
        {
            this.ConnectionString = connectionString;
        }

        public NovA() { }
        public string GetValuesAsStringForCSV<T>(T obj)
        {
            return string.Join("^", obj.GetType().GetProperties().Select(x => (x.GetValue(obj, null) == null ? "" : x.GetValue(obj, null).ToString())).ToList());
        }
        public string GetValuesAsStringForCSV(ICollection<object> obj)
        {
            return string.Join("^", obj.Select(x => x == null ? "" : x.ToString()).ToList());
        }
        private Dictionary<Type, SqlDbType> TypeMap
        {
            get
            {
                return new Dictionary<Type, SqlDbType> {
            { typeof(string), SqlDbType.NVarChar },
            { typeof(int), SqlDbType.Int },
            { typeof(double),SqlDbType.Float },
            { typeof(DateTime),SqlDbType.DateTime },
            { typeof(char),SqlDbType.NChar },
            { typeof(bool),SqlDbType.Bit },
            { typeof(decimal),SqlDbType.Money },

            };
            }
        }
        private SqlDbType GetDBType(object obj)
        {
            return obj != null ? TypeMap.Where(x => x.Key == obj.GetType()).SingleOrDefault().Value : SqlDbType.NVarChar;
        }

        #endregion
        public List<T> GetList<T>(string sql)
        {
            List<T> items = new List<T>();
            using (SqlConnection con = GetConnection())
            {
                if (con.State != System.Data.ConnectionState.Open)
                {
                    con.Open();
                }
                using (SqlCommand com = new SqlCommand(sql, con))
                {

                    SqlDataReader reader = com.ExecuteReader();
                    while (reader.Read())
                    {
                        T item = (T)Activator.CreateInstance(typeof(T));
                        foreach (PropertyInfo prop in item.GetType().GetProperties())
                        {
                            prop.SetValue(item, DBNull.Value.Equals(reader[prop.Name]) ? null : reader[prop.Name], null);
                        }
                        items.Add(item);
                    }
                }
            }
            return items;
        }
        public List<T> GetList<T>()
        {
            List<T> items = new List<T>();
            using (SqlConnection con = GetConnection())
            {
                if (con.State != System.Data.ConnectionState.Open)
                {
                    con.Open();
                }
                StringBuilder builder = new StringBuilder();
                builder.Append("select * from " + typeof(T).Name);
                if (!string.IsNullOrEmpty(Predicate))
                {
                    builder.Append(" where " + Predicate);
                }
                using (SqlCommand com = new SqlCommand(builder.ToString(), con))
                {
                    SqlDataReader reader = com.ExecuteReader();
                    while (reader.Read())
                    {
                        T item = (T)Activator.CreateInstance(typeof(T));
                        foreach (PropertyInfo prop in item.GetType().GetProperties())
                        {
                            prop.SetValue(item, DBNull.Value.Equals(reader[prop.Name]) ? null : reader[prop.Name], null);
                        }
                        items.Add(item);
                    }
                }
            }
            if (!string.IsNullOrEmpty(Predicate))
            {
                Predicate = string.Empty;
            }
            return items;
        }
        public NovA Where<T>(Expression<Predicate<T>> predicate)
        {
            Dictionary<string, string> operandAndOperatorMap = new Dictionary<string, string>()
                {{ "==","=" },
                { "!=","<>" },
                { "AndAlso","AND" },
                { "OrElse","OR" },
                { "\"","'" },
                { "False","0" },
                { "True","1" }};
            var parseDict = new Dictionary<string, string>();
            var body = predicate.Body.ToString().Replace(predicate.Parameters[0].Name + ".", typeof(T).Name + ".").Replace(")", " )").Replace("Convert", "");
            this.Predicate = string.Join(" ", body.Split(' ').ToList().Select(x => operandAndOperatorMap.Where(y => y.Key == x).SingleOrDefault().Value != null ? operandAndOperatorMap.Where(y => y.Key == x).SingleOrDefault().Value : x.Replace("\"", "'")));
            return this;
        }
        public object GetConvertType(string value)
        {
            DateTime possibleDateTime = DateTime.Now;
            if (DateTime.TryParse(value, out possibleDateTime))
                return "'" + value + "'";
            else
                return value;
        }
        public List<IDictionary<string, object>> GetListFromQuery(string sql)
        {
            List<IDictionary<string, object>> items = new List<IDictionary<string, object>>();
            using (SqlConnection con = GetConnection())
            {
                if (con.State != System.Data.ConnectionState.Open)
                {
                    con.Open();
                }
                using (SqlCommand com = new SqlCommand(sql, con))
                {
                    SqlDataReader reader = com.ExecuteReader();
                    while (reader.Read())
                    {
                        var item = (new ExpandoObject() as IDictionary<string, object>);
                        for (int i = 0; i <= reader.FieldCount - 1; i++)
                        {
                            item.Add(reader.GetName(i), DBNull.Value.Equals(reader.GetValue(i)) ? null : reader.GetValue(i));
                        }
                        items.Add(item);
                    }
                }
            }
            return items;
        }
        public void UpdateWithValues<T>(T tableObject)
        {
            StringBuilder builder = new StringBuilder();
            using (SqlConnection con = GetConnection())
            {
                if (con.State != System.Data.ConnectionState.Open)
                {
                    con.Open();
                }
                builder.Append("update " + tableObject.GetType().Name);
                builder.Append(" set ");
                builder.Append(GetPropertiesAsStringForUpdate<T>(true, true));
                if (tableObject.GetType().GetProperties().ToList().Where(x => x.GetCustomAttributes(typeof(IsPrimaryKey), false).Count() > 0).FirstOrDefault() != null)
                {
                    builder.Append(" where ");
                    var primaryKey = tableObject.GetType().GetProperties().ToList().Where(x => x.GetCustomAttributes(typeof(IsPrimaryKey), false).Count() > 0).FirstOrDefault();
                    builder.Append(primaryKey.Name + " = " + (primaryKey.GetValue(tableObject, null).ToString()));
                }

                using (SqlCommand com = new SqlCommand(builder.ToString(), con))
                {
                    var props = GetPropertiesAsString<T>(ignorePK: true, parameterize: true).Split(',');
                    var vals = GetValuesAsList<T>(tableObject, ignorePK: true);
                    int i = 0;
                    props.ToList().ForEach(x =>
                    {
                        com.Parameters.Add(new SqlParameter() { ParameterName = x, Value = vals[i] == null ? DBNull.Value : vals[i], SqlDbType = GetDBType(vals[i++]) });
                    });
                    com.ExecuteNonQuery();
                }
            }
        }
        public void DeleteFromWhere(string table, string predicate)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("delete from ");
            builder.Append(table);
            if (!string.IsNullOrEmpty(predicate))
            {
                builder.Append(" where ");
                builder.Append(predicate);
            }
            using (SqlConnection con = GetConnection())
            {
                if (con.State != System.Data.ConnectionState.Open)
                {
                    con.Open();
                }
                using (SqlCommand com = new SqlCommand(builder.ToString(), con))
                {
                    com.ExecuteNonQuery();
                }
            }
        }
        public void UpdateWithValues(IDictionary<string, object> tableObject, string tableName, string predicate)
        {
            StringBuilder builder = new StringBuilder();
            using (SqlConnection con = GetConnection())
            {
                if (con.State != System.Data.ConnectionState.Open)
                {
                    con.Open();
                }
                builder.Append("update " + tableName);
                builder.Append(" set ");
                builder.Append(GetPropertiesAsStringForUpdate(tableObject, true));
                if (!string.IsNullOrEmpty(predicate))
                {
                    builder.Append(" where ");
                    builder.Append(predicate);
                }

                using (SqlCommand com = new SqlCommand(builder.ToString(), con))
                {
                    var props = GetPropertiesAsString(tableObject).Split(',');
                    var vals = GetValuesAsList(tableObject);
                    int i = 0;
                    props.ToList().ForEach(x =>
                    {
                        com.Parameters.Add(new SqlParameter() { ParameterName = x, Value = vals[i] == null ? DBNull.Value : vals[i], SqlDbType = GetDBType(vals[i++]) });
                    });
                    com.ExecuteNonQuery();
                }
            }
        }
        public void InsertWithValues<T>(T tableObject)
        {
            StringBuilder builder = new StringBuilder();
            using (SqlConnection con = GetConnection())
            {
                if (con.State != System.Data.ConnectionState.Open)
                {
                    con.Open();
                }
                // assumes class name is table name...
                builder.Append("insert into " + tableObject.GetType().Name);
                builder.Append("(");
                builder.Append(GetPropertiesAsString<T>(ignorePK: true));
                builder.Append(")");
                builder.Append(" values ");
                builder.Append("(");
                builder.Append(GetPropertiesAsString<T>(ignorePK: true, parameterize: true));
                builder.Append(")");
                using (SqlCommand com = new SqlCommand(builder.ToString(), con))
                {
                    var props = GetPropertiesAsString<T>(ignorePK: true, parameterize: true).Split(',');
                    var vals = GetValuesAsList<T>(tableObject, ignorePK: true);
                    int i = 0;
                    props.ToList().ForEach(x =>
                    {
                        com.Parameters.Add(new SqlParameter() { ParameterName = x, Value = vals[i] == null ? DBNull.Value : vals[i], SqlDbType = GetDBType(vals[i++]) });
                    });

                    com.ExecuteNonQuery();
                }
            }
        }
        public void InsertWithValues(IDictionary<string, object> tableObject, string tableName)
        {
            StringBuilder builder = new StringBuilder();
            using (SqlConnection con = GetConnection())
            {
                if (con.State != System.Data.ConnectionState.Open)
                {
                    con.Open();
                }
                builder.Append("insert into " + tableName);
                builder.Append("(");
                builder.Append(GetPropertiesAsString(tableObject));
                builder.Append(")");
                builder.Append(" values ");
                builder.Append("(");
                builder.Append(GetValuesAsString(tableObject));
                builder.Append(")");
                using (SqlCommand com = new SqlCommand(builder.ToString(), con))
                {
                    var props = GetPropertiesAsString(tableObject, parameterize: true).Split(',');
                    var vals = GetValuesAsList(tableObject);
                    int i = 0;
                    props.ToList().ForEach(x =>
                    {
                        com.Parameters.Add(new SqlParameter() { ParameterName = x, Value = vals[i] == null ? DBNull.Value : vals[i], SqlDbType = GetDBType(vals[i++]) });
                    });
                    com.ExecuteNonQuery();
                }
            }
        }
        #endregion

    }
}
