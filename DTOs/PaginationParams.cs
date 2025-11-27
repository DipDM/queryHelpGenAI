using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace queryHelp.DTOs
{
    public class PaginationParams
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }
}