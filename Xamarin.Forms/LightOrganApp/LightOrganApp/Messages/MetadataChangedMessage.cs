using LightOrganApp.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightOrganApp.Messages
{
    public class MetadataChangedMessage
    {
        public MediaMetadata Metadata { get; set; }
    }
}
