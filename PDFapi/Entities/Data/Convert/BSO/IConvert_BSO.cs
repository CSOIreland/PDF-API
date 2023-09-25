using API;

namespace PDFapi.Data.BSO
{
    public interface IConvert_BSO
    {
        dynamic Create(JSONRPC_API request, Convert_DTO_Create dto);
    }
}
