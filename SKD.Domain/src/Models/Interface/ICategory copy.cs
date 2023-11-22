namespace SKD.Domain;

public interface IPcvMetaICategory : ICategory{
    public ICollection<PCV> Pcvs { get; set;  }
}