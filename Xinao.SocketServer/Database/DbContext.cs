using log4net.Repository.Hierarchy;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xinao.SocketServer.Database
{
    public class DbContext
    {
        public static readonly string ConnStr = System.Configuration.ConfigurationManager.AppSettings["database"];
        public static SqlSugarClient DbClient
        {
            get
            {
                return new SqlSugarClient(new ConnectionConfig()
                {
                    ConnectionString = ConnStr,
                    DbType = DbType.MySql,
                    IsAutoCloseConnection = true,
                    AopEvents = new AopEvents
                    {
                        OnLogExecuting = (sql, pars) =>
                        {
                            //SQL执行前事件
                        },
                        OnLogExecuted = (sql, pars) =>
                        {
                            //SQL执行完事件
                        },
                        OnError = (exp) =>
                        {
                            //执行SQL 错误事件
                        },
                    }
                });
            }
        }
    }
}
