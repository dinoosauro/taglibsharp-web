using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace MetadataChange
{
    /// <summary>
    /// Remove byte arrays when serializing the JSON
    /// </summary>
    /// <param name="AdvancedMetadata">What is being exported</param>
    public class IgnoreByteArrayResolver(MetadataExportType AdvancedMetadata) : DefaultContractResolver
    {
        protected override JsonProperty CreateProperty(System.Reflection.MemberInfo member, MemberSerialization memberSerialization)
        {
            var property = base.CreateProperty(member, memberSerialization);
            // We'll have a lot of if switches here, but we just need to change the Converter to include metadata in various tags. And also, we need to remove byte array references.
            if (property.PropertyName == "FrameId" || (property.PropertyName == "BoxType" && property.PropertyType == typeof(TagLib.ByteVector)))
            {
                property.Converter = new ByteVectorToStringConverter();
            }
            else if (property.PropertyName == "BoxType" && property.PropertyType == typeof(TagLib.ByteVector[]))
            {
                property.Converter = new ByteVectorDoubleArrayToStringConverter();
            }
            else if (property.PropertyType == typeof(IEnumerable<TagLib.Ogg.XiphComment>))
            {
                property.Converter = new XiphCommentConverter(true);

            }
            else if (property.PropertyType == typeof(TagLib.Asf.Tag))
            {
                property.Converter = new AsfCommentConverter();
            }
            else if (property.PropertyType == typeof(Dictionary<string, List<TagLib.Matroska.SimpleTag>>))
            {
                property.Converter = new MatroskaSimpleTagConverter();
            }
            else if (property.PropertyType == typeof(byte[]) || property.PropertyType == typeof(TagLib.ByteVector) || (AdvancedMetadata != MetadataExportType.ALL_APP && property.PropertyType == typeof(TagLib.NonContainer.StartTag)) || (AdvancedMetadata != MetadataExportType.ALL_APP && property.PropertyType == typeof(TagLib.NonContainer.EndTag) || (AdvancedMetadata == MetadataExportType.COMMON && property.PropertyType == typeof(TagLib.Tag[]))))
            {
                property.ShouldSerialize = _ => false;
            }
            else if (property.PropertyType != null && !property.PropertyType.IsPrimitive && property.PropertyType != typeof(string))
            {
                property.Converter = null; // Prevent Newtonsoft from caching wrong converter
            }
            return property;
        }

    }

    /// <summary>
    /// Only fixes serialization of some TagLib libraries. This is done also by the other two ContractResolvers. Without this, metadata values of certain tags might be lost.
    /// It also deletes StartTag and EndTag if not required.
    /// </summary>
    /// <param name="AdvancedMetadata">What is being exported</param>
    public class StandardJsonResolver(MetadataExportType AdvancedMetadata) : DefaultContractResolver
    {
        protected override JsonProperty CreateProperty(System.Reflection.MemberInfo member, MemberSerialization memberSerialization)
        {
            var property = base.CreateProperty(member, memberSerialization);
            if (property.PropertyName == "FrameId" || (property.PropertyName == "BoxType" && property.PropertyType == typeof(TagLib.ByteVector)))
            {
                property.Converter = new ByteVectorToStringConverter();
            }
            else if (property.PropertyName == "BoxType" && property.PropertyType == typeof(TagLib.ByteVector[]))
            {
                property.Converter = new ByteVectorDoubleArrayToStringConverter();
            }
            else if (property.PropertyType == typeof(IEnumerable<TagLib.Ogg.XiphComment>))
            {
                property.Converter = new XiphCommentConverter(false);

            }
            else if (property.PropertyType == typeof(Dictionary<string, List<TagLib.Matroska.SimpleTag>>))
            {
                property.Converter = new MatroskaSimpleTagConverter();
            }
            else if (property.PropertyType == typeof(TagLib.Asf.Tag))
            {
                property.Converter = new AsfCommentConverter();
            }
            else if (AdvancedMetadata != MetadataExportType.ALL_APP && (property.PropertyType == typeof(TagLib.NonContainer.StartTag) || property.PropertyType == typeof(TagLib.NonContainer.EndTag)) || (AdvancedMetadata == MetadataExportType.COMMON && property.PropertyType == typeof(TagLib.Tag[])))
            {
                property.ShouldSerialize = _ => false;
            }
            return property;
        }
    }


    /// <summary>
    /// Convert byte arrays when serializing the JSON
    /// </summary>
    /// <param name="AdvancedMetadata">What is being exported</param>
    public class ByteArrayBase64Resolver(MetadataExportType AdvancedMetadata) : DefaultContractResolver
    {
        protected override JsonProperty CreateProperty(System.Reflection.MemberInfo member, MemberSerialization memberSerialization)
        {
            var property = base.CreateProperty(member, memberSerialization);
            Console.WriteLine(property.PropertyName);
            Console.WriteLine(property.PropertyType);
            if (property.PropertyName == "FrameId" || (property.PropertyName == "BoxType" && property.PropertyType == typeof(TagLib.ByteVector)))
            {
                property.Converter = new ByteVectorToStringConverter();
            }
            else if (property.PropertyName == "BoxType" && property.PropertyType == typeof(TagLib.ByteVector[]))
            {
                property.Converter = new ByteVectorDoubleArrayToStringConverter();
            }
            else if (property.PropertyType == typeof(IEnumerable<TagLib.Ogg.XiphComment>))
            {
                property.Converter = new XiphCommentConverter(false);

            }
            else if (property.PropertyType == typeof(Dictionary<string, List<TagLib.Matroska.SimpleTag>>))
            {
                property.Converter = new MatroskaSimpleTagConverter();
            }
            else if (property.PropertyType == typeof(TagLib.Asf.Tag))
            {
                property.Converter = new AsfCommentConverter();
            }
            else if (AdvancedMetadata != MetadataExportType.ALL_APP && (property.PropertyType == typeof(TagLib.NonContainer.StartTag) || property.PropertyType == typeof(TagLib.NonContainer.EndTag)) || (AdvancedMetadata == MetadataExportType.COMMON && property.PropertyType == typeof(TagLib.Tag[])))
            {
                property.ShouldSerialize = _ => false;
            }
            if (property.PropertyType == typeof(byte[]))
            {
                property.Converter = new ByteArrayToBase64Converter();
            }
            else if (property.PropertyType == typeof(TagLib.ByteVector))
            {
                property.Converter = new TagLibByteVectorToBase64Converter();
            }
            return property;
        }
    }

    /// <summary>
    /// Convert a Byte Array to a base64
    /// </summary>
    public class ByteArrayToBase64Converter : JsonConverter<byte[]>
    {
        public override void WriteJson(JsonWriter writer, byte[] value, JsonSerializer serializer)
        {
            writer.WriteValue(Convert.ToBase64String(value));
        }

        public override byte[] ReadJson(JsonReader reader, Type objectType, byte[] existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            return [];
        }
    }
    /// <summary>
    /// Decode a byte vector to a string
    /// </summary>
    public class ByteVectorToStringConverter : JsonConverter<TagLib.ByteVector>
    {
        public override void WriteJson(JsonWriter writer, TagLib.ByteVector value, JsonSerializer serializer)
        {
            writer.WriteValue(Encoding.UTF8.GetString(value.ToArray()));
        }

        public override TagLib.ByteVector ReadJson(JsonReader reader, Type objectType, TagLib.ByteVector existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            return [];
        }
    }
    /// <summary>
    /// Convert an array of ByteVectors to a String
    /// </summary>
    public class ByteVectorDoubleArrayToStringConverter : JsonConverter<TagLib.ByteVector[]>
    {
        public override void WriteJson(JsonWriter writer, TagLib.ByteVector[] value, JsonSerializer serializer)
        {
            string[] output = value.Select(i => Encoding.UTF8.GetString(i.ToArray())).ToArray();
            writer.WriteValue(output);
        }

        public override TagLib.ByteVector[] ReadJson(JsonReader reader, Type objectType, TagLib.ByteVector[] existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            return [[]];
        }
    }

    /// <summary>
    /// Convert a Matroska SimpleTag to a serializable Dictionary of keys and values.
    /// In case subkeys are there, the key will be a JSON-stringified array with the tag and the subtag.
    /// </summary>
    public class MatroskaSimpleTagConverter : JsonConverter<Dictionary<String, List<TagLib.Matroska.SimpleTag>>>
    {
        public override void WriteJson(JsonWriter writer, Dictionary<String, List<TagLib.Matroska.SimpleTag>> value, JsonSerializer serializer)
        {
            Dictionary<string, string> output = [];
            foreach (var singleTag in value)
            {
                foreach (var tag in singleTag.Value)
                {
                    if (tag.SimpleTags == null) output[singleTag.Key] = Encoding.UTF8.GetString(tag.Value.ToArray()); // Simple key/value metadata
                    else
                    {
                        foreach (var innerTags in tag.SimpleTags) // Metadata with key, subkey, and value
                        {
                            output[Newtonsoft.Json.JsonConvert.SerializeObject(new string[] { singleTag.Key, innerTags.Key })] = Encoding.UTF8.GetString(innerTags.Value.First().Value.ToArray());
                        }
                    }
                }
            }
            serializer.Serialize(writer, output);
        }

        public override Dictionary<string, List<TagLib.Matroska.SimpleTag>> ReadJson(JsonReader reader, Type objectType, Dictionary<String, List<TagLib.Matroska.SimpleTag>> existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            return [];
        }
    }

    /// <summary>
    /// Convert Xiph comments to a serializable dictionary
    /// </summary>
    /// <param name="noBinary"></param>
    public class XiphCommentConverter(bool noBinary) : JsonConverter<IEnumerable<TagLib.Ogg.XiphComment>>
    {
        public override void WriteJson(JsonWriter writer, IEnumerable<TagLib.Ogg.XiphComment> value, JsonSerializer serializer)
        {
            Dictionary<string, string[]> output = [];
            foreach (TagLib.Ogg.XiphComment comment in value)
            {
                foreach (var key in comment)
                {
                    if (key == "METADATA_BLOCK_PICTURE" && noBinary) continue;
                    output[key] = comment.GetField(key);
                }
            }
            serializer.Serialize(writer, output);
        }

        public override IEnumerable<TagLib.Ogg.XiphComment> ReadJson(JsonReader reader, Type objectType, IEnumerable<TagLib.Ogg.XiphComment> existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            return [];
        }
    }

    /// <summary>
    /// Convert ASF tags in a serializable dictionarues
    /// </summary>
    public class AsfCommentConverter : JsonConverter<TagLib.Asf.Tag>
    {
        public override void WriteJson(JsonWriter writer, TagLib.Asf.Tag value, JsonSerializer serializer)
        {
            Dictionary<string, string> output = [];
            foreach (var comment in value) output[comment.Name] = comment.ToString();
            serializer.Serialize(writer, output);
        }

        public override TagLib.Asf.Tag ReadJson(JsonReader reader, Type objectType, TagLib.Asf.Tag existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            return null;
        }
    }



    /// <summary>
    /// Convert a TagLibVector to a Base64 string
    /// </summary>
    public class TagLibByteVectorToBase64Converter : JsonConverter<TagLib.ByteVector>
    {
        public override void WriteJson(JsonWriter writer, TagLib.ByteVector value, JsonSerializer serializer)
        {
            writer.WriteValue(Convert.ToBase64String(value.ToArray()));
        }

        public override TagLib.ByteVector ReadJson(JsonReader reader, Type objectType, TagLib.ByteVector existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            return [];
        }
    }

    /// <summary>
    /// What should be exported in the output JSON/CSV metadata file.
    /// </summary>
    public enum MetadataExportType
    {
        /// <summary>
        /// Common files fetched by TagLib
        /// </summary>
        COMMON = 0,
        /// <summary>
        /// Everything fetched by TagLib
        /// </summary>
        ALL_TAGLIB = 1,
        /// <summary>
        /// Everything fetched by TagLib, but in the container-specific syntax
        /// </summary>
        ALL_TAGLIB_SPECIFIC_ONLY = 2,
        /// <summary>
        /// Everything fetched by TagLib, plus the custom metadata array created by TagLibSharp-Web
        /// </summary>
        ALL_APP = 3
    }

}
