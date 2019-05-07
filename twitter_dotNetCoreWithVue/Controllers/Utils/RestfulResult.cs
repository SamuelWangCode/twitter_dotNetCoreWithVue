using System;
using System.Collections.Generic;

namespace twitter_dotNetCoreWithVue.Controllers.Utils
{
    static public class RestfulResult
    {
        public class RestfulData
        {
            public RestfulData()
            {
            }

            public RestfulData(int v1, string v2)
            {
                this.Code = v1;
                this.Message = v2;
            }

            /// <summary>
            /// <![CDATA[错误码]]>
            /// </summary>
            public int Code { get; set; }

            /// <summary>
            ///<![CDATA[消息]]>
            /// </summary>
            public string Message { get; set; }

        }

        /// <summary>
        ///
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public class RestfulData<T> : RestfulData
        {
            /// <summary>
            /// <![CDATA[数据]]>
            /// </summary>
            public virtual T Data { get; set; }
        }

        /// <summary>
        /// <![CDATA[返回数组]]>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public class RestfulArray<T> : RestfulData<IEnumerable<T>>
        {

        }
    }
}
