using System;
using System.Text.RegularExpressions;
namespace twitter_dotNetCoreWithVue.Controllers.Utils
{
    static public class ParameterChecker
    {

        public enum ParaTpye
        {
            Email, Password, StringNoNull, String, Number, Gender
        };
        static public bool CheckPara(object para, ParaTpye para_type, int min_len, int max_len)
        {
            string para_str = para.ToString();
            int len = System.Text.Encoding.Default.GetByteCount(para_str);
            if (len < min_len || len > max_len)
            {
                return false;
            }
            return Regex.IsMatch(para_str, reg_str[(int)para_type]);
        }
        static public bool CheckPara(object para, ParaTpye para_type, int len)
        {
            return CheckPara(para, para_type, 0, len);
        }
        static public bool CheckPara(object para, ParaTpye para_type)
        {
            return CheckPara(para, para_type, 0, 65535);
        }
        static private string[] reg_str = { "^[a-zA-Z0-9_-]+@[a-zA-Z0-9_-]+(\\.[a-zA-Z0-9_-]+)+$",
        "^[0-9a-zA-Z_#@!\\?\\-\\\\]{6,20}$",".+",".*","^[0-9]{1,38}$","^Female$|^Male$|^ÄÐ$|^Å®$"};

    }
}