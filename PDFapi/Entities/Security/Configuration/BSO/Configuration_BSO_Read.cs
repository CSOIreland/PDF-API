using API;
using PDFapi.Template;
using System.Dynamic;

namespace PDFapi.Security
{
    internal class Configuration_BSO_Read : BaseTemplate_Read<Configuration_DTO_Update, Configuration_VLD_Update>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="request"></param>
        internal Configuration_BSO_Read(JSONRPC_API request) : base(request, new Configuration_VLD_Update()) { }

        /// <summary>
        /// Execute
        /// </summary>
        /// <returns></returns>
        protected override bool Execute()
        {
            var global = Configuration_BSO.GetCustomConfig(ConfigType.global);
            var server = Configuration_BSO.GetCustomConfig(ConfigType.server);
            dynamic configuration = new ExpandoObject();
            configuration.global = global;
            configuration.server = server;
            Response.data = configuration;
            return true;
        }
    }
}
