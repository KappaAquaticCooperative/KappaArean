using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ServerSingletion<T> : MonoBehaviour where T : MonoBehaviour {

	private static string rootName = "ServerManager(Clone)";
	private static GameObject monoSingletionRoot;

	private static T instance;
	public static T Instance
	{
		get
		{
			if (monoSingletionRoot == null)
			{
				monoSingletionRoot = GameObject.Find(rootName);
				if (monoSingletionRoot == null) Debug.Log("please create a gameobject named " + rootName);
			}
			if (instance == null)
			{
				instance = monoSingletionRoot.GetComponent<T>();
				if (instance == null && monoSingletionRoot != null)
					instance = monoSingletionRoot.AddComponent<T> ();
				else
					return null;
			}
			return instance;
		}
	}
}

public abstract class ClientSingletion<T> : MonoBehaviour where T : MonoBehaviour {

	private static string rootName = "ClientManager(Clone)";
	private static GameObject monoSingletionRoot;

	private static T instance;
	public static T Instance
	{
		get
		{
			if (monoSingletionRoot == null)
			{
				monoSingletionRoot = GameObject.Find(rootName);
				if (monoSingletionRoot == null) Debug.Log("please create a gameobject named " + rootName);
			}
			if (instance == null)
			{
				instance = monoSingletionRoot.GetComponent<T>();
				if (instance == null) instance = monoSingletionRoot.AddComponent<T>();
			}
			return instance;
		}
	}
}
