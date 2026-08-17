using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderFlow.Infrastructure.Persistence.Inbox;

namespace OrderFlow.Infrastructure.Persistence.Configurations;

public sealed class InboxMessageConfiguration : IEntityTypeConfiguration<InboxMessage>
{
    public void Configure(EntityTypeBuilder<InboxMessage> builder)
    {
        builder.ToTable("InboxMessages");
        builder.HasKey(x => x.EventId);
        builder.Property(x => x.Type).HasMaxLength(300).IsRequired();
        builder.Property(x => x.Payload).IsRequired();
        builder.Property(x => x.ReceivedOnUtc).IsRequired();
        builder.Property(x => x.ProcessingStartedOnUtc).IsRequired();
        builder.Property(x => x.ProcessedOnUtc);
        builder.HasIndex(x => x.ProcessedOnUtc).HasDatabaseName("IX_InboxMessages_ProcessedOnUtc");
        builder.Property(x => x.Error);
        builder.Property(message => message.Status).HasConversion<int>().IsRequired();
    }
}
