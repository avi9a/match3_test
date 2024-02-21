// using UnityEngine;
// using System.IO;
// using System.Runtime.Serialization.Formatters.Binary;
//
// public static class SaveSystem
// {
//     public static void SaveBlock(Cube block)
//     {
//         // Debug.Log("Save");
//         BinaryFormatter formatter = new BinaryFormatter();
//         string path = Application.persistentDataPath + "/block.pos";
//         FileStream stream = new FileStream(path, FileMode.Create);
//         GameData data = new GameData(block);
//         
//         formatter.Serialize(stream, data);
//         stream.Close();
//     }
//
//     public static GameData LoadBlock()
//     {
//         string path = Application.persistentDataPath + "/block.pos";
//         if (File.Exists(path))
//         {
//             // Debug.Log("Load");
//             BinaryFormatter formatter = new BinaryFormatter();
//             FileStream stream = new FileStream(path, FileMode.Open);
//             GameData data = formatter.Deserialize(stream) as GameData;
//             stream.Close();
//             return data;
//         }
//         else
//         {
//             Debug.LogError("Save file not found " + path);
//             return null;
//         }
//     }
// }
