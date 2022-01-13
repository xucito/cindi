using Cindi.Domain.Entities;
using Cindi.Domain.Entities.BotKeys;
using Cindi.Domain.Entities.ExecutionSchedule;
using Cindi.Domain.Entities.ExecutionTemplates;
using Cindi.Domain.Entities.GlobalValues;
using Cindi.Domain.Entities.Metrics;
using Cindi.Domain.Entities.States;
using Cindi.Domain.Entities.Steps;
using Cindi.Domain.Entities.StepTemplates;
using Cindi.Domain.Entities.Users;
using Cindi.Domain.Entities.Workflows;
using Cindi.Domain.Entities.WorkflowsTemplates;
using Cindi.Domain.Entities.WorkflowTemplates.ValueObjects;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Cindi.Persistence.Data
{
    /*public class ApplicationDbContext : DbContext
    {
        public DbSet<CindiClusterState> CindiClusterStates { get; set; }
        public DbSet<BotKey> BotKeys { get; set; }
        public DbSet<ExecutionSchedule> ExecutionSchedules { get; set; }
        public DbSet<ExecutionTemplate> ExecutionTemplates { get; set; }
        public DbSet<StepTemplate> StepTemplates { get; set; }
        public DbSet<WorkflowTemplate> WorkflowTemplates { get; set; }
        public DbSet<Step> Steps { get; set; }
        public DbSet<Workflow> Workflows { get; set; }
        public DbSet<GlobalValue> GlobalValues { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Metric> Metrics { get; set; }
        public DbSet<MetricTick> MetricTicks { get; set; }
        public DbSet<StepLog> StepLogs { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ExecutionTemplate>()
            .Property(b => b.Inputs)
            .HasColumnType("jsonb");

            modelBuilder.Entity<GlobalValue>()
            .Property(b => b.Value)
            .HasColumnType("jsonb");

            modelBuilder.Entity<Step>()
            .Property(b => b.Inputs)
            .HasColumnType("jsonb");

            modelBuilder.Entity<Step>()
            .Property(b => b.Outputs)
            .HasColumnType("jsonb");

            modelBuilder.Entity<DefaultValue>()
            .Property(b => b.Value)
            .HasColumnType("jsonb");

            modelBuilder.Entity<Workflow>()
            .Property(b => b.Inputs)
            .HasColumnType("jsonb");

            modelBuilder.Entity<CindiClusterState>()
            .Property(b => b.Settings)
            .HasColumnType("jsonb");
        }

            public async Task<T> LockObject<T>(T objectToBeLocked, int msToLock = 60000) where T : TrackedEntity
        {
            var foundObject = Find<T>(objectToBeLocked.Id);
            if (foundObject == null || foundObject.LockExpiryDate > DateTime.UtcNow)
            {
                return null;
            }
            else
            {
                var dateTimeToSet = DateTime.UtcNow.AddMilliseconds(msToLock);
                var lockId = Guid.NewGuid();
                foundObject.LockExpiryDate = dateTimeToSet;
                foundObject.LockId = lockId;
                Update(foundObject);
                await SaveChangesAsync();
                foundObject = Find<T>(objectToBeLocked.Id);
                if(foundObject.LockId == lockId)
                {
                    return foundObject;
                }
                else
                {
                    return null;
                }
            }
        }

        public async Task<T> LockObject<T>(Guid objectToBeLockedId, int msToLock = 60000) where T : TrackedEntity
        {
            var foundObject = Find<T>(objectToBeLockedId);
            if (foundObject == null || foundObject.LockExpiryDate > DateTime.UtcNow)
            {
                return null;
            }
            else
            {
                var dateTimeToSet = DateTime.UtcNow.AddMilliseconds(msToLock);
                var lockId = Guid.NewGuid();
                foundObject.LockExpiryDate = dateTimeToSet;
                foundObject.LockId = lockId;
                Update(foundObject);
                await SaveChangesAsync();
                foundObject = Find<T>(objectToBeLockedId);
                if (foundObject.LockId == lockId)
                {
                    return foundObject;
                }
                else
                {
                    return null;
                }
            }
        }*/
    }
}
