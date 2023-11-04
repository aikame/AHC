using ADDC.Services;
using ActiveDirectory.Services;
using Newtonsoft.Json;
using Powershell;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Management.Automation;
using System.Web.Http;

using Powershell.Abstractions;

namespace ADDC.Controllers
{
    [ConnectorAuthorize]
    public class PowershellController : BaseController 
    {
    }
}
