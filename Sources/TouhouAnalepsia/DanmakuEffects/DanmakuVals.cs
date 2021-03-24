using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using TaleWorlds.Core;

namespace TouhouAnalepsia
{
    // XML reader vals
    [Serializable]
    public class DanmakuVals
    {
        [XmlElement]
        public string Id { get; set; }

        [XmlElement]
        public string DanmakuNum { get; set; }

        [XmlElement]
        public string DanmakuDisp { get; set; }

        [XmlElement]
        public string DanmakuType { get; set; }
    }

    // XML List
    [Serializable]
    public class DanmakuValsList
    {
        [XmlArray("Data")]
        public List<DanmakuVals> Data { get; set; }
    }

    // In mission object
    public struct DanmakuItem
    {
        public ItemObject Item { get; set; }
        public int DanmakuNum { get; set; }
        public float DanmakuDisp { get; set; }
        public DanmakuType Type { get; set; }
    }

    public enum DanmakuType
    {
        spread,
        burst
    }
}
