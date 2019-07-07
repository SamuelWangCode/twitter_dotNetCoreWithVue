using System;
namespace twitter_dotNetCoreWithVue.Controllers.Utils
{
    public class ConnStr
    {
        static public String getConnStr()
        {
            return "Data Source=(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST=localhost)(PORT=1521)))(CONNECT_DATA=(SERVER=DEDICATED)(SERVICE_NAME=TwitterProject)));User Id=twitter;Password=19981229;"; // 使用自己的string
        }
    }
}
