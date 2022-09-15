using System;
using DTO_PremierDucts.Entities;

namespace DTO_PremierDucts.EntityResponse
{
    public class DispatchDetailResponse : DispatchDetail
    {
        public string pathValue { get; set; }
        public DispatchDetailResponse()
        {
        }
    }
}

