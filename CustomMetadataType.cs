using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using TagLib;

namespace MetadataChange
{
    /// <summary>
    /// The Class that contains the necessary keys to show and update all the metadata fields of a file.
    /// </summary>
    /// <param name="key">the key of the metadata field</param>
    /// <param name="values">the content of the metadata</param>
    /// <param name="updateEntry">the function to call when the user changes the metadata value</param>
    /// <param name="updateTag">the function called when the user edits the key</param>
    /// <param name="meanstring">the meanstring or the main key, if supported by the container</param>
    /// <param name="updateMeanstring">the function called when the user edits the meanstring</param>
    /// <param name="deleteItem">the function called when the user wants to edit the metadata</param>
    /// <param name="binaryItem">the binary content. If passed, this will be used in the export section instead of the value string</param>
    public class CurrentMetadataStorage(string key, string[] values, Func<string, bool>? updateEntry, Func<string, bool>? updateTag, string? meanstring, Func<string, bool>? updateMeanstring, Func<bool>? deleteItem, byte[]? binaryItem = null)
    {
        /// <summary>
        /// The key of the metadata field
        /// </summary>
        public string key = key;
        /// <summary>
        /// The content of the metadata
        /// </summary>
        public string[] values = values;
        /// <summary>
        /// The function to call when the user changes the metadata value
        /// </summary>
        public Func<string, bool>? updateEntry = updateEntry;
        /// <summary>
        /// The function called when the user edits the key
        /// </summary>
        public Func<string, bool>? updateTag = updateTag;
        /// <summary>
        /// An unique ID of this metadata. Not related in any way to the metadata itself. 
        /// </summary>
        public string id = Guid.NewGuid().ToString();
        /// <summary>
        /// The meanstring or the main key, if supported by the container
        /// </summary>
        public string? meanstring = meanstring;
        /// <summary>
        /// The function called when the user edits the meanstring
        /// </summary>
        public Func<string, bool>? updateMeanstring = updateMeanstring;
        /// <summary>
        /// The function called when the user wants to edit the metadata
        /// </summary>
        public Func<bool>? deleteItem = deleteItem;
        /// <summary>
        /// the binary content. If passed, this will be used in the export section instead of the value string
        /// </summary>
        public byte[]? binaryItem = binaryItem;   
    }
    public static class CustomMetadataUtils
    {
        /// <summary>
        /// Get all the tags in a file
        /// </summary>
        /// <param name="type">the type of the metadata that is being read</param>
        /// <param name="file">the TagLib.File that is being read</param>
        /// <param name="changeCallback">the function to call to rerender the State</param>
        /// <param name="addAlsoBinaryData">if binary data (ex: album art) should be included in the output list</param>
        /// <returns></returns>
        public static CurrentMetadataStorage[] GetAllTags(CustomMetadataFormat.CustomMetadataTypes type, TagLib.File file, Action? changeCallback, bool addAlsoBinaryData = false)
        {
            List<CurrentMetadataStorage> output = new();
            switch (type)
            {
                case CustomMetadataFormat.CustomMetadataTypes.ID3:
                    {
                        if (file.GetTag(TagLib.TagTypes.Id3v2, false) is not TagLib.Id3v2.Tag tag) return [];
                        foreach (var frame in tag)
                        {
                            // ID3 Tags might be of different types, but basically the code is the same.
                            if (frame is TagLib.Id3v2.UserTextInformationFrame userFrame)
                            {
                                output.Add(new CurrentMetadataStorage(userFrame.Description, userFrame.Text, (string output) =>
                                {
                                    userFrame.Text = [output];
                                    return true;
                                }, (string str) =>
                                {
                                    tag.RemoveFrame(userFrame);
                                    TagLib.Id3v2.UserTextInformationFrame.Get(tag, str, true).Text = userFrame.Text;
                                    changeCallback?.Invoke();
                                    return true;
                                }, null, null, () => { tag.RemoveFrame(userFrame); changeCallback?.Invoke(); return true; }));
                            }
                            else if (frame is TagLib.Id3v2.PrivateFrame privateFrame)
                            {
                                if (privateFrame.PrivateData == null) continue;
                                output.Add(new CurrentMetadataStorage(privateFrame.Owner, [Encoding.UTF8.GetString(privateFrame.PrivateData.ToArray())], (string str) =>
                                {
                                    privateFrame.PrivateData = Encoding.UTF8.GetBytes(str);
                                    return true;
                                }, (string str) =>
                                {
                                    tag.RemoveFrame(privateFrame);
                                    TagLib.Id3v2.PrivateFrame.Get(tag, str, true).PrivateData = privateFrame.PrivateData;
                                    changeCallback?.Invoke();
                                    return true;
                                }, null, null, () => { tag.RemoveFrame(privateFrame); changeCallback?.Invoke(); return true; }));
                            }
                            else if (frame is TagLib.Id3v2.CommentsFrame commentFrame)
                            {
                                output.Add(new CurrentMetadataStorage(commentFrame.Description, [commentFrame.Text], (string str) =>
                                {
                                    commentFrame.Text = str;
                                    return true;
                                }, (string str) =>
                                {
                                    tag.RemoveFrame(commentFrame);
                                    TagLib.Id3v2.CommentsFrame comment = TagLib.Id3v2.CommentsFrame.Get(tag, str, commentFrame.Language, true);
                                    comment.Description = commentFrame.Description;
                                    comment.Text = commentFrame.Text;
                                    comment.TextEncoding = commentFrame.TextEncoding;
                                    changeCallback?.Invoke();
                                    return true;
                                }, null, null, () => { tag.RemoveFrame(commentFrame); changeCallback?.Invoke(); return true; }));
                            }
                            else if (frame is TagLib.Id3v2.TextInformationFrame textInfoFrame)
                            {
                                output.Add(new CurrentMetadataStorage(Encoding.UTF8.GetString(textInfoFrame.FrameId.ToArray()), textInfoFrame.Text, (string str) =>
                                {
                                    textInfoFrame.Text = [str];
                                    return true;
                                }, (string str) =>
                                {
                                    tag.RemoveFrame(textInfoFrame);
                                    TagLib.Id3v2.TextInformationFrame text = TagLib.Id3v2.TextInformationFrame.Get(tag, str, true);
                                    text.Text = textInfoFrame.Text;
                                    text.TextEncoding = textInfoFrame.TextEncoding;
                                    changeCallback?.Invoke();
                                    return true;
                                }, null, null, () => { tag.RemoveFrame(textInfoFrame); changeCallback?.Invoke(); return true; }));
                            }
                            else if (frame is TagLib.Id3v2.UnsynchronisedLyricsFrame lyricsFrame)
                            {
                                output.Add(new CurrentMetadataStorage(Encoding.UTF8.GetString(lyricsFrame.FrameId.ToArray()), [lyricsFrame.Text], (string str) =>
                                {
                                    lyricsFrame.Text = str;
                                    return true;
                                }, (string str) =>
                                {
                                    tag.RemoveFrame(lyricsFrame);
                                    TagLib.Id3v2.UnsynchronisedLyricsFrame frame = TagLib.Id3v2.UnsynchronisedLyricsFrame.Get(tag, str, lyricsFrame.Language, true);
                                    frame.Description = lyricsFrame.Description;
                                    frame.Language = lyricsFrame.Language;
                                    frame.Text = lyricsFrame.Text;
                                    frame.TextEncoding = lyricsFrame.TextEncoding;
                                    changeCallback?.Invoke();
                                    return true;
                                }, null, null, () => { tag.RemoveFrame(lyricsFrame); changeCallback?.Invoke(); return true; }));
                            }
                            else if (frame is not TagLib.Id3v2.AttachmentFrame)
                            {
                                output.Add(new CurrentMetadataStorage(Encoding.UTF8.GetString(frame.FrameId.ToArray()), [Encoding.UTF8.GetString(frame.Render(4).Data[6..])], null, null, null, null, () => { tag.RemoveFrame(frame); return true; }));
                            }
                            else
                            {
                                output.Add(new CurrentMetadataStorage(Encoding.UTF8.GetString(frame.FrameId.ToArray()), ["Binary data"], null, null, null, null, null, ((TagLib.Id3v2.AttachmentFrame)frame).Data.ToArray()));
                            }

                            // TODO: We could also have a look to chapters (ChapterFrame)...
                        }
                        break;
                    }
                case CustomMetadataFormat.CustomMetadataTypes.APPLE:
                    {
                        if (file.GetTag(TagLib.TagTypes.Apple, false) is TagLib.Mpeg4.AppleTag appleTag)
                        {
                            foreach (var tag in appleTag.ToArray())
                            {
                                if (tag.BoxType == null) continue;
                                if (tag.Children.Count() == 1) // Standard metadata, with no meanspace.
                                {
                                    if (tag.BoxType == "covr")
                                    {
                                        if (!addAlsoBinaryData) continue;
                                        output.Add(new CurrentMetadataStorage(Encoding.UTF8.GetString(tag.BoxType.ToArray()), ["Binary data"], null, null, null, null, null, tag.Children.First().Data.ToArray()));
                                        continue;
                                    }
                                    output.Add(new CurrentMetadataStorage(Encoding.UTF8.GetString(tag.BoxType.ToArray()), [Encoding.UTF8.GetString(tag.Children.First().Data.ToArray())], (string str) =>
                                    {
                                        tag.Children.First().Data = Encoding.UTF8.GetBytes(str);
                                        return true;
                                    }, (string str) =>
                                    {
                                        byte[] prevData = tag.Data.ToArray();
                                        appleTag.SetText(tag.BoxType, "");
                                        if (str.Length == 3 || str.Length == 4) appleTag.SetText(Encoding.UTF8.GetBytes(str), Encoding.UTF8.GetString(tag.Data.ToArray())); else appleTag.SetDashBox("---", str, Encoding.UTF8.GetString(prevData));
                                        changeCallback?.Invoke();
                                        return true;
                                    }, null, null, () => { appleTag.SetText(tag.BoxType, ""); changeCallback?.Invoke(); return true; }));
                                }
                                else if (tag.Children.Count() > 2) // Custom metadata with meanstring: the chiildren is an array composed of [meanstring, key, value]
                                {
                                    var arr = tag.Children.ToArray();
                                    string meanstring = Encoding.UTF8.GetString(arr[0].Data.ToArray());
                                    string key = Encoding.UTF8.GetString(arr[1].Data.ToArray());
                                    string value = Encoding.UTF8.GetString(arr[2].Data.ToArray());
                                    output.Add(new CurrentMetadataStorage(key, [value], (string str) =>
                                    {
                                        tag.Children.Last().Data = Encoding.UTF8.GetBytes(str);
                                        value = str;
                                        return true;
                                    }, (string str) =>
                                    {
                                        appleTag.SetDashBox(meanstring, key, "");
                                        appleTag.SetDashBox(meanstring, str, value);
                                        key = str;
                                        return true;
                                    }, meanstring, (string str) =>
                                    {
                                        appleTag.SetDashBox(meanstring, key, "");
                                        appleTag.SetDashBox(str, key, value);
                                        meanstring = str;
                                        return true;
                                    }, () => { appleTag.SetDashBox(meanstring, key, ""); changeCallback?.Invoke(); return true; }));
                                }
                            }
                        }
                        break;
                    }
                case CustomMetadataFormat.CustomMetadataTypes.XIPH:
                    {
                        if (file.GetTag(TagLib.TagTypes.Xiph, false) is not TagLib.Ogg.XiphComment tag) return [];
                        foreach (var frame in tag)
                        {
                            if (frame == "METADATA_BLOCK_PICTURE" && !addAlsoBinaryData) continue;
                            string key = frame;
                            output.Add(new CurrentMetadataStorage(key, tag.GetField(frame), (string str) =>
                            {
                                tag.RemoveField(key);
                                tag.SetField(key, [str]);
                                return true;
                            }, (string str) =>
                            {
                                string[] value = tag.GetField(key);
                                tag.RemoveField(key);
                                tag.SetField(str, value);
                                key = str;
                                return true;
                            }, null, null, () => { tag.RemoveField(key); changeCallback?.Invoke(); return true; }));
                        }
                        break;
                    }
                case CustomMetadataFormat.CustomMetadataTypes.MATROSKA:
                    {
                        var tag = file.GetTag(TagLib.TagTypes.Matroska, false) as TagLib.Matroska.Tag;
                        if (tag == null) return [];
                        foreach (var frame in tag.SimpleTags)
                        {
                            foreach (var singleTag in frame.Value)
                            {
                                if (singleTag.SimpleTags == null) // Simple key-value string, without any subkeys.
                                {
                                    output.Add(new CurrentMetadataStorage(frame.Key, [Encoding.UTF8.GetString(singleTag.Value.ToArray())], (string str) =>
                                    {
                                        singleTag.Value = Encoding.UTF8.GetBytes(str);
                                        return true;
                                    }, (string str) =>
                                    {
                                        string value = Encoding.UTF8.GetString(singleTag.Value.ToArray());
                                        tag.Remove(frame.Key);
                                        tag.Set(frame.Key, null, value);
                                        changeCallback?.Invoke();
                                        return true;
                                    }, "", (string str) =>
                                    {
                                        if (str != "")
                                        {
                                            string value = Encoding.UTF8.GetString(singleTag.Value.ToArray());
                                            tag.Remove(frame.Key);
                                            tag.Set(str, frame.Key, value);
                                            changeCallback?.Invoke();
                                        }
                                        return true;

                                    }, () => { tag.Remove(frame.Key); changeCallback?.Invoke(); return true; }));
                                }
                                else // Key-subkey-value syntax
                                {
                                    /// <summary>
                                    /// A list composed of the meastring name, and the number of times it's added.
                                    /// This is done since TagLibSharp automatically creates an empty key for the meanstring. If there are no longer children of that key (so, the value of the meanstring is 0), it should be deleted too.
                                    /// </summary>
                                    Dictionary<string, int> meanstringEntries = new Dictionary<string, int>();
                                    foreach (var subtags in singleTag.SimpleTags)
                                    {
                                        if (subtags.Value.Count() == 0) continue; // Empty main key
                                        if (!meanstringEntries.ContainsKey(frame.Key)) meanstringEntries[frame.Key] = 0;
                                        meanstringEntries[frame.Key]++;
                                        output.Add(new CurrentMetadataStorage(subtags.Key, [Encoding.UTF8.GetString(subtags.Value.First().Value.ToArray())], (string str) =>
                                        {
                                            subtags.Value.First().Value = Encoding.UTF8.GetBytes(str);
                                            return true;
                                        }, (string str) =>
                                        {
                                            string value = Encoding.UTF8.GetString(subtags.Value.First().Value.ToArray());
                                            tag.Remove(frame.Key, subtags.Key);
                                            tag.Set(frame.Key, subtags.Key, value);
                                            changeCallback?.Invoke();
                                            return true;
                                        }, frame.Key, (string str) =>
                                        {
                                            string value = Encoding.UTF8.GetString(subtags.Value.First().Value.ToArray());
                                            tag.Remove(frame.Key, subtags.Key);
                                            meanstringEntries[frame.Key]--;
                                            if (meanstringEntries[frame.Key] == 0) tag.Remove(frame.Key);
                                            tag.Set(str == "" ? subtags.Key : str, str == "" ? null : subtags.Key, value);
                                            changeCallback?.Invoke();
                                            return true;
                                        }, () =>
                                        {
                                            tag.Remove(frame.Key, subtags.Key);
                                            meanstringEntries[frame.Key]--;
                                            if (meanstringEntries[frame.Key] == 0) tag.Remove(frame.Key);
                                            changeCallback?.Invoke();
                                            return true;
                                        }));
                                    }
                                }
                            }
                        }
                        break;
                    }
                case CustomMetadataFormat.CustomMetadataTypes.ASF:
                    {
                        if (file.GetTag(TagLib.TagTypes.Asf, false) is not TagLib.Asf.Tag tag) return [];
                        foreach (var frame in tag)
                        {
                            string key = frame.Name;
                            output.Add(new CurrentMetadataStorage(key, [frame.ToString()], (string str) =>
                            {
                                tag.SetDescriptorString(str, [key]);
                                return true;
                            }, (string str) =>
                            {
                                string value = frame.ToString();
                                tag.RemoveDescriptors(key);
                                tag.SetDescriptorString(value, [str]);
                                changeCallback?.Invoke();
                                return true;
                            }, null, null, () => { tag.RemoveDescriptors(key); changeCallback?.Invoke(); return true; }));
                        }
                        break;
                    }
                case CustomMetadataFormat.CustomMetadataTypes.APE: // Untested
                    {
                        if (file.GetTag(TagLib.TagTypes.Ape, false) is not TagLib.Ape.Tag tag) return [];
                        foreach (var frame in tag)
                        {
                            string key = frame;
                            output.Add(new CurrentMetadataStorage(key, [Encoding.UTF8.GetString(tag.GetItem(frame).Value.ToArray())], (string str) =>
                            {
                                tag.RemoveItem(frame);
                                tag.SetItem(new TagLib.Ape.Item(frame, str));
                                return true;
                            }, (string str) =>
                            {
                                var value = tag.GetItem(frame).ToString();
                                tag.RemoveItem(frame);
                                tag.SetItem(new TagLib.Ape.Item(str, value));
                                changeCallback?.Invoke();
                                return true;
                            }, null, null, () => { tag.RemoveItem(frame); changeCallback?.Invoke(); return true; }));
                        }
                        break;
                    }
                case CustomMetadataFormat.CustomMetadataTypes.PNG: // Untested
                    {
                        if (file.GetTag(TagLib.TagTypes.Png, false) is not TagLib.Png.PngTag tag) return [];
                        foreach (var frame in tag)
                        {
                            if (frame is KeyValuePair<String, String> innerTag)
                            {
                                string key = innerTag.Key;
                                output.Add(new CurrentMetadataStorage(key, [innerTag.Value], (string str) =>
                                {
                                    tag.SetKeyword(key, str);
                                    return true;
                                }, null, null, null, null));
                            }
                        }
                        break;
                    }
            }
            return output.ToArray();
        }

