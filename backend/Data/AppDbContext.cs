using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Data;

// Таблицы и имена столбцов настроены так же, как в db/schema.sql (ЛР9)
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<WasteRequest> WasteRequests => Set<WasteRequest>();
    public DbSet<Laboratory> Laboratories => Set<Laboratory>();
    public DbSet<WasteType> WasteTypes => Set<WasteType>();
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Laboratory>(e =>
        {
            e.ToTable("laboratories");
            e.HasKey(x => x.Id).HasName("laboratories_pkey");
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.Name).HasColumnName("name").IsRequired();
            e.HasIndex(x => x.Name).IsUnique().HasDatabaseName("laboratories_name_key");

            e.HasData(
                new Laboratory { Id = 1, Name = "Лаборатория аналитической химии" },
                new Laboratory { Id = 2, Name = "Лаборатория органического синтеза" },
                new Laboratory { Id = 3, Name = "Лаборатория 214 (физхимия)" });
        });

        modelBuilder.Entity<WasteType>(e =>
        {
            e.ToTable("waste_types", t =>
                t.HasCheckConstraint("waste_types_hazard_class_check", "hazard_class BETWEEN 1 AND 5"));
            e.HasKey(x => x.Id).HasName("waste_types_pkey");
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.Name).HasColumnName("name").IsRequired();
            e.Property(x => x.HazardClass).HasColumnName("hazard_class");
            e.HasIndex(x => x.Name).IsUnique().HasDatabaseName("waste_types_name_key");

            e.HasData(
                new WasteType { Id = 1, Name = "Отработанные кислоты", HazardClass = 2 },
                new WasteType { Id = 2, Name = "Органические растворители", HazardClass = 3 },
                new WasteType { Id = 3, Name = "Ртутные лампы", HazardClass = 1 },
                new WasteType { Id = 4, Name = "Битая лабораторная посуда", HazardClass = 5 });
        });

        modelBuilder.Entity<User>(e =>
        {
            e.ToTable("users", t =>
                t.HasCheckConstraint("users_role_check", "role IN ('LabHead', 'Engineer', 'EcoResponsible')"));
            e.HasKey(x => x.Id).HasName("users_pkey");
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.Login).HasColumnName("login").IsRequired();
            e.Property(x => x.FullName).HasColumnName("full_name").IsRequired();
            e.Property(x => x.Role).HasColumnName("role").IsRequired();
            e.Property(x => x.PasswordHash).HasColumnName("password_hash");
            e.HasIndex(x => x.Login).IsUnique().HasDatabaseName("users_login_key");

            // учебные пользователи до ЛР11, без паролей
            e.HasData(
                new User { Id = 1, Login = "petrova", FullName = "Петрова Анна Сергеевна", Role = "LabHead" },
                new User { Id = 2, Login = "ivanov", FullName = "Иванов Дмитрий Олегович", Role = "Engineer" },
                new User { Id = 3, Login = "sabirov", FullName = "Сабиров Ильнур Ринатович", Role = "Engineer" },
                new User { Id = 4, Login = "kuznetsova", FullName = "Кузнецова Мария Петровна", Role = "EcoResponsible" });
        });

        modelBuilder.Entity<WasteRequest>(e =>
        {
            e.ToTable("waste_requests", t =>
            {
                t.HasCheckConstraint("waste_requests_status_check",
                    "status IN ('New', 'InProgress', 'Closed', 'Cancelled')");
                t.HasCheckConstraint("waste_requests_title_check", "char_length(title) BETWEEN 5 AND 80");
                t.HasCheckConstraint("waste_requests_description_check",
                    "char_length(description) BETWEEN 10 AND 500");
            });
            e.HasKey(x => x.Id).HasName("waste_requests_pkey");
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.Number).HasColumnName("number").IsRequired();
            e.Property(x => x.Title).HasColumnName("title").IsRequired();
            e.Property(x => x.Description).HasColumnName("description").IsRequired();
            e.Property(x => x.Status).HasColumnName("status").HasConversion<string>();
            e.Property(x => x.LaboratoryId).HasColumnName("laboratory_id");
            e.Property(x => x.WasteTypeId).HasColumnName("waste_type_id");
            e.Property(x => x.CreatedByUserId).HasColumnName("created_by_user_id");
            e.Property(x => x.AssigneeUserId).HasColumnName("assignee_user_id");
            e.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");
            e.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("now()");

            e.HasIndex(x => x.Number).IsUnique().HasDatabaseName("waste_requests_number_key");
            e.HasIndex(x => x.Status).HasDatabaseName("waste_requests_status_idx");
            e.HasIndex(x => x.LaboratoryId).HasDatabaseName("waste_requests_laboratory_id_idx");
            e.HasIndex(x => x.AssigneeUserId).HasDatabaseName("waste_requests_assignee_user_id_idx");

            e.HasOne<Laboratory>().WithMany().HasForeignKey(x => x.LaboratoryId)
                .HasConstraintName("waste_requests_laboratory_id_fkey").OnDelete(DeleteBehavior.NoAction);
            e.HasOne<WasteType>().WithMany().HasForeignKey(x => x.WasteTypeId)
                .HasConstraintName("waste_requests_waste_type_id_fkey").OnDelete(DeleteBehavior.NoAction);
            e.HasOne<User>().WithMany().HasForeignKey(x => x.CreatedByUserId)
                .HasConstraintName("waste_requests_created_by_user_id_fkey").OnDelete(DeleteBehavior.NoAction);
            e.HasOne<User>().WithMany().HasForeignKey(x => x.AssigneeUserId)
                .HasConstraintName("waste_requests_assignee_user_id_fkey").OnDelete(DeleteBehavior.NoAction);
        });
    }
}
