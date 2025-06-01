using Microsoft.EntityFrameworkCore;
using EmergencyComm.Api.Models;

namespace EmergencyComm.Api.Data
{
    public class EmergencyContext : DbContext
    {
        public EmergencyContext(DbContextOptions<EmergencyContext> options) : base(options)
        {
        }

        public DbSet<Device> Devices { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<MessageDelivery> MessageDeliveries { get; set; }
        public DbSet<Resource> Resources { get; set; }
        public DbSet<DangerZone> DangerZones { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Device configuration
            modelBuilder.Entity<Device>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedNever();
                entity.HasIndex(e => e.IsOnline);
                entity.HasIndex(e => new { e.Latitude, e.Longitude });
                entity.Property(e => e.LastSeen).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.RegisteredAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            });

            // Message configuration
            modelBuilder.Entity<Message>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.HasIndex(e => e.SenderId);
                entity.HasIndex(e => e.CreatedAt);
                entity.HasIndex(e => e.IsEmergency);
                entity.HasIndex(e => e.IsBroadcast);

                // Foreign key relationship
                entity.HasOne(e => e.Sender)
                      .WithMany(d => d.SentMessages)
                      .HasForeignKey(e => e.SenderId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // MessageDelivery configuration
            modelBuilder.Entity<MessageDelivery>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.HasIndex(e => new { e.MessageId, e.RecipientId }).IsUnique();
                entity.HasIndex(e => e.Status);

                // Foreign key relationships
                entity.HasOne(e => e.Message)
                      .WithMany(m => m.Deliveries)
                      .HasForeignKey(e => e.MessageId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Recipient)
                      .WithMany(d => d.ReceivedMessages)
                      .HasForeignKey(e => e.RecipientId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Resource configuration
            modelBuilder.Entity<Resource>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.HasIndex(e => e.Type);
                entity.HasIndex(e => e.IsAvailable);
                entity.HasIndex(e => new { e.Latitude, e.Longitude });
                entity.HasIndex(e => e.ProviderId);

                // Foreign key relationship
                entity.HasOne(e => e.Provider)
                      .WithMany(d => d.SharedResources)
                      .HasForeignKey(e => e.ProviderId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // DangerZone configuration
            modelBuilder.Entity<DangerZone>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.HasIndex(e => e.IsActive);
                entity.HasIndex(e => e.Type);
                entity.HasIndex(e => e.SeverityLevel);
                entity.HasIndex(e => new { e.Latitude, e.Longitude });
                entity.HasIndex(e => e.ReportedBy);
            });

            // Seed data
            SeedData(modelBuilder);
        }

        private static void SeedData(ModelBuilder modelBuilder)
        {
            // Seed sample devices
            modelBuilder.Entity<Device>().HasData(
                new Device
                {
                    Id = "device-001",
                    Name = "Emergency Coordinator Device",
                    DeviceType = "Mobile",
                    Latitude = -23.5505,
                    Longitude = -46.6333,
                    IsOnline = true,
                    Status = "Available",
                    BatteryLevel = 85,
                    NetworkType = "WiFiDirect",
                    SignalStrength = 95.0,
                    RegisteredAt = DateTime.UtcNow.AddDays(-1),
                    LastSeen = DateTime.UtcNow
                },
                new Device
                {
                    Id = "device-002",
                    Name = "Rescue Team Alpha",
                    DeviceType = "Tablet",
                    Latitude = -23.5515,
                    Longitude = -46.6343,
                    IsOnline = true,
                    Status = "Busy",
                    BatteryLevel = 67,
                    NetworkType = "Bluetooth",
                    SignalStrength = 78.0,
                    RegisteredAt = DateTime.UtcNow.AddHours(-6),
                    LastSeen = DateTime.UtcNow.AddMinutes(-2)
                }
            );

            // Seed sample resources
            modelBuilder.Entity<Resource>().HasData(
                new Resource
                {
                    Id = 1,
                    Name = "Abrigo Temporário - Escola Municipal",
                    Type = "Shelter",
                    Description = "Abrigo com capacidade para 200 pessoas, com banheiros e cozinha",
                    Latitude = -23.5525,
                    Longitude = -46.6353,
                    ProviderId = "device-001",
                    IsAvailable = true,
                    Capacity = 200,
                    CurrentUsage = 45,
                    Status = "Available",
                    Priority = "High",
                    ContactName = "Maria Silva",
                    ContactPhone = "(11) 98765-4321",
                    AccessInstructions = "Entrada pela porta principal, apresentar documento",
                    CreatedAt = DateTime.UtcNow.AddHours(-3)
                },
                new Resource
                {
                    Id = 2,
                    Name = "Ponto de Distribuição de Água",
                    Type = "Water",
                    Description = "Caminhão pipa com água potável",
                    Latitude = -23.5535,
                    Longitude = -46.6363,
                    ProviderId = "device-002",
                    IsAvailable = true,
                    Capacity = 5000,
                    CurrentUsage = 1200,
                    Status = "Available",
                    Priority = "Critical",
                    ContactName = "João Santos",
                    ContactPhone = "(11) 91234-5678",
                    AccessInstructions = "Trazer recipientes próprios",
                    CreatedAt = DateTime.UtcNow.AddHours(-2)
                }
            );

            // Seed sample danger zones
            modelBuilder.Entity<DangerZone>().HasData(
                new DangerZone
                {
                    Id = 1,
                    Name = "Área de Deslizamento",
                    Type = "Landslide",
                    Description = "Risco de deslizamento de terra devido às chuvas intensas",
                    Latitude = -23.5545,
                    Longitude = -46.6373,
                    RadiusMeters = 500,
                    SeverityLevel = "High",
                    ReportedBy = "device-001",
                    IsActive = true,
                    IsVerified = true,
                    ConfirmationCount = 3,
                    SafetyInstructions = "Evacuação imediata da área. Não retornar até liberação oficial.",
                    EvacuationRoute = "Seguir pela Rua Principal em direção ao centro",
                    EmergencyContact = "193 - Bombeiros",
                    Status = "Active",
                    CreatedAt = DateTime.UtcNow.AddHours(-4)
                }
            );
        }
    }
}