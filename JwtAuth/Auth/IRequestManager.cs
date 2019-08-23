using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Auth
{
    internal interface IRequestManager
    {
        HttpContext ProcessRequest(HttpContext context);
    }
}
