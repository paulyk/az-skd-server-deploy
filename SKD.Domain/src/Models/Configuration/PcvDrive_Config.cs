namespace SKD.Domain.Config;

public class PcvDrive_Config : IEntityTypeConfiguration<PcvDrive> {
    public void Configure(EntityTypeBuilder<PcvDrive> builder) {

        builder.ToTable("pcv_drive");

        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).HasMaxLength(EntityFieldLen.Id).ValueGeneratedOnAdd();
        builder.Property(t => t.Code).HasMaxLength(EntityFieldLen.Category_Code);
        builder.Property(t => t.Name).HasMaxLength(EntityFieldLen.Category_Name);

        builder.HasIndex(t => t.Code).IsUnique();
        builder.HasIndex(t => t.Name).IsUnique();

        builder.HasMany(t => t.Pcvs)
            .WithOne(t => t.PcvDrive)
            .HasForeignKey(t => t.PcvDriveId);

    }
}