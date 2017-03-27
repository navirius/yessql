using YesSql.Core.Services;

namespace YesSql.Data
{
    public class DbProviderOptions : IDbProviderOptions
    {
        public string ProviderName { get; set; }

        public Configuration Configuration { get; set; }
    }
}
