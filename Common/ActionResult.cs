using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class ActionResult
    {
        public bool IsSuccess { get; set; }
        public string EnglishMessage { get; set; }
        public int ResultCode { get; set; }
        public string ResultMessage { get; set; }
        public object ResultData { get; set; }

    }
}
