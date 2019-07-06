using System;
namespace twitter_dotNetCoreWithVue.Controllers.Utils
{
    public class ConnStr
    {
        static public String getConnStr()
        {
            return "Data Source=(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST=localhost)(PORT=1521)))(CONNECT_DATA=(SID=NewDB)));User Id=yzc;Password=zhjt9910;"; // 使用自己的string
        }
    }
}
