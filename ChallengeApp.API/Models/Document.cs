public class Document
{
    public int Id { get; set; }
    public string Title { get; set; }
    public List<DocumentTag> Tags { get; set; } = new();
    public string UploadedBy { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime ModifiedDate { get; set; }
    public string AccessType { get; set; } 
    public string FileName { get; set; }
    public long FileSize { get; set; }
}
