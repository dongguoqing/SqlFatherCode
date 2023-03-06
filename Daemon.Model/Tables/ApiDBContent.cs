using System;
using System.Data;
using System.Xml.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

#nullable disable

namespace Daemon.Model.Tables
{
	public partial class ApiDBContent : DbContext
	{
		public ApiDBContent()
		{
		}

		public ApiDBContent(DbContextOptions<ApiDBContent> options)
			: base(options)
		{
		}

		public virtual DbSet<User> Users { get; set; }

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			if (!optionsBuilder.IsConfigured)
			{
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
				optionsBuilder.UseMySql("server=127.0.0.1;port=3306;database=blogtest;uid=root;pwd=123456;pooling=true;charset=utf8", Microsoft.EntityFrameworkCore.ServerVersion.Parse("5.7.38-mysql"));
			}
		}

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.HasCharSet("utf8mb4")
				.UseCollation("utf8mb4_general_ci");

			modelBuilder.Entity<User>(entity =>
			{
				entity.ToTable("user");

				entity.HasComment("用户");

				entity.HasIndex(e => e.UserAccount, "uni_userAccount")
					.IsUnique();

				entity.Property(e => e.Id)
					.HasColumnType("bigint(20)")
					.HasColumnName("id")
					.HasComment("id");

				entity.Property(e => e.CreateTime)
					.HasColumnType("datetime")
					.HasColumnName("createTime")
					.HasDefaultValueSql("CURRENT_TIMESTAMP")
					.HasComment("创建时间");

				entity.Property(e => e.Gender)
					.HasColumnType("tinyint(4)")
					.HasColumnName("gender")
					.HasComment("性别");

				entity.Property(e => e.IsDelete)
					.HasColumnType("tinyint(4)")
					.HasColumnName("isDelete")
					.HasComment("是否删除");

				entity.Property(e => e.UpdateTime)
					.HasColumnType("datetime")
					.ValueGeneratedOnAddOrUpdate()
					.HasColumnName("updateTime")
					.HasDefaultValueSql("CURRENT_TIMESTAMP")
					.HasComment("更新时间");

				entity.Property(e => e.UserAccount)
					.IsRequired()
					.HasMaxLength(256)
					.HasColumnName("userAccount")
					.HasComment("账号");

				entity.Property(e => e.UserAvatar)
					.HasMaxLength(1024)
					.HasColumnName("userAvatar")
					.HasComment("用户头像");

				entity.Property(e => e.UserName)
					.HasMaxLength(256)
					.HasColumnName("userName")
					.HasComment("用户昵称");

				entity.Property(e => e.UserPassword)
					.IsRequired()
					.HasMaxLength(512)
					.HasColumnName("userPassword")
					.HasComment("密码");

				entity.Property(e => e.UserRole)
					.IsRequired()
					.HasMaxLength(256)
					.HasColumnName("userRole")
					.HasDefaultValueSql("'user'")
					.HasComment("用户角色：user/ admin");
			});

			OnModelCreatingPartial(modelBuilder);
		}

		partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
	}
}
