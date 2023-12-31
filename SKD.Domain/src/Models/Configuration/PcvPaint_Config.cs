namespace SKD.Domain.Config;

public class PcvPaint_Config : IEntityTypeConfiguration<PcvPaint> {
    public void Configure(EntityTypeBuilder<PcvPaint> builder) {

        builder.ToTable("pcv_paint");

        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).HasMaxLength(EntityFieldLen.Id).ValueGeneratedOnAdd();
        builder.Property(t => t.Code).HasMaxLength(EntityFieldLen.Category_Code);
        builder.Property(t => t.Name).HasMaxLength(EntityFieldLen.Category_Name);

        builder.HasIndex(t => t.Code).IsUnique();
        builder.HasIndex(t => t.Name).IsUnique();

        builder.HasMany(t => t.Pcvs)
            .WithOne(t => t.PcvPaint)
            .HasForeignKey(t => t.PcvPaintId);

    }
}