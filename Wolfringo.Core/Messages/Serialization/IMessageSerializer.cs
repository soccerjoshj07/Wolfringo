﻿using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace TehGM.Wolfringo.Messages.Serialization
{
    public interface IMessageSerializer
    {
        JToken Serialize(IWolfMessage message);
        IWolfMessage Deserialize(string command, JToken payload, IEnumerable<byte[]> binaryMessages);
    }
}
