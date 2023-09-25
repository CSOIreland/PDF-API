using API;
using FluentValidation;
using FluentValidation.Results;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using PDFapi.Resources;
using PDFapi.Security;
using System;
using System.Diagnostics;
using System.Linq;

namespace PDFapi.Template
{
    /// <summary>
    /// Base Abstract class to allow for the template method pattern of Create, Read, Update and Delete objects from our model
    /// 
    /// </summary>
    internal abstract class BaseTemplate<T, V>
    {
        #region Properties
        /// <summary>
        /// Cache related metadata
        /// </summary>
        protected CacheMetadata cDTO { get; set; }

        /// <summary>
        /// Request passed into the API
        /// </summary>
        protected JSONRPC_API Request { get; }

        /// <summary>
        /// Response passed back by API
        /// </summary>
        public JSONRPC_Output Response { get; set; }

        /// <summary>
        /// DTO created from request parameters
        /// </summary>
        protected T DTO { get; set; }

        /// <summary>
        /// Validator (Fluent Validation)
        /// </summary>
        protected IValidator<T> Validator { get; }

        /// <summary>
        /// Validation result
        /// </summary>
        protected ValidationResult DTOValidationResult { get; set; }
        #endregion 

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="request"></param>
        /// <param name="validator"></param>
        protected BaseTemplate(JSONRPC_API request, IValidator<T> validator)
        {
            Configuration_BSO.SetConfigFromFiles();

            Request = request;
            Response = new JSONRPC_Output();
            Validator = validator;
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
        }

        /// <summary>
        /// Dispose tidy-up
        /// </summary>
        protected void Dispose()
        {

        }

        #region Abstract methods.
        // These methods must be overriden.
        abstract protected bool Execute();

        /// <summary>
        /// Return the current DTO
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        virtual protected T GetDTO(dynamic parameters = null)
        {
            try
            {
                if (parameters != null)
                {
                    return (T)Activator.CreateInstance(typeof(T), parameters);

                }
                else return (T)Activator.CreateInstance(typeof(T), Request.parameters);
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                {
                    if (ex.InnerException is FormatException)
                    {
                        throw ex.InnerException;
                    }
                }

                throw ex;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        abstract protected void OnExecutionSuccess();

        /// <summary>
        /// 
        /// </summary>
        abstract protected void OnExecutionError();
        #endregion

        #region Default methods.
        // These methods can be left as their default implementations.

        /// <summary>
        /// Validation fail
        /// </summary>
        virtual protected void OnDTOValidationError()
        {
            //parameter validation not ok - return an error and proceed no further
            Log.Instance.Debug("Validation failed: " + JsonConvert.SerializeObject(DTOValidationResult.Errors));
            Response.error = Label.Get("error.validation");
        }
        #endregion


    }

    public class InputFormatException : Exception
    {
        public InputFormatException() : base("Invalid format found in input parameters")
        {

        }
    }
}
