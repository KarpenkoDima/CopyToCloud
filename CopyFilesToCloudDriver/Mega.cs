using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CG.Web.MegaApiClient;

namespace CopyFilesToCloudDriver
{
    interface ICloud
    {
        void ConnectToDrive(object o);
    }
}
