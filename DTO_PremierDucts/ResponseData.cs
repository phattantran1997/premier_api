using System;
using DTO_PremierDucts;

namespace DTO_PremierDucts
{
    public class ResponseData
    {
        public ERROR_CODE Code { get; set; }
        public Object Data { get; set; }

        public ResponseData()
        {
            Code = ERROR_CODE.FAIL;
            Data = null;
        }
    }

  
}
