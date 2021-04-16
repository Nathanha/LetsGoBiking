using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.ServiceModel;

namespace WebProxyService
{
    [ServiceContract]
    public interface IWebProxyService
    {

        [OperationContract]
        List<Station> GetStationsForContract(string contract);

        [OperationContract]
        Station GetStation(int station_id,  string contract);

    }

}
