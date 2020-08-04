using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Oracle.ManagedDataAccess.Client;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Reflection;

namespace DXL_MVC.SQL
{
    public class OracleHelper
    {
        public readonly static IConfiguration Configuration;
        static OracleHelper()
        {
            //
            Configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true)
                .Build();
        }

        public static string GetConnectionString()
        {
            return Configuration.GetSection("ConnectionStrings").GetSection("DCSConnection").Value;
        }

        #region 查询符合条件的第一条数据

        /// <summary>
        /// 只查询符合条件的第一条数据

        /// </summary>
        /// <param name="array">结果数组</param>
        /// <param name="sql">sql语句</param>
        public static object[] ReturnFirstRecord(string sql)
        {

            OracleConnection conn = null;
            OracleDataReader read = null;
            object[] array;
            try
            {
                conn = new OracleConnection(GetConnectionString());
                OracleCommand cmd = new OracleCommand(sql, conn);
                conn.Open();
                read = cmd.ExecuteReader(System.Data.CommandBehavior.SingleRow);
                array = new object[read.FieldCount];
                while (read.Read())
                {
                    if (read.HasRows)
                    {
                        for (int j = 0; j < read.FieldCount; j++)
                        {
                            array[j] = read.GetValue(j);
                        }
                        break;
                    }
                }
            }
            finally
            {
                if (null != read)
                {
                    read.Close();
                    read.Dispose();
                }
                if (null != conn)
                {
                    if (ConnectionState.Closed != conn.State)
                        conn.Close();
                    conn.Dispose();
                }
            }
            if (array != null)
            {
                for (int i = 0; i < array.Length; i++)
                {
                    if (array[i] == System.DBNull.Value)
                    {
                        array[i] = null;
                    }
                }
            }
            return array;
        }
        #endregion

        #region 执行Sql语句，并返回结果集中的第一行第一列元素，忽略其他成员，用于取count,max等操作

        /// <summary>
        /// 执行Sql语句，并返回结果集中的第一行第一列元素，忽略其他成员，用于取count,max等操作

        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <returns>如果sql语句没有错误，而数据库中没有数据时，count返回0，而max返回system.DBNull.value </returns>
        public static object ReturnScarlar(string sql)
        {
            OracleConnection conn = null;
            object scarlar;
            try
            {
                conn = new OracleConnection(GetConnectionString());
                conn.Open();
                OracleCommand cmd = new OracleCommand(sql, conn);
                scarlar = cmd.ExecuteScalar();
                if (scarlar == System.DBNull.Value)
                    scarlar = null;
                return scarlar;
            }
            finally
            {
                if (null != conn)
                {
                    if (ConnectionState.Closed != conn.State)
                        conn.Close();
                    conn.Dispose();
                }
            }

        }
        #endregion

        #region 执行SQL语句,返回所影响的行数

        /// <summary>
        /// 执行SQL语句,返回所影响的行数

        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public static int ExecuteSQL(string sql)
        {
            OracleConnection conn = null;
            int count = 0;
            try
            {
                conn = new OracleConnection(GetConnectionString());
                conn.Open();
                OracleCommand command = new OracleCommand(sql, conn);
                count = command.ExecuteNonQuery();
                return count;
            }
            catch (Exception ex)
            {
                return -1;
            }
            finally
            {
                if (null != conn)
                {
                    if (ConnectionState.Closed != conn.State)
                        conn.Close();
                    conn.Dispose();
                }
            }
        }
        #endregion

        #region 根据sql语句，返回Datatable
        /// <summary>
        /// 根据sql语句，返回Datatable
        /// </summary>
        /// <param name="tablename">Datatable</param>
        /// <param name="sql">sql语句</param>
        public static DataTable ReturnDataTable(string sql)
        {
            OracleConnection conn = null;
            DataTable dt = new DataTable();
            try
            {
                conn = new OracleConnection(GetConnectionString());
                conn.Open();
                OracleDataAdapter da = new OracleDataAdapter(sql, conn);
                da.Fill(dt);
                return dt;
            }
            catch (Exception ex)
            {
                return null;
            }
            finally
            {
                if (null != conn)
                {
                    if (ConnectionState.Closed != conn.State)
                        conn.Close();
                    conn.Dispose();
                }
            }
        }
        #endregion

        public static List<T> ReturnList<T>(string sql)
        {
            List<T> result = new List<T>();
            DataTable dt = ReturnDataTable(sql);
            if (dt != null && dt.Rows.Count > 0)
            {
                Type type = typeof(T);
                PropertyInfo[] props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                foreach (DataRow dr in dt.Rows)
                {
                    object obj = Activator.CreateInstance(type);
                    foreach (PropertyInfo pi in props)
                    {
                        string name = pi.Name;
                        if (dr.Table.Columns.Contains(name))
                        {
                            if (pi.PropertyType.IsGenericType && pi.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                            {
                                pi.SetValue(obj, Convert.ChangeType(dr[name], pi.PropertyType.GetGenericArguments()[0]), null);
                            }
                            else
                            {
                                pi.SetValue(obj, Convert.ChangeType(dr[name], pi.PropertyType), null);
                            }
                        }
                    }
                    result.Add((T)obj);
                }
            }
            return result;
        }

        public static T ReturnFirstObject<T>(string sql)
        {
            DataTable dt = ReturnDataTable(sql);
            if (dt != null && dt.Rows.Count > 0)
            {
                Type type = typeof(T);
                PropertyInfo[] props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

                object obj = Activator.CreateInstance(type);
                foreach (PropertyInfo pi in props)
                {
                    string name = pi.Name;
                    if (dt.Columns.Contains(name))
                    {
                        if (pi.PropertyType.IsGenericType && pi.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                        {
                            pi.SetValue(obj, Convert.ChangeType(dt.Rows[0][name], pi.PropertyType.GetGenericArguments()[0]), null);
                        }
                        else
                        {
                            pi.SetValue(obj, Convert.ChangeType(dt.Rows[0][name], pi.PropertyType), null);
                        }
                    }
                }
                return (T)obj;
            }
            return default(T);
        }

        #region 执行一组SQL语句，如果其中有不能执行的语句，则回滚事务

        /// <summary>
        /// 执行一组SQL语句，如果其中有不能执行的语句，则回滚事务

        /// </summary>
        /// <param name="sqlList">sql语句列表</param>
        /// <returns>执行是否成功</returns>
        public bool ExecuteSQLList(List<string> sqlList)
        {
            OracleConnection conn = null;
            OracleTransaction ts = null;
            try
            {
                conn = new OracleConnection(GetConnectionString());
                conn.Open();
                OracleCommand cmd = new OracleCommand();
                ts = conn.BeginTransaction();
                int i;
                cmd.Connection = conn;
                cmd.Transaction = ts;
                for (i = 0; i < sqlList.Count; i++)
                {
                    if (sqlList[i].ToString() != "")
                    {
                        cmd.CommandText = sqlList[i].ToString();
                        cmd.ExecuteNonQuery();
                    }
                }
                ts.Commit();
                return true;
            }
            catch (Exception ex)
            {
                ts.Rollback();
                return false;
            }
            finally
            {
                if (null != conn)
                {
                    if (ConnectionState.Closed != conn.State)
                        conn.Close();
                    conn.Dispose();
                }
            }
        }
        #endregion

    }
}
