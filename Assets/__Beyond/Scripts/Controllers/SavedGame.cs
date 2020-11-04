/*
Vector3 serialisation and surrogate code taken from Cherno in https://answers.unity.com/questions/956047/serialize-quaternion-or-vector3.html
*/

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;


namespace Beyond
{
    
    [System.Serializable]
    public class SavedGame
    {
        public Place place ;
        public Vector3 fp_position ;
        public Quaternion fp_rotation ;
        public Quaternion fplook_rotation ;
        public List<SavedComponent> components ;

        public string ShowMe()
        {
            string s = string.Format("Place {0}. W={1} , L={2} , H={3} (min={4}) in hemispehre {5}. ",place.name  , place.Width , place.Length , place.Height , place.LowestY , place.hemisphere);
            s+=string.Format("FP position = [{0};{1};{2}]\n",fp_position[0],fp_position[1],fp_position[2]);
            s+=string.Format("Gametime={0}", place.gametime.DateStr());
            s+=string.Format(", Number of groups found={0}: ",place.beyondGroups.Count);
            foreach (BeyondGroup bg in place.beyondGroups)
            {
                s+=bg.name+", ";
            }
            return s ;
        }

        public static SavedGame CreateSavedGame()
        {
            SavedGame save = new SavedGame();
            save.place = PlaceController.Instance.place ;
            FirstPersonController.Instance.Save(ref save) ;
            FirstPersonMouseLook.Instance.Save(ref save) ;
            // Place has a list of BeyondGroup, but they don't save their components (can't serialize monobehaviour)
            // Do it by hand
            save.components = new List<SavedComponent>() ;
            foreach(BeyondGroup bg in save.place.beyondGroups)
            {
                foreach(BeyondComponent bc in bg.componentList)
                {
                    SavedComponent sc = new SavedComponent(bc) ;
                    save.components.Add(sc) ;
                }
            }
            return save ;
        }

        public static void Save() 
        {
            SavedGame save = CreateSavedGame();
            BinaryFormatter bf = new BinaryFormatter();
            // GetSurrogateSelector() adds the ability to serialize Vector3, Vector3Int and Quaternion
            bf.SurrogateSelector = GetSurrogateSelector();

            FileStream file = File.Create (Application.persistentDataPath + "/savedGame.bg");
            bf.Serialize(file, save);
            file.Close();

            Debug.Log("Game Saved");
        }

        public static void Load() 
        {
            if(File.Exists(Application.persistentDataPath + "/savedGame.bg"))
            {
                BinaryFormatter bf = new BinaryFormatter();
                // GetSurrogateSelector() adds the ability to serialize Vector3, Vector3Int and Quaternion
                bf.SurrogateSelector = GetSurrogateSelector();

                FileStream file = File.Open(Application.persistentDataPath + "/savedGame.bg", FileMode.Open);
                SavedGame savedGame = (SavedGame)bf.Deserialize(file);
                file.Close();

                FirstPersonController.Instance.Load(savedGame) ;
                FirstPersonMouseLook.Instance.Load(savedGame);
                PlaceController.Instance.Load(savedGame) ;
                Debug.Log("Game Loaded"+savedGame.ShowMe());
            }
            else
            {
                Debug.Log("No saved game file found.");
            }
        }

        public static SurrogateSelector GetSurrogateSelector()
        { // Construct a SurrogateSelector object to serialize normally non serializable classes: Vector3, Vector3Int, Quaternion
            SurrogateSelector ss = new SurrogateSelector();
            Vector3SerializationSurrogate v3ss = new Vector3SerializationSurrogate();
            Vector3IntSerializationSurrogate v3iss = new Vector3IntSerializationSurrogate();
            QuaternionSerializationSurrogate qss = new QuaternionSerializationSurrogate();
            ss.AddSurrogate(typeof(Vector3), new StreamingContext(StreamingContextStates.All), v3ss);
            ss.AddSurrogate(typeof(Vector3Int), new StreamingContext(StreamingContextStates.All), v3iss);
            ss.AddSurrogate(typeof(Quaternion), new StreamingContext(StreamingContextStates.All), qss);
            return ss ;
        }
    }


    /* ====================================================================================================
    * 
    *  Utility structures & classes to serialize Vector3 and Quaternions
    *
    *  ====================================================================================================*/
 
    /// <summary>
    /// Since unity doesn't flag the Vector3 as serializable, we
    /// need to create our own version. This one will automatically convert
    /// between Vector3 and SerializableVector3
    /// </summary>

    [System.Serializable]
    public struct SerializableVector3
    {
        public float x;
        public float y;
        public float z;
        
        public SerializableVector3(float rX, float rY, float rZ)
        {
            x = rX;
            y = rY;
            z = rZ;
        }
        
