using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ASCTracAuthAPI
{
    public interface IJWTAuth
    {
        string Authentication(string username, string password);
    }
}
