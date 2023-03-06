

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Infrastructure;
namespace Daemon.Model
{
    public class ApiDBContent : DbContext
    {

        public ApiDBContent(DbContextOptions<ApiDBContent> options) : base(options)
        {

        }

        public DbSet<BlogUser> BlogUser { get; set; }

        public DbSet<BlogSysInfo> BlogSysInfo { get; set; }

        public DbSet<Article> Article { get; set; }
        public DbSet<ArticleComment> ArticleComment { get; set; }
        public DbSet<BlogDictionary> BlogDictionary { get; set; }
        public DbSet<Resource> Resource { get; set; }
        public DbSet<BlogRole> BlogRole { get; set; }
        public DbSet<BlogRoleResource> BlogRoleResource { get; set; }
        public DbSet<BlogUserRole> BlogUserRole { get; set; }

        public DbSet<BlogFile> BlogFile { get; set; }

        public DbSet<FileDirectory> FileDirectory { get; set; }

        public DbSet<Order> Order { get; set; }

        public DbSet<Customer> Customer { get; set; }

        public DbSet<Dict> Dict { get; set; }

        public DbSet<TableInfo> TableInfo { get; set; }

        public DbSet<FieldInfo> FieldInfo { get; set; }

        public DbSet<User> User { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.ReplaceService<IModelCacheKeyFactory, DynamicModelCacheKeyFactory>();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }

    public class ApplicationContextDbFactory : IDesignTimeDbContextFactory<ApiDBContent>
    {
        ApiDBContent IDesignTimeDbContextFactory<ApiDBContent>.CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ApiDBContent>();
            string connectionStr = string.Empty;
            optionsBuilder.UseMySql<ApiDBContent>(ServerVersion.AutoDetect("Server=103.28.213.9;port=3306;Database=blogtest;uid=root;pwd=123qwe!@#;pooling=true;CharSet=utf8")).ReplaceService<IModelCacheKeyFactory, DynamicModelCacheKeyFactory>().UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            return new ApiDBContent(optionsBuilder.Options);
        }
    }

    //class SchemaModelCacheFactory : IModelCacheKeyFactory
    //{
    //    public object Create(DbContext context) => new SchemaModelCache(context);
    //}


    //class SchemaModelCache : ModelCacheKey
    //{
    //    readonly string _schema;
    //    public SchemaModelCache(DbContext context) : base(context)
    //    {
    //        _schema = (context as ApiDBContent)?.CurrentScheme
    //    }
    //}
    //public class DynamicModelCacheKeyFactory : IModelCacheKeyFactory
    //{
    //    public object Create(DbContext context)
    //    {
    //        if (context is ApiDBContent apiDbContent)
    //        {
    //            return (context.GetType());
    //        }
    //        return context.GetType();
    //    }
    //}
}
