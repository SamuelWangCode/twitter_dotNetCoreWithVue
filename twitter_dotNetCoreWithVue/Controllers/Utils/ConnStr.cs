using System;
using System.IO;
namespace twitter_dotNetCoreWithVue.Controllers.Utils
{
    
    public class ConnStr
    {
        public static string line = new StreamReader("dbconf.cof").ReadLine();
        static public String getConnStr()
        {
            return line;
            return "Data Source=(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST=localhost)(PORT=1521)))(CONNECT_DATA=(SID=)));User Id=;Password=;"; // 使用自己的string

        }
    }
}