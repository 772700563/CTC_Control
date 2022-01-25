using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Oracle.ManagedDataAccess.Client;

namespace CTCDB
{
    public class DBHELPER
    {
        /// <summary>
        /// 连接oracle，并返回连接对象
        /// </summary>
        /// <param name="host">服务器地址</param>
        /// <param name="port">端口号</param>
        /// <param name="serviceName">数据库对外的名称</param>
        /// <param name="id">用户名</param>
        /// <param name="passwd">密码</param>
        /// <returns></returns>
        public static OracleConnection OpenConn(string host, string port, string serviceName, string id, string passwd)
        {
            OracleConnection conn = new OracleConnection();
            conn.ConnectionString = string.Format("User Id={0};Password={1};" +
                "Data Source=(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST={2})(PORT={3})))(CONNECT_DATA=(SERVICE_NAME={4})))",
                id, passwd, host, port, serviceName);
            try
            {
                conn.Open();
            }
            catch { }
            return conn;
        }

        /// <summary>
        /// 关闭连接
        /// </summary>
        /// <param name="conn"></param>
        public static void CloseConn(OracleConnection conn)
        {
            conn.Close();
        }

        /// <summary>
        /// 执行SQL语句并返回DataTable
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static DataTable ExcuteDataTable(OracleConnection conn, string sql, params OracleParameter[] parameters)
        {
            if (conn.State != ConnectionState.Open)
            {
                conn.Open();
            }
            try
            {
                using (OracleCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = sql;
                    cmd.Parameters.AddRange(parameters);
                    OracleDataAdapter adapter = new OracleDataAdapter(cmd);
                    DataTable dataTable = new DataTable();
                    adapter.Fill(dataTable);
                    return dataTable;
                }
            }
            catch (Exception e)
            {
                return new DataTable();
            }
        }

        /// <summary>
        /// 获取数据表
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="user_name"></param>
        /// <param name="table_name"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static DataTable GetDBTable(OracleConnection conn, string user_name, string table_name, params OracleParameter[] parameters)
        {
            string sqlstr = string.Format("SELECT * FROM {0}.\"{1}\"", user_name, table_name);
            DataTable dataTable = ExcuteDataTable(conn, sqlstr);
            return dataTable;
        }
    }
}