        public override string ToString()
        {
            return String.Format("[{0}, {1}, {2}]", x, y, z);
        }
        
        public static implicit operator Vector3(SerializableVector3 rValue)
        {
            return new Vector3(rValue.x, rValue.y, rValue.z);
        }
        
        public static implicit operator SerializableVector3(Vector3 rValue)
        {
            return new SerializableVector3(rValue.x, rValue.y, rValue.z);
        }
    }

    public struct SerializableVector3Int
    {
        public int x;
        public int y;
        public int z;
        
        public SerializableVector3Int(int rX, int rY, int rZ)
        {
            x = rX;
            y = rY;
            z = rZ;
        }
        
        public override string ToString()
        {
            return String.Format("[{0}, {1}, {2}]", x, y, z);
        }
        
        public static implicit operator Vector3Int(SerializableVector3Int rValue)
        {
            return new Vector3Int(rValue.x, rValue.y, rValue.z);
        }
        
        public static implicit operator SerializableVector3Int(Vector3Int rValue)
        {
            return new SerializableVector3Int(rValue.x, rValue.y, rValue.z);
        }
    }

    [System.Serializable]
    public struct SerializableQuaternion
    {
        public float x;
        public float y;
        public float z;
        public float w;
        
        public SerializableQuaternion(float rX, float rY, float rZ, float rW)
        {
            x = rX;
            y = rY;
            z = rZ;
            w = rW;
        }
        
        public override string ToString()
        {
            return String.Format("[{0}, {1}, {2}, {3}]", x, y, z, w);
        }
        
        public static implicit operator Quaternion(SerializableQuaternion rValue)
        {
            return new Quaternion(rValue.x, rValue.y, rValue.z, rValue.w);
        }
        
        public static implicit operator SerializableQuaternion(Quaternion rValue)
        {
            return new SerializableQuaternion(rValue.x, rValue.y, rValue.z, rValue.w);
        }
    }


    sealed class Vector3SerializationSurrogate : ISerializationSurrogate 
    {

        // Method called to serialize a Vector3 object
        public void GetObjectData(System.Object obj, SerializationInfo info, StreamingContext context) 
        {
            Vector3 v3 = (Vector3) obj;
            info.AddValue("x", v3.x);
            info.AddValue("y", v3.y);
            info.AddValue("z", v3.z);
            Debug.Log(v3);
        }
        
        // Method called to deserialize a Vector3 object
        public System.Object SetObjectData(System.Object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector) 
        {
            Vector3 v3 = (Vector3) obj;
            v3.x = (float)info.GetValue("x", typeof(float));
            v3.y = (float)info.GetValue("y", typeof(float));
            v3.z = (float)info.GetValue("z", typeof(float));
            obj = v3;
            return obj;   // Formatters ignore this return value //Seems to have been fixed!
        }
    }

    sealed class Vector3IntSerializationSurrogate : ISerializationSurrogate 
    {

        // Method called to serialize a Vector3 object
        public void GetObjectData(System.Object obj, SerializationInfo info, StreamingContext context) 
        {
            Vector3Int v3 = (Vector3Int) obj;
            info.AddValue("x", v3.x);
            info.AddValue("y", v3.y);
            info.AddValue("z", v3.z);
            Debug.Log(v3);
        }
        
        // Method called to deserialize a Vector3 object
        public System.Object SetObjectData(System.Object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector) 
        {
            Vector3Int v3 = (Vector3Int) obj;
            v3.x = (int)info.GetValue("x", typeof(int));
            v3.y = (int)info.GetValue("y", typeof(int));
            v3.z = (int)info.GetValue("z", typeof(int));
            obj = v3;
            return obj;   // Formatters ignore this return value //Seems to have been fixed!
        }
    }

    sealed class QuaternionSerializationSurrogate : ISerializationSurrogate 
    {

        // Method called to serialize a Vector3 object
        public void GetObjectData(System.Object obj, SerializationInfo info, StreamingContext context) 
        {
            Quaternion q = (Quaternion) obj;
            info.AddValue("x", q.x);
            info.AddValue("y", q.y);
            info.AddValue("z", q.z);
            info.AddValue("w", q.w);
            Debug.Log(q);
        }
        
        // Method called to deserialize a Vector3 object
        public System.Object SetObjectData(System.Object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector) 
        {
            Quaternion q = (Quaternion) obj;
            q.x = (float)info.GetValue("x", typeof(float));
            q.y = (float)info.GetValue("y", typeof(float));
            q.z = (float)info.GetValue("z", typeof(float));
            q.w = (float)info.GetValue("w", typeof(float));
            obj = q;
            return obj;   // Formatters ignore this return value //Seems to have been fixed!
        }
    }

}
