using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Xml.Serialization;
using System.IO;

public class WorldController : MonoBehaviour {

    public static WorldController Instance { get; protected set; }
    public World World { get; protected set; }
    static bool loadWorld = false;

    // Use this for initialization
    void OnEnable()
    {
        Instance = this;
        Numbers n = new Numbers();
        Words w = new Words();
        WorldTime wt = new WorldTime();
        if (loadWorld)
        {
            CreateWorldFromSaveFile();
        }
        else
        {
            CreateVillageWorld();
        }
    }
	
	// Update is called once per frame
	void Update () {

        World.Current.Update(Time.deltaTime);
    }

    void CreateVillageWorld()
    {
        World = new World(Numbers.Current.VillageWidth, Numbers.Current.VillageHeight, loadWorld);
        Camera.main.transform.position = new Vector3(World.Width / 2, World.Height / 2, Camera.main.transform.position.z);
        
    }
    public void NewWorld()
    {
        loadWorld = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void SaveWorld()
    {

        Debug.Log("SaveWorld button was clicked.");

        World.SaveGame();
        return;

        XmlSerializer serializer = new XmlSerializer(typeof(World));
        TextWriter writer = new StringWriter();
        serializer.Serialize(writer, World.Current);
        writer.Close();

        Debug.Log(writer.ToString());

        PlayerPrefs.SetString("SaveGame00", writer.ToString());
    }

    public void LoadWorld()
    {

        loadWorld = true;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void CreateWorldFromSaveFile()
    {
        Debug.Log("CreateWorldFromSaveFile");
        // Create a world from our save file data.

        World = new World(Numbers.Current.VillageWidth, Numbers.Current.VillageHeight, loadWorld);
        World.LoadGame();
        return;
        XmlSerializer serializer = new XmlSerializer(typeof(World));
        TextReader reader = new StringReader(PlayerPrefs.GetString("SaveGame00"));
        Debug.Log(reader.ToString());
        World = (World)serializer.Deserialize(reader);
        reader.Close();



        // Center the Camera
        Camera.main.transform.position = new Vector3(World.Width / 2, World.Height / 2, Camera.main.transform.position.z);

    }
}
