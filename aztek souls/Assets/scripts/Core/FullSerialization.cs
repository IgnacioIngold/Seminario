using System.IO;
using System.Runtime.Serialization.Formatters.Binary;


namespace Core.Serialization
{
    public static class FullSerialization
    {
        public static void Serialize<T>(this T ob, string path, bool binary = true)
        {
            if (binary)
            {
                BinaryFormatter bf = new BinaryFormatter();
                FileStream file = File.Create(path);
                bf.Serialize(file, ob);
                file.Close();
            }
            else
            {
                StreamWriter file = File.CreateText(path);
                string json = UnityEngine.JsonUtility.ToJson(ob, true);
                file.Write(json);
                file.Close();
            }
        }

        public static T Deserialize<T>(string path, bool binary = true)
        {
            if (File.Exists(path))
            {
                if (binary)
                {
                    FileStream file = File.Open(path, FileMode.Open);
                    BinaryFormatter bf = new BinaryFormatter();
                    T data = (T)bf.Deserialize(file);
                    file.Close();
                    return data;
                }
                else
                {
                    string fileToLoad = File.ReadAllText(path);
                    return UnityEngine.JsonUtility.FromJson<T>(fileToLoad);
                }
            }
            else return default(T);
        }
    }
}
