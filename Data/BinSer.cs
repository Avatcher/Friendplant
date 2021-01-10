using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace Friendplant {
    class BinSer {
        static public void Serialize<Object>(Object dictionary, Stream stream) {
            using(stream) {
                BinaryFormatter bin = new BinaryFormatter();
                bin.Serialize(stream, dictionary);
            }
        }
        static public Object Deserialize<Object>(Stream stream) where Object : new() {
            Object ret = Activator.CreateInstance<Object>();
            using(stream) {
                BinaryFormatter bin = new BinaryFormatter();
                ret = (Object)bin.Deserialize(stream);
            }
            return ret;
        }
    }
}