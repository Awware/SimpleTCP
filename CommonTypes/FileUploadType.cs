using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonTypes
{
    public class FileUploadType
    {
        public FileUploadType(byte[] file, string name)
        {
            File = file;
            Name = name;
        }
        [JsonProperty("File")]
        public byte[] File { get; }
        [JsonProperty("FName")]
        public string Name { get; }
    }
}
