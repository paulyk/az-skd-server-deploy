namespace SKD.Domain.Config;

public class KitStatusEvent_Config : IEntityTypeConfiguration<KitStatusEvent> {
    public void Configure(EntityTypeBuilder<KitStatusEvent> builder) {

        builder.ToTable("kit_timeline_event");

        builder.HasKey(t => t.Id);
        builder.HasIndex(t => t.CreatedAt);
        builder.HasIndex(t => new { t.KitId, t.CreatedAt });
        builder.HasIndex(t => t.EventDate);
        builder.HasIndex(t => t.PartnerStatusUpdatedAt);
        builder.HasIndex(t => t.RemovedAt);

        builder.Property(t => t.Id).HasMaxLength(EntityFieldLen.Id).ValueGeneratedOnAdd();
        builder.Property(t => t.EventNote).HasMaxLength(EntityFieldLen.Event_Note);

        builder.HasOne(t => t.Kit)
            .WithMany(t => t.KitStatusEvents)
            .HasForeignKey(t => t.KitId);
    }

}
