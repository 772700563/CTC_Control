using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace CTC_Control.Classes.Helper
{
    class SerializeHelper
    {       
        /// <summary>
        /// 序列化
        /// </summary>
        public static bool SerializeNow<T>(T toSaveClass, string filePath)
        {
            bool temp = false;

            FileStream fileStream = new FileStream(filePath, FileMode.Create);
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            binaryFormatter.Serialize(fileStream, toSaveClass);
            fileStream.Close();
            temp = true;
            return temp;
        }

        /// <summary>
        /// 反序列化
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static T DeSerializeNow<T>(string filePath) where T : class
        {
            FileStream fileStream = new FileStream(filePath, FileMode.Open);
            BinaryFormatter formatter = new BinaryFormatter();
            T obj = formatter.Deserialize(fileStream) as T;

            fileStream.Close();
            return obj;
        }
    }
}
