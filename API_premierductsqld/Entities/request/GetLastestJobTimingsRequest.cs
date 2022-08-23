using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

namespace API_premierductsqld.Entities.request
{
    [BindProperties]
    public partial class GetLastestJobTimingsRequest
    {

        public string jobday { get; set; }
        public List<string> users { get; set; }

    }
}
