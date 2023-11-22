namespace SKD.Domain.Config;

public class PcvTransmission_Config : IEntityTypeConfiguration<PcvTransmission> {
    public void Configure(EntityTypeBuilder<PcvTransmission> builder) {

        builder.ToTable("pcv_transmission");

        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).HasMaxLength(EntityFieldLen.Id).ValueGeneratedOnAdd();
        builder.Property(t => t.Code).HasMaxLength(EntityFieldLen.Category_Code);
        builder.Property(t => t.Name).HasMaxLength(EntityFieldLen.Category_Name);

        builder.HasIndex(t => t.Code).IsUnique();
        builder.HasIndex(t => t.Name).IsUnique();

        builder.HasMany(t => t.Pcvs)
            .WithOne(t => t.PcvTransmission)
            .HasForeignKey(t => t.PcvTransmissionId);

    }
}