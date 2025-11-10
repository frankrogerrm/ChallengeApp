public class DocumentShare
{
    public int Id { get; set; }
    public int DocumentId { get; set; }
    public required string SharedWithUserEmail { get; set; }
    public required string Permission { get; set; }
    public required Document Document { get; set; }
}
