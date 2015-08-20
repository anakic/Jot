using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.ComponentModel;

namespace Ursus.Storage.Serialization
{
    public interface ISerializer
    {
        string Serialize(object obj);
        object Deserialize(string serialized, Type originalType);
    }

    public static class SerializerExtensions
    {
        public static T Deserialize<T>(this ISerializer serializer, string serialized)
        {
            return (T)serializer.Deserialize(serialized, typeof(T));
        }
    }
}
