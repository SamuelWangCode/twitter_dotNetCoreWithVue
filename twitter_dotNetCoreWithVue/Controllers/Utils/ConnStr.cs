using System;
namespace twitter_dotNetCoreWithVue.Controllers.Utils
{
    public class ConnStr
    {
        static public String getConnStr()
        {

            return "Data Source=(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST=localhost)(PORT=1521)))(CONNECT_DATA=(SID=ORCL)));User Id=C##PLUMLOPEZ;Password=BwnZed62;"; // 使用自己的string
            
        }
    }
}
