namespace MetadataChange
{
    public class AvailableProperties
    {
        /// <summary>
        /// The class that contains information to each metadata tag that can be edited from the standard view. This doesn't count custom metadata!
        /// <param name="DisplayName">The name that'll be visible in the column header and in the FluentSelect</param>
        /// <param name="TagLibProperty">The name of the TagLib Property to edit</param>
        /// <param name="ConvertValue">The type of the TagLib Property to edit</param>
        /// </summary>
        public class PropertiesObject(string DisplayName, string TagLibProperty, PropertiesObject.PropertiesType ConvertValue)
        {
            /// <summary>
            /// The type of the tag entry
            /// </summary>
            public enum PropertiesType
            {
                UINT,
                /// <summary>
                /// Shows a FluentInputField for single-line text
                /// </summary>
                STRING,
                STRING_ARRAY,
                /// <summary>
                /// Shows a FluentTextArea for text that usually is on more liens
                /// </summary>
                STRING_TEXTAREA
            }
            /// <summary>
            /// The name that'll be visible in the column header and in the FluentSelect
            /// </summary>
            public string DisplayName = DisplayName;
            /// <summary>
            /// The name of the TagLib Property to edit
            /// </summary>
            public string TagLibProperty = TagLibProperty;
            /// <summary>
            /// The type of the TagLib Property to edit
            /// </summary>
            /// 
            public PropertiesType ConvertValue = ConvertValue;
        }
        public static List<PropertiesObject> AvailableMetadata =
        [
            new("Album", "Album", PropertiesObject.PropertiesType.STRING),
        new("Album Artists", "AlbumArtists", PropertiesObject.PropertiesType.STRING_ARRAY),
        new ("Album Artists (Sort by)", "AlbumArtistsSort", PropertiesObject.PropertiesType.STRING_ARRAY),
        new ("Amazon ID", "AmazonID", PropertiesObject.PropertiesType.STRING),
        new ("Artists", "Artists", PropertiesObject.PropertiesType.STRING_ARRAY),
        new ("Comment", "Comment", PropertiesObject.PropertiesType.STRING_TEXTAREA),
        new ("Composers", "Composers", PropertiesObject.PropertiesType.STRING_ARRAY),
        new ("Composers (Sort by)", "ComposersSort", PropertiesObject.PropertiesType.STRING_ARRAY),
        new ("Conductor", "Conductor", PropertiesObject.PropertiesType.STRING),
        new ("Copyright", "Copyright", PropertiesObject.PropertiesType.STRING),
        new ("Description", "Description", PropertiesObject.PropertiesType.STRING_TEXTAREA),
        new ("Disc", "Disc", PropertiesObject.PropertiesType.UINT),
        new ("Disc count", "DiscCount", PropertiesObject.PropertiesType.UINT),
        new ("Genres", "Genres", PropertiesObject.PropertiesType.STRING_ARRAY),
        new ("Lyrics", "Lyrics", PropertiesObject.PropertiesType.STRING_TEXTAREA),
        new ("Publisher", "Publisher", PropertiesObject.PropertiesType.STRING),
        new ("Remixed By", "RemixedBy", PropertiesObject.PropertiesType.STRING),
        new ("Subtitle", "Subtitle", PropertiesObject.PropertiesType.STRING),
        new ("Title", "Title", PropertiesObject.PropertiesType.STRING),
        new ("Title (Sort by)", "TitleSort", PropertiesObject.PropertiesType.STRING),
        new ("Track", "Track", PropertiesObject.PropertiesType.UINT),
        new ("Track count", "TrackCount", PropertiesObject.PropertiesType.UINT),
        new ("Year", "Year", PropertiesObject.PropertiesType.UINT),

    ];
    }
}