using System.Reflection;

/// <summary>
/// The Class that permits to get or set a value onto a Tag
/// </summary>
namespace MetadataChange
{

    public static class TagValueHandler
    {
        /// <summary>
        /// Set a Tag to a TagLib.Tag
        /// </summary>
        /// <param name="value">The value of the tag</param>
        /// <param name="destinationTag">The TagLib.Tag where the new value should be added</param>
        /// <param name="selectedItem">The Tag to update</param>
        public static void SetCurrentValue(string? value, TagLib.Tag destinationTag, AvailableProperties.PropertiesObject selectedItem)
        {
            PropertyInfo propertyInfo = destinationTag.GetType().GetProperty(selectedItem.TagLibProperty);
            if (propertyInfo != null && propertyInfo.CanWrite)
            {
                object? getValue = propertyInfo.GetValue(destinationTag, null);
                switch (getValue == null ? "" : getValue.GetType().ToString()) // Convert it to the right object for TagLib Sharp
                {
                    case "System.String[]":
                        propertyInfo.SetValue(destinationTag, value == null ? null : new string[] { value }, null);
                        break;
                    case "System.UInt32":
                        uint tempInt;
                        propertyInfo.SetValue(destinationTag, uint.TryParse(value, out tempInt) ? tempInt : null, null);
                        break;
                    default:
                        propertyInfo.SetValue(destinationTag, value, null);
                        break;
                }
            }
        }
        /// <summary>
        /// Set a Tag to a TagLib.Tag
        /// </summary>
        /// <param name="value">The value of the tag</param>
        /// <param name="files">A list of the files to update</param>
        /// <param name="selectedItem">The Tag to update</param>
        public static void SetCurrentValue(string? value, List<TagInfoContainer> files, AvailableProperties.PropertiesObject selectedItem)
        {
            foreach (TagInfoContainer file in files) SetCurrentValue(value, file.tag.Tag, selectedItem);
        }
        /// <summary>
        /// Get the value of a tag from a TagLib.File
        /// </summary>
        /// <param name="propertyName">The name of the property to get</param>
        /// <param name="source">The TagInfoContainer where the item will be fetched</param>
        /// <returns></returns>
        public static string GetCurrentValue(string propertyName, TagInfoContainer source)
        {
            PropertyInfo propertyInfo = source.tag.Tag.GetType().GetProperty(propertyName);
            if (propertyInfo == null) return "";
            var val = propertyInfo.GetValue(source.tag.Tag, null);
            if (val == null) return "";
            return val.GetType().ToString() switch
            {
                "System.String[]" => string.Join(", ", ((string[])val)),
                _ => val.ToString(),
            };
        }

    }
}