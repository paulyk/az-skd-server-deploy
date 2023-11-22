namespace SKD.Domain.Config;

public class KitStatusEventType_Config : IEntityTypeConfiguration<KitStatusEventType> {
    public void Configure(EntityTypeBuilder<KitStatusEventType> builder) {

        builder.ToTable("kit_timeline_event_type");

        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).HasMaxLength(EntityFieldLen.Id).ValueGeneratedOnAdd();

        builder.Property(t => t.Code).HasConversion<string>();
        builder.HasIndex(t => t.Code).IsUnique();

        builder.Property(t => t.Code).IsRequired().HasMaxLength(EntityFieldLen.Event_Code);
        builder.Property(t => t.Description).IsRequired().HasMaxLength(EntityFieldLen.Event_Description);

        builder.Property(t => t.PartnerStatusCode).HasConversion<string>();
        builder.Property(t => t.PartnerStatusCode).HasMaxLength(EntityFieldLen.Event_Code);
        builder.HasIndex(t => t.PartnerStatusCode).IsUnique();

        builder.HasIndex(t => t.Code).IsUnique();
    }
}
