using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EfCore.Sharding.Suggestion.Entities;
using EfCore.Sharding.Suggestion.Sharding;
using EfCore.Sharding.Suggestion.Sharding.Abstractions;
using EfCore.Sharding.Suggestion.Sharding.Abstractions.Shardings;
using EfCore.Sharding.Suggestion.Sharding.EFCores;
using EfCore.Sharding.Suggestion.Sharding.Impls;
using EfCore.Sharding.Suggestion.Sharding.Impls.Shardings;
using EfCore.Sharding.Suggestion.ShardingRoutes;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EfCore.Sharding.Suggestion
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            //用户需要配置的路由
            services.AddSingleton<IVirtualRoute<LogMessage>, LogMessageRoute>();
            
            var options = new SqlServerOptions(){};
            services.AddSingleton(options);
            services.AddScoped<IRepository, DefaultRepository>();
            services.AddSingleton<IShardingDbContextProvider, ShardingDbContextProvider>();
            services.AddSingleton<IVirtualTableManager, OneDbVirtualTableManager>();
            services.AddSingleton(typeof(IVirtualTable<>), typeof(OneDbVirtualTable<>));
            services.AddSingleton<ShardingBootstrapper>();
            var dbContextOptions = new DbContextOptionsBuilder()
                .UseSqlServer(options.ConnectionString)
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
                .ReplaceService<IModelCacheKeyFactory, ShardingModelCacheKeyFactory>()
                .Options;
            services.AddSingleton(dbContextOptions);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.ApplicationServices.GetService<ShardingBootstrapper>().Start();
            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}