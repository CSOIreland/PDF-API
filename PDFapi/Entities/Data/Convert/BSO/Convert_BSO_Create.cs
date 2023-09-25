using API;
using Autofac;
using PDFapi.Resources;
using PDFapi.Template;
using System;
using System.Web;
using C = System.Convert;

namespace PDFapi.Data.BSO
{
    internal class Convert_BSO_Create : BaseTemplate_Create<Convert_DTO_Create, Convert_VLD_Create>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="request"></param>
        internal Convert_BSO_Create(JSONRPC_API request) : base(request, new Convert_VLD_Create())
        {
        }

        /// <summary>
        /// Execute
        /// </summary>
        /// <returns></returns>
        protected override bool Execute()
        {
            var container = Convert_Container.Configure();

            using (var scope = container.BeginLifetimeScope())
            {
                var convertBSO = scope.Resolve<IConvert_BSO>();
                dynamic data;
                try
                {
                    data = convertBSO.Create(Request, DTO);
                }
                catch (Exception e)
                {
                    Response.error = e.Message;
                    if (e is HttpRequestValidationException)
                    {
                        Response.data = "";
                    }
                    return false;
                }
                switch (DTO.ReturnType)
                {
                    case Constants.BTYE_ARRAY:
                        // If the data is cached as a string it needs to be converted to a byte[]
                        if (data.GetType().Name.Equals(Constants.STRING))
                        {
                            Response.data = C.FromBase64String(data);
                        }
                        else
                        {
                            Response.data = (byte[])data;
                        }
                        break;
                    case Constants.BASE_64_STRING:
                    default:
                        Response.data = "data:" + "application/pdf" + ";base64," + data;
                        break;
                }
            }
            return true;
        }
    }
}
