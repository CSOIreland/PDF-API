using API;
using FluentValidation;
using PDFapi.Resources;
using System;
using System.Data;

namespace PDFapi.Template
{
    /// <summary>
    /// Base Abstract class to allow for the template method pattern of Create, Read, Update and Delete objects from our model
    /// 
    /// </summary>
    internal abstract class BaseTemplate_Delete<T, V> : BaseTemplate<T, V>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="request"></param>
        /// <param name="validator"></param>
        protected BaseTemplate_Delete(JSONRPC_API request, IValidator<T> validator) : base(request, validator)
        {
        }

        /// <summary>
        /// On success
        /// </summary>
        protected override void OnExecutionSuccess()
        {
            Log.Instance.Debug("Record deleted");
            //See if there's a cache in the process. If so then we need to flush the cache.
            if (MethodReader.MethodHasAttribute(Request.method, Utility.GetCustomConfig("APP_CACHE_FLUSH_ATTRIBUTE")))
            {
                cDTO = new CacheMetadata(Utility.GetCustomConfig("APP_CACHE_FLUSH_ATTRIBUTE"), Request.method, DTO);
                foreach (Cas cas in cDTO.CasList) MemCacheD.CasRepositoryFlush(cas.CasRepository + cas.Domain);

            }
        }

        /// <summary>
        /// On error
        /// </summary>
        protected override void OnExecutionError()
        {
            Log.Instance.Debug("No record deleted");
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <returns></returns>
        public BaseTemplate_Delete<T, V> Delete()
        {
            try
            {
                //Run the parameters through the cleanse process
                dynamic cleansedParams = Cleanser.Cleanse(Request.parameters);

                try
                {
                    DTO = GetDTO(cleansedParams);
                }
                catch
                {
                    throw new InputFormatException();
                }

                DTO = Sanitizer.Sanitize(DTO);

                DTOValidationResult = Validator.Validate(DTO);

                if (!DTOValidationResult.IsValid)
                {
                    OnDTOValidationError();
                    return this;
                }

                // The Actual Creation should happen here by the specific class!
                if (!Execute())
                {
                    OnExecutionError();
                    return this;
                }
                OnExecutionSuccess();
                return this;
            }
            catch (FormatException formatException)
            {
                //A FormatException error has been caught, log the error and return a message to the caller
                Log.Instance.Error(formatException);
                Response.error = Label.Get("error.schema");
                return this;
            }
            catch (InputFormatException inputError)
            {
                //An error has been caught, log the error and return a message to the caller
                Log.Instance.Error(inputError);
                Response.error = Label.Get("error.schema");
                return this;
            }
            catch (Exception ex)
            {
                //An error has been caught, log the error and return a message to the caller
                Log.Instance.Error(ex);
                Response.error = Label.Get("error.exception");
                return this;
            }
            finally
            {
                Dispose();
            }
        }

    }
}
