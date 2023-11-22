namespace SKD.Service;

public record SyncKitComponentsInput(string KitNo);
public record SyncKitComponentsResult(string KitNo, ICollection<string> ComponentCodes);
