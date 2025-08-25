namespace MetadataChange
{
    public class PicturesType()
    {
        /// <summary>
        /// Information about the possible album art description 
        /// </summary>
        /// <param name="id">The ID of the picture type</param>
        /// <param name="description">Its description to show in the Select</param>
        public class PictureContent(int id, string description)
        {
            /// <summary>
            /// The ID of the picture type
            /// </summary>
            public int id = id;
            /// <summary>
            /// Its description to show in the Select
            /// </summary>
            public string description = description;
        }
        /// <summary>
        /// A List that contains all the possible description of an album art
        /// </summary>
        public static List<PictureContent> pictureTypes = [
            new(8, "Artist image"),
            new(3, "Front cover"),
        new(4, "Back cover"),
        new(10, "Band/orchestra image"),
        new(19, "Band/performer logo"),
        new(17, "Large, colored fish"),
        new(11, "Composer"),
        new(9, "Conductor"),
        new(15, "During artist's performance"),
        new(14, "During track's recording"),
        new(1, "File Icon"),
        new(18, "Illustration"),
        new(7, "Lead artist"),
        new(5, "Leaflet page"),
        new(12, "Lyricist"),
        new(6, "Media (album or disk itself)"),
        new(16, "Movie screen capture"),
        new(255, "Not a Picture (another file type)"),
        new(0, "Other"),
        new(2, "Other file icon"),
        new(20, "Publisher logo"),
        new(13, "Recording location"),
    ];
    }
}
