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
        }
    }
}