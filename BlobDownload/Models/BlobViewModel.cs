namespace BlobDownload.Models
{
    public class BlobContaner
    {
        public string? Name { get; set; }
        public List<Blobs>? Blobis { get; set; }
        public BlobContaner(string? _name, List<Blobs> _blobis)
        {
            Name = _name;
            Blobis = _blobis;
        }
        public BlobContaner()
        {
        }
    }
    public class Blobs
    {
        public string? Date { get; set; }
        public string? SiteName { get; set; }
        public string? FileName { get; set; }

        public Blobs(string? date, string? siteName, string? fileName)
        {
            Date = date;
            SiteName = siteName;
            FileName = fileName;
        }
        public Blobs()
        {
        }
    }
}
