using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using YesSql.Extensions.InMemory;
using YesSql.Extensions.SqLite;
using YesSql.Extensions.SqlServer;

namespace Demo
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            //services.AddDbProvider(options => options.UseInMemory());

            //services.AddDbProvider(options =>
            //    options.UseSqLite("Filename=YesSql.db"));

            services.AddDbProvider(options =>
                options.UseSqlServer("Server=.;Database=YesSql;Integrated Security=True"));

            services.AddMvc();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();
        }
    }
}
