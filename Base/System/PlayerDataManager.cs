using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;
using System.IO;
using System;

public class PlayerDataManager : ClientSingletion<PlayerDataManager> {
	public PlayerData MyData;
	public static string GameDataPath = System.Environment.GetFolderPath (Environment.SpecialFolder.MyDocuments) + "/KAC/KappaArean";

	// Use this for initialization
	void Awake () {
		LoadGame ();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	/// <summary>
	/// 通过该方法来读取游戏.
	/// </summary>
	public void LoadGame () {
		MyData = LoadData ();
	}

	[ContextMenu("读取数据")]
	public PlayerData LoadData () {
		try {
			string text = File.ReadAllText (GameDataPath + "/save.json");
			PlayerData data = new PlayerData ();
			try {
				data = JsonMapper.ToObject<PlayerData> (text);
			} catch (Exception ex) {
				Debug.Log (ex.ToString ());
			}
			return JsonMapper.ToObject<PlayerData> (text);
		} catch {
			return new PlayerData ();
		}
	}

	[ContextMenu("存储游戏")]
	public void SaveGame () {
		if (!File.Exists (GameDataPath + "/save.json")) {
			Directory.CreateDirectory (GameDataPath);
		}
		string text = JsonMapper.ToJson (MyData);
		File.WriteAllText (GameDataPath+"/save.json",text);
	}
}

[System.Serializable]
public class PlayerData {
	public string Name;
	public float Experience;
	public int Money;
	public int Dollar;
	public int[] UnlockedWeaponID;

	public PlayerData () {
		Name = "Touhou Player";
	}
}
