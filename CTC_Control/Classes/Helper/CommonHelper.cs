using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTC_Control.Classes.Helper
{
    class CommonHelper
    {
        /// <summary>
        /// 本地datetime对象转换为sql语句的date值
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static string DateToOracleDate(DateTime dt)
        {
            return string.Format("to_date('{0}','yyyy-MM-dd hh24:mi:ss')", dt.ToString("yyyy-MM-dd HH:mm:ss"));
        }
    }
}
