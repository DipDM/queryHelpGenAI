using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace queryHelp.DTOs
{
    public record AiSuggestRequest(int DataSourceId, string NlText);
}