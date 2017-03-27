using YesSql.Core.Services;

namespace YesSql.Data
{
    public interface IDbProviderOptions
    {
        string ProviderName { get; set; }

        Configuration Configuration { get; set; }
    }
}
