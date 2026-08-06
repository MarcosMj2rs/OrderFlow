using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace OrderFlow.Infrastructure.Persistence.Outbox;

public sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("OutboxMessages");

        builder.HasKey(message => message.Id);

        builder.Property(message => message.Id)
            .ValueGeneratedNever();

        builder.Property(message => message.Type)
            .HasMaxLength(300)
            .IsRequired();

        builder.Property(message => message.Payload)
            .HasColumnType("nvarchar(max)")
            .IsRequired();

        builder.Property(message => message.OccurredOnUtc)
            .HasColumnType("datetime2")
            .IsRequired();

        builder.Property(message => message.ProcessedOnUtc)
            .HasColumnType("datetime2")
            .IsRequired(false);

        builder.Property(message => message.Error)
            .HasColumnType("nvarchar(max)")
            .IsRequired(false);

        builder.HasIndex(
                message => new
                {
                    message.ProcessedOnUtc,
                    message.OccurredOnUtc
                })
            .HasDatabaseName(
                "IX_OutboxMessages_ProcessedOnUtc_OccurredOnUtc");
    }
}