using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using LitJson;

public class UnitsManager : ClientSingletion<UnitsManager> {
	public List<UnitProperty> Dictionary = new List<UnitProperty>();

	private List<UnitLocalization> UnitLocalizations = new List<UnitLocalization> ();

	void Awake () {
		ReloadUnitsLocalizations ();
	}

	public UnitStates GetUnitStatsByID (int ID) {
		//遍历字典,查找单位
		foreach (UnitProperty d in Dictionary) {
			if (d.Stats.ID == ID) {
				return d.Stats;
			}
		}

		//没有找到!
		Debug.LogError("字典中没有ID为"+ID+"的单位,请进行必要的检查!");
		return null;
	}

	public GameObject GetUnitObjectByID (int ID,bool isPlayer) {
		foreach (UnitProperty d in Dictionary) {
			if (d.Stats.ID == ID) {
				if (!isPlayer)
					return d.UnitObject;
				else
					return d.PlayerObject;
			}
		}

		//没有找到!
		Debug.LogError("字典中没有ID为"+ID+"的单位,请进行必要的检查!");
		return null;
	}
		
	[ContextMenu("重新读取单位文本")]
	public void ReloadUnitsLocalizations () {
		string LocalizationPath = Application.streamingAssetsPath + "/UnitLocalization.json";
		UnitLocalizations = JsonMapper.ToObject<List<UnitLocalization>> (File.ReadAllText (LocalizationPath));
	}

	public string GetUnitsName (int ID){
		foreach (UnitLocalization u in UnitLocalizations) {
			if (u.ID == ID)
				return u.Name;
		}

		return null;
	}

	public string GetUnitsDescription (int ID){
		foreach (UnitLocalization u in UnitLocalizations) {
			if (u.ID == ID)
				return u.Description;
		}

		return null;
	}
}

[System.Serializable]
public class UnitProperty{
	[Header("单位属性,无需编辑")]
	public UnitStates Stats;
	[Header("其他属性")]
	public GameObject UnitObject;
	public GameObject PlayerObject;
}

[System.Serializable]
public class UnitLocalization {
	public int ID;
	public string Name;
	public string Description;
}
