using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace ContactsIntegration
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IBulkLoadService" in both code and config file together.
    [ServiceContract]
    public interface IBulkLoadService
    {
        [OperationContract]
        void BulkLoadContacts([MessageParameter(Name = "SourceID")]
                                         string sourceID);


        [OperationContractAttribute(AsyncPattern = true)]
        IAsyncResult BeginAsyncBulkLoadContacts(string sourceID, AsyncCallback callback, object asyncState);

        // Note: There is no OperationContractAttribute for the end method.
        string EndAsyncBulkLoadContacts(IAsyncResult result);
    }
}
