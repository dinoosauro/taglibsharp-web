namespace MetadataChange
{
    /// <summary>
    /// Content of the response obtained from LRCLib.
    /// </summary>
    public class LRCLibJson
    {
        public int? id { get; set; }
        public string? trackName { get; set; }
        public string? artistName { get; set; }
        public string? albumName { get; set; }
        public float? duration { get; set; }
        public bool? instrumental { get; set; }
        public string? plainLyrics { get; set; }
        public string? syncedLyrics { get; set; }
    }
}