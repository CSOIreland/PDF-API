using Autofac;
using PDFapi.Data.BSO;

namespace PDFapi.Data
{
    public static class Convert_Container
    {
        public static IContainer Configure()
        {
            ContainerBuilder builder = new ContainerBuilder();
            builder.RegisterType<Convert_BSO>().As<IConvert_BSO>();
            return builder.Build();
        }
    }
}
