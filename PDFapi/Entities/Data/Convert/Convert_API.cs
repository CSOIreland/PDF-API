using API;
using PDFapi.Data.BSO;
using System.Net;

namespace PDFapi.Data
{

    public class Convert_API
    {
        private const string URLS = "urls";

        public static dynamic Create(JSONRPC_API jsonrpcRequest)
        {
            return new Convert_BSO_Create(jsonrpcRequest).Create().Response;
        }

        public static dynamic Create(RESTful_API restfulRequestApi)
        {
            // Map the RESTful request to an equivalent Json Rpc request
            JSONRPC_API jsonRpcRequest = Map.RESTful2JSONRPC_API(restfulRequestApi);

            // Map the parameters
            Convert_MAP map = new Convert_MAP();
            jsonRpcRequest.parameters = map.Create_MapParameters(jsonRpcRequest.parameters, jsonRpcRequest.httpGET[URLS]);
            JSONRPC_Output response = new Convert_BSO_Create(jsonRpcRequest).Create().Response;
            if (response.data == null)
            {
                return Map.JSONRPC2RESTful_Output(response, "application/pdf", response.data == null ? HttpStatusCode.InternalServerError : HttpStatusCode.OK, HttpStatusCode.InternalServerError);
            }
            else if (isResponseBlank(response.data))
            {
                return Map.JSONRPC2RESTful_Output(response, "application/pdf", response.data == "" ? HttpStatusCode.BadRequest : HttpStatusCode.OK, HttpStatusCode.BadRequest);
            }
            return Map.JSONRPC2RESTful_Output(response, "application/pdf", response.data == null ? HttpStatusCode.NotFound : HttpStatusCode.OK);
        }

        private static bool isResponseBlank(dynamic data)
        {
            if (data is byte[])
            {
                return ((byte[])data).Length == 0;
            }
            else if (data is string)
            {
                return ((string)data).Length == 0;
            }
            return false;
        }
    }
}
