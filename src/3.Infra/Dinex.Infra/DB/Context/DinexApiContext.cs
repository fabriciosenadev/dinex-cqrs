﻿namespace Dinex.Infra;

public class DinexApiContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<QueueIn> QueueIn { get; set; }

    #region
    public DbSet<InvestmentHistory> InvestmentHistory { get; set; }
    #endregion

    public DinexApiContext(DbContextOptions<DinexApiContext> options) : base(options)
    {

    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Ignore<Notifiable<Notification>>();
        modelBuilder.Ignore<Notification>();

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DinexApiContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}
