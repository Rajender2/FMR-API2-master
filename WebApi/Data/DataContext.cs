using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WebApi.Models;
using Microsoft.AspNetCore.Identity;

namespace WebApi.Data
{
    public class DataContext : IdentityDbContext<User, Role, long, IdentityUserClaim<long>, UserRole, IdentityUserLogin<long>, IdentityRoleClaim<long>, IdentityUserToken<long>>
    {
        public DataContext(DbContextOptions options)
            : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>()
                .ToTable("User");

            modelBuilder.Entity<Role>()
                .ToTable("Role");

            modelBuilder.Entity<IdentityUserClaim<long>>(b =>
            {
                b.HasKey(uc => uc.Id);
                b.ToTable("UserClaim");
            });

            modelBuilder.Entity<IdentityRoleClaim<long>>(b =>
            {
                b.HasKey(rc => rc.Id);
                b.ToTable("RoleClaim");
            });

            modelBuilder.Entity<UserRole>(b =>
            {
                b.HasKey(ur => new { ur.UserId, ur.RoleId });
                b.HasOne(ur => ur.Role).WithMany(x => x.UserRoles).HasForeignKey(r => r.RoleId);
                b.HasOne(ur => ur.User).WithMany(x => x.UserRoles).HasForeignKey(u => u.UserId);
                b.ToTable("UserRole");
            });

            modelBuilder.Entity<IdentityUserLogin<long>>(b =>
            {
                b.HasKey(ur => new { ur.LoginProvider, ur.ProviderKey, ur.UserId });
                b.ToTable("UserLogin");

            });

            modelBuilder.Entity<IdentityUserToken<long>>(b =>
            {
                b.HasKey(ur => new { ur.LoginProvider, ur.UserId });
                b.ToTable("UserToken");

            });

            modelBuilder.Entity<UserAddress>()
                .HasOne(x => x.User)
                .WithMany(a => a.UserAddresses)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<vwCandidateJob>()
               .HasKey(c => new {c.JobOrderId, c.CandidateId, c.QuestionId, c.AssessmentId });
        }
        public DbSet<User> User { get; set; }
        public DbSet<Role> Role { get; set; }
        public DbSet<Address> Address { get; set; }
        public DbSet<State> State { get; set; }
        public DbSet<City> City { get; set; }

        public DbSet<Company> Company { get; set; }
        public DbSet<JobOrder> JobOrder { get; set; }
        public DbSet<JobType> Jobtype { get; set; }
        public DbSet<QuestionType> QuestionType { get; set; }
        public DbSet<Question> Question { get; set; }
        public DbSet<Candidate> Candidate { get; set; }
        public DbSet<InviteCandidate> InviteCandidate { get; set; }

        //ViewModels
        public DbSet<Assessment> Assessment { get; set; }
        public DbSet<JobCandidate> JobCandidate { get; set; }
        public DbSet<JobQuestion> JobQuestion { get; set; }
        public DbSet<AssesmentCandidate> AssesmentCandidate { get; set; }
        public DbSet<AssessmentOnBoarding> AssessmentOnBoarding { get; set; }
        public DbSet<vwCandidateJob> vwCandidateJob { get; set; }

        public DbSet<PracticeCandidate> PracticeCandidate { get; set; }
        public DbSet<AssessmentDocument> AssessmentDocument { get; set; }
        public DbSet<AssessmentForm> AssessmentForm { get; set; }

        public DbSet<JobOrderDocuments> JobOrderDocuments { get; set; }
        public DbSet<JobMCQuestion> JobMCQuestion { get; set; }
        public DbSet<DocumentTemplate> DocumentTemplate { get; set; }

        public DbSet<FormTemplate> FormTemplate { get; set; }

        public DbSet<vwPractice> vwPractice { get; set; }


        //Readonly/Master
        public DbSet<AssessmentStatus> AssessmentStatus { get; set; }
        public DbSet<JobStatus> JobStatus { get; set; }
        public DbSet<Calendar> Calendar { get; set; }
        public DbSet<Event> Event { get; set; }
        public DbSet<EventUser> EventUser { get; set; }
        public DbSet<ActivityLog> ActivityLog { get; set; }
        public DbSet<UserActivate> UserActivate { get; set; }

    }
}
