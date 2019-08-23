using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Auth;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace WebApplication1
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
            services.Configure<AuthConfiguration>(o =>
            {
                o.Seed = GetSeed();
                o.AccessTokenExpirationSpan = TimeSpan.FromSeconds(5);
                o.RefreshTokenExpirationSpan = 2 * o.AccessTokenExpirationSpan;
            });
            services.AddAuth();
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseAuthMiddleware();
            app.UseMvc();
        }

        private string GetSeed() => "2eXV5HQbXwdjPFyKd6szuoDzBSGe3TlgLZpELheldv8Wn3uQ3oCvkfzehbllxdOiCqiMH21pwblRI0cuPfHjRaxf16gpbgnVtLRKw3T1Qb3IFdlmdSwagri2cpPQ8uvWpEoEMGQ1W2G29MWfvWSkvKHY9psMRIaBhXz5oEYGTJQ8MOEHixKlEimqftGLwzwmPtcpOlnptv5c1ASMEDlbjn2qEMqCI4838CKmivuhVwEwQnr9gEiXxudx20TvRJ2g1ngSiP00fX8SGiluj4OBWosfVroMdFk0zgKM5X9WqudOHjCcrjtCk76s3jnAf0XynN0dtxebZnMv21TOo2JvUdAgRej4pMME85pAUJvRoiPsWe5X1tqL6k8h01mAo01AVWgiOL1NsMR30GX5ZK8cTzLW2kWoG8dwvxJdFgY1fJczwrGBx5RLHjXlqctlIn4ihUxipO6hgEbX0eQ1vc7sTC5YChlMbW2lnNVzvOvvy2Sb6k0VnPbj6GzDig5iGveGHkyuwX5T6xPX5ftgRx3mmbqPJeTBCYnvYtWRCT1BY1ohdY0Ge3sPZEN9XZPGtv45LfgpUFXycBmFtOfqN0NuCpkb4Ns4f7AXaAsYXqZ9Tz1yCuVig3FISY44dRWlqjib2AXGF8xZxjMfwsNgItIOfK1nvigJgGlIB8g0srN9FEmUMVvWUTclPYm10zkgoMc9wjEHZDp4omrHXUHro055g68ACFkaqrYMbn91UGxThXz7s9OsU50u4XjWQROVBFtCqiBEBlkTNJ5PT19RDv9c4Dh1ANtlhuIKMin77zxpRGDB6S0xScQbvU1Qjbbos8bL2rF2EqhPHuJinHWA0H9FpXGfRJyKIgsLnmEMPneef7KkMmH038Z5UKp9hyaXkp0wr1VsmBoqprAxsYjhdeyYigeFwjToRPVXrDgNqADAyZ0usUDVOA309x1pSSWYQu8scWi2qZfcRhLKJML94AERl7s462QSxv7ywGdC7QOvyEKqtBMpQ3sxVolW56SXNcJV";

    }
}
