using System.Text.Json.Serialization;

public class DocumentTag
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int DocumentId { get; set; }
     [JsonIgnore]
    public Document Document { get; set; }
}
