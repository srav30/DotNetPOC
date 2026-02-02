namespace AccountAPI.Models;

public class AccountFile
{
    public int FileId { get; set; }
    public int AccountId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty; // csv, pdf, etc.
    public byte[] FileContent { get; set; } = Array.Empty<byte>(); // actual file bytes
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
}
