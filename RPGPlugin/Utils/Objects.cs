using System;

namespace RPGPlugin.Utils
{
    public sealed class ExperienceAction
    {
        public long ownerID { get; set; }
        public string subType { get; set; }
        public double amount { get; set; }
    }
    
    public class SerializableTuple<T1,T2>
    {
        public T1 Item1 { get; set; }
        public T2 Item2 { get; set; }
 
        public static implicit operator Tuple<T1,T2>(SerializableTuple<T1,T2>  st)
        {
            return Tuple.Create(st.Item1,st.Item2);
        }

        public static implicit operator SerializableTuple<T1,T2>(Tuple<T1,T2> t)
        {
            return new SerializableTuple<T1,T2>() {
                Item1 = t.Item1,
                Item2 = t.Item2
            };   
        }
 
        public SerializableTuple()
        {
        }
    }
}