        /// <summary>
        /// The void that adds the custom metadata to the supported containers
        /// <param name="type">The CustomMetadataTypes enum of the container</param>
        /// <param name="file">The TagLib.File where the new metadata will be applied</param>
        /// </summary>
        public static void UpdateCustomValueHandle(CustomMetadataFormat.CustomMetadataTypes type, TagLib.File file, string key, string value, string meanstring, string mp3Type)
        {
            switch (type)
            {
                case CustomMetadataFormat.CustomMetadataTypes.APE:
                    {
                        TagLib.Ape.Tag tag = (TagLib.Ape.Tag)file.GetTag(TagLib.TagTypes.Ape, true);
                        tag.SetValue(key, value);
                        break;
                    }
                case CustomMetadataFormat.CustomMetadataTypes.APPLE:
                    {
                        TagLib.Mpeg4.AppleTag tag = (TagLib.Mpeg4.AppleTag)file.GetTag(TagLib.TagTypes.Apple, true);
                        tag.SetDashBox(meanstring, key, value);
                        break;
                    }
                case CustomMetadataFormat.CustomMetadataTypes.ASF:
                    {
                        TagLib.Asf.Tag tag = (TagLib.Asf.Tag)file.GetTag(TagLib.TagTypes.Asf, true);
                        tag.SetDescriptorString(value, [key]);
                        break;
                    }
                case CustomMetadataFormat.CustomMetadataTypes.ID3:
                    {
                        TagLib.Id3v2.Tag metadata = (TagLib.Id3v2.Tag)file.GetTag(TagLib.TagTypes.Id3v2, true);
                        switch (mp3Type)
                        {
                            case "1":
                                {
                                    TagLib.Id3v2.PrivateFrame privateFrame = TagLib.Id3v2.PrivateFrame.Get(metadata, key, true);
                                    privateFrame.PrivateData = Encoding.UTF8.GetBytes(value);
                                    break;
                                }
                            case "2":
                                {
                                    TagLib.Id3v2.CommentsFrame comments = TagLib.Id3v2.CommentsFrame.Get(metadata, key, "en", true);
                                    comments.Text = value;
                                    break;
                                }
                            default:
                                {
                                    TagLib.Id3v2.UserTextInformationFrame userText = TagLib.Id3v2.UserTextInformationFrame.Get(metadata, key, true);
                                    userText.Text = [value];
                                    break;
                                }
                        }
                        break;
                    }
                case CustomMetadataFormat.CustomMetadataTypes.MATROSKA:
                    {

                        TagLib.Matroska.Tag tag = (TagLib.Matroska.Tag)file.GetTag(TagLib.TagTypes.Matroska, true);
                        tag.Set(key, meanstring == "" ? null : meanstring, value);
                        break;
                    }
                case CustomMetadataFormat.CustomMetadataTypes.XIPH:
                    {
                        TagLib.Ogg.XiphComment tag = (TagLib.Ogg.XiphComment)file.GetTag(TagLib.TagTypes.Xiph, true);
                        tag.SetField(key, [value]);
                        break;
                    }
                case CustomMetadataFormat.CustomMetadataTypes.PNG:
                    {
                        TagLib.Png.PngTag tag = (TagLib.Png.PngTag)file.GetTag(TagLib.TagTypes.Png, true);
                        tag.SetKeyword(key, value);
                        break;
                    }
                case CustomMetadataFormat.CustomMetadataTypes.RIFF:
                    {
                        TagLib.Riff.MovieIdTag tag = (TagLib.Riff.MovieIdTag)file.GetTag(TagLib.TagTypes.MovieId, true);
                        tag.SetValue(TagLib.ByteVector.FromString(key), new TagLib.ByteVectorCollection(){
TagLib.ByteVector.FromString(value)
});
                        break;
                    }
                case CustomMetadataFormat.CustomMetadataTypes.XMP:
                    {
                        TagLib.Xmp.XmpTag tag = (TagLib.Xmp.XmpTag)file.GetTag(TagLib.TagTypes.XMP, true);
                        tag.SetTextNode(meanstring, key, value);
                        break;
                    }
            }
        }
    }
    public class CustomMetadataFormat
    {
        /// <summary>
        /// Gets the container TagLibSharp has identified for the current file
        /// </summary>
        /// <param name="type">The Type of the TagLib.Tag</param>
        /// <returns>A CustomMetadataType of the container, if a supported container for custom metadata is available</returns>
        public static CustomMetadataTypes? GetSuggestedContainer(string type)
        {
            string[] permittedTypes = System.Enum.GetNames(typeof(CustomMetadataTypes));
            foreach (string availableType in permittedTypes)
            {
                if (type.Contains(availableType.ToLower())) return (CustomMetadataTypes)Enum.Parse(typeof(CustomMetadataTypes),
                availableType);
            }
            return null;
        }
        /// <summary>
        /// The different types of container that support custom metadata
        /// </summary>
        public enum CustomMetadataTypes
        {
            APE,
            APPLE,
            ASF,
            ID3,
            MATROSKA,
            XIPH,
            PNG,
            RIFF,
            XMP
        }
        /// <summary>
        /// A list of the metadata keys that TagLib Sharp automatically copies after running tag.CopyTo()
        /// </summary>
        public static Dictionary<CustomMetadataTypes, string[]> CopiedProps = new(){
            {CustomMetadataTypes.ID3 , ["TIT2", "TIT3", "Description", "TPE2", "TPE1", "TCOM", "TALB", "COMM", "TCON", "TDRC", "TRCK", "TPOS", "TBPM", "TKEY", "TPUB", "TSRC", "TPE4", "TIT1", "TPE3", "TCOP", "TDTG", "APIC", "TMCL", "TSO2", "TSOA", "TSOC", "USLT", "TSOT", "TMCL"]},
            {CustomMetadataTypes.APPLE, ["�too", "�nam", "Subt", "desc", "aART", "�ART", "role", "�wrt", "�alb", "�cmt", "�gen", "�day", "trkn", "disk", "tmpo", "initialkey", "publisher", "ISRC", "REMIXEDBY", "�grp", "cond", "cprt", "dtag", "covr", "soaa", "soal", "soco", "�lyr", "sonm"]},
            {CustomMetadataTypes.XIPH, ["ENCODER", "TITLE", "SUBTITLE", "DESCRIPTION", "ALBUMARTIST", "ARTIST", "ARTISTROLE", "COMPOSER", "ALBUM", "COMMENT", "GENRE", "DATE", "TRACKNUMBER", "TRACKTOTAL", "DISCNUMBER", "TEMPO", "GROUPING", "CONDUCTOR", "COPYRIGHT", "DATETAGGED", "METADATA_BLOCK_PICTURE", "ALBUMARTISTSORT", "ALBUMSORT", "COMPOSERSORT", "LYRICS", "TITLESORT"]},
            {CustomMetadataTypes.MATROSKA, ["ENCODER", "TITLE", "SUBTITLE", "SUMMARY", "ARTIST", "PERFORMER", "COMPOSER", "COMMENT", "GENRE", "DATE_RECORDED", "PART_NUMBER", "BPM", "GROUPING", "CONDUCTOR", "COPYRIGHT", "DATE_TAGGED", "LYRICS"]},
            {CustomMetadataTypes.ASF, ["WM/EncodingSettings", "WM/SubTitle", "WM/AlbumArtist", "WM/Composer", "WM/AlbumTitle", "WM/Text", "WM/Genre", "WM/Year", "WM/TrackNumber", "TrackTotal", "WM/PartOfSet", "WM/BeatsPerMinute", "WM/ContentGroupDescription", "WM/Conductor", "WM/AlbumArtistSortOrder", "WM/AlbumSortOrder", "WM/Lyrics", "WM/TitleSortOrder"]}
        };


    }
}