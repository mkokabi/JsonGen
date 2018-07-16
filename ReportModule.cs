using Autofac;

namespace JsonGen
{
    public class ReportModule : Autofac.Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<Generator>().
                As<IGenerator>();

            base.Load(builder);
        }
    }
}