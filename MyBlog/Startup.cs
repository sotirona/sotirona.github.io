using Microsoft.Owin;
using MyBlog.Migrations;
using MyBlog.Models;
using Owin;
using System.Data.Entity;

[assembly: OwinStartupAttribute(typeof(MyBlog.Startup))]
namespace MyBlog
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<BlogDbContext, Configuration>());
            ConfigureAuth(app);
        }
    }
}
