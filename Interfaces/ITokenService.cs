using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using queryHelp.Models;

namespace queryHelp.Interfaces
{
    public interface ITokenService
    {
        string CreateToken(User user);
    }
}