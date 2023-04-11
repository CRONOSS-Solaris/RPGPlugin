//using System.Collections.Generic;
//using System.Xml.Serialization;
//using Torch;

//namespace RPGPlugin
//{
//    public class MinerConfig : ViewModel
//    {
//        private Dictionary<string, double> _expRatio;

//        [XmlIgnore] // Ignores the dictionary when serializing XML
//        public Dictionary<string, double> ExpRatio
//        {
//            get => _expRatio;
//            set => SetValue(ref _expRatio, value);
//        }

//        public MinerConfig()
//        {
//            _expRatio = new Dictionary<string, double>()
//            {
//                ["Stone"] = 0.2,
//                ["Silicon"] = 0.3,
//                ["Iron"] = 0.3,
//                ["Nickel"] = 0.3,
//                ["Cobalt"] = 0.4,
//                ["Magnesium"] = 0.4,
//                ["Silver"] = 0.5,
//                ["Gold"] = 0.5,
//                ["Platinum"] = 0.55,
//                ["Uranium"] = 0.6,
//                ["Ice"] = 0.35
//            };
//        }

//        [XmlArray("ExpRatioList")] // Property serialized as a list
//        [XmlArrayItem("ExpRatioEntry")] // List items are serialized as dictionary entries
//        public List<KeyValuePair<string, double>> ExpRatioList
//        {
//            get
//            {
//                return new List<KeyValuePair<string, double>>(_expRatio);
//            }
//            set
//            {
//                if (value != null)
//                {
//                    _expRatio = new Dictionary<string, double>((IDictionary<string, double>)value);
//                }
//            }
//        }
//    }
//}
