using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Net;
using System;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Threading;
using LitJson;
using System.Linq;

public class ClientManager : ClientSingletion<ClientManager>
{
	public static string Version = "V0.12";
    public static Socket MySocket;
    public int MyID = 0;

	public bool CanPlay;

	public event ChattingDelegate OnChatting;

	public Dictionary<int, ClientsProperty> clients = new Dictionary<int, ClientsProperty>();
	public Dictionary<int, ClientsProperty> NetworkObjects = new Dictionary<int, ClientsProperty>();

	[HideInInspector]public delegate void ChattingDelegate(ChatText text);

	[HideInInspector]public event OnConnectSuccessCallBack OnConnected;
	[HideInInspector]public delegate void OnConnectSuccessCallBack (int MapID);
    // Start is called before the first frame update
    void Start() {
//		Connection ("127.0.0.1", 2333);
    }

    // Update is called once per frame
    void Update()
    {
		if (NetworkInterface.Instance.Player == null) {
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;
			NetworkInterface.Instance.ActiveArms ();
		} else {
			NetworkInterface.Instance.DeActiveArms ();
		}
    }

	public void Connection(string Address,int Port)
	{
		MySocket = new Socket (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
		IPAddress ipAd = IPAddress.Parse (Address);
		IPEndPoint ipEp = new IPEndPoint (ipAd, Port);
		Debug.Log (ipEp.ToString ());
		//Connect
		MySocket.Connect (ipEp);
		Debug.Log ("Connected");
		StartCoroutine (Selecting ());
		RegistPlayer ();
	}

    public IEnumerator Selecting ()
    {
        List<Socket> CheckRead = new List<Socket>();
        while (true)
        {
            CheckRead.Clear();
            CheckRead.Add(MySocket);
            //Selecting
            Socket.Select(CheckRead, null, null, 0);
            //check
            foreach(Socket s in CheckRead)
            {
                byte[] readBuff = new byte[65535];
                int count = s.Receive(readBuff);
                string text = System.Text.Encoding.Default.GetString(readBuff);
                FiringMessage(text);
            }
            yield return null;
        }
    }

	public void SendMessage (MessageBase Basement) {
		string s = JsonUtility.ToJson (Basement);
		s = s + "$";
		byte[] sendBytes = System.Text.Encoding.Default.GetBytes(s);
		MySocket.Send (sendBytes);
	}

    #region 方法列表
    public void FiringMessage (string s)
    {
		string[] message = s.Split('$');
		foreach (string text in message) {
			try {
				if (text.Length < 4)
					return;
				MessageBase TempBase = JsonUtility.FromJson<MessageBase> (text);
				MessageBase MsgBase = (MessageBase)JsonUtility.FromJson (text, Type.GetType (TempBase.ProtoName));

				//批发处理
				if (typeof(MessageMove).IsAssignableFrom (Type.GetType (MsgBase.ProtoName))) {
					MessageMove move = (MessageMove)MsgBase;
					OnMoveMessage (move);
				} else if (typeof(MessageAttack).IsAssignableFrom (Type.GetType (MsgBase.ProtoName))) {
					MessageAttack attack = (MessageAttack)MsgBase;
					OnAttackMessage (attack);
				} else if (typeof(MessageBuildingAttack).IsAssignableFrom (Type.GetType (MsgBase.ProtoName))) {
					MessageBuildingAttack attack = (MessageBuildingAttack)MsgBase;
					OnBuildingAttackMessage (attack);
				} else if (typeof(MessageBuildingFocusing).IsAssignableFrom (Type.GetType (MsgBase.ProtoName))) {
					MessageBuildingFocusing focus = (MessageBuildingFocusing)MsgBase;
					OnBuildingFocusMessage (focus);
				} else if (typeof(MessageInteractiveStart).IsAssignableFrom (Type.GetType (MsgBase.ProtoName))) {
					MessageInteractiveStart interactive = (MessageInteractiveStart)MsgBase;
					OnInteractiveStartMessage (interactive);
				}  else if (typeof(MessageInteractiveExit).IsAssignableFrom (Type.GetType (MsgBase.ProtoName))) {
					MessageInteractiveExit interactive = (MessageInteractiveExit)MsgBase;
					OnInteractiveExitMessage (interactive);
				}  else if (typeof(MessageRespawnConstruction).IsAssignableFrom (Type.GetType (MsgBase.ProtoName))) {
					MessageRespawnConstruction respawn = (MessageRespawnConstruction)MsgBase;
					OnConstructionRespawn (respawn);
				} else if (typeof(MessageChangingWeapon).IsAssignableFrom (Type.GetType (MsgBase.ProtoName))) {
					MessageChangingWeapon Stats = (MessageChangingWeapon)MsgBase;
					OnChangeingWeaponMessage (Stats);
				} else if (typeof(MessageDamage).IsAssignableFrom (Type.GetType (MsgBase.ProtoName))) {
					MessageDamage Stats = (MessageDamage)MsgBase;
					OnDamageMessage (Stats);
				} else if (typeof(MessagePlayerDead).IsAssignableFrom (Type.GetType (MsgBase.ProtoName))) {
					MessagePlayerDead dead = (MessagePlayerDead)MsgBase;
					OnPlayerDeadMessage (dead);
				} else if (typeof(MessageClientsStats).IsAssignableFrom (Type.GetType (MsgBase.ProtoName))) {
					MessageClientsStats Stats = (MessageClientsStats)MsgBase;
					OnNewClientMessage (Stats);
				} else if (typeof(MessageRespawnPlayer).IsAssignableFrom (Type.GetType (MsgBase.ProtoName))) {
					MessageRespawnPlayer respawn = (MessageRespawnPlayer)MsgBase;
					OnRespawnPlayer ((MessageRespawnPlayer)MsgBase);
				} else if (typeof(MessageLeaveGame).IsAssignableFrom (Type.GetType (MsgBase.ProtoName))) {
					MessageLeaveGame leave = (MessageLeaveGame)MsgBase;
					OnPlayerLeaveGame (leave);
				} else if (typeof(MessageChat).IsAssignableFrom (Type.GetType (MsgBase.ProtoName))) {
					MessageChat chat = (MessageChat)MsgBase;
					OnPlayerChatting (chat);
				}else {
					Debug.Log ("未知数据类型，该数据为：" + MsgBase.ToString () + "，记得处理！");
				}
			}catch (ArgumentException ex) {

			}catch (Exception ex){
				Debug.LogError ("协议包无法解析!出错报告为:"+ ex.ToString ());
				Debug.LogError ("数据包为:"+text);
			}
		}
    }
		
    void OnMoveMessage (MessageMove message)
    {
		//自己的方法
		if (message.NetworkID == MyID) {
			return;
			float distance = Vector3.Distance (transform.position, message.Position);
			if (distance > 3) {
				Debug.Log ("Name"+clients [MyID].unit);
				clients [MyID].unit.transform.SetPositionAndRotation (message.Position, Quaternion.Euler(message.Rotation));
			}
		} else {//别人的方法
			clients[message.NetworkID].unit.Moving(message.Position,Quaternion.Euler(message.Rotation));
		}
    }

	public void OnAttackMessage (MessageAttack message){
		if (message.AttackerNetworkID != MyID) {
			clients [message.AttackerNetworkID].unit.Attack (message.TargetPosition, message.StartPosition, message.ComboList);
		}
	}

	public void OnBuildingAttackMessage (MessageBuildingAttack message) {
		try {
			if (NetworkObjects [message.AttackerNetworkID].unit.GetComponent<MannedCannon> ().User.tag == Tags.player)
				return;
		} catch {
			
		}

		NetworkObjects [message.AttackerNetworkID].unit.GetComponent<CannonBasement> ().Attack (message.TargetPosition, message.StartPosition);
	}

	public void OnBuildingFocusMessage (MessageBuildingFocusing message) {
		NetworkObjects [message.AttackerNetworkID].unit.GetComponent<CannonBasement> ().FocusPosition (message.TargetPosition);
	}

	public void OnInteractiveStartMessage(MessageInteractiveStart message){
		NetworkObjects [message.ObjectID].unit.GetComponent<IInteractable> ().OnInteractiveStart (clients [message.UserID].unit);
		clients [message.UserID].unit.GetComponent<InfantryBasement> ().InteractiveStart (NetworkObjects [message.ObjectID].unit);
	}

	public void OnInteractiveExitMessage(MessageInteractiveExit message){
		NetworkObjects [message.ObjectID].unit.GetComponent<IInteractable> ().OnInteractiveExit (clients [message.UserID].unit);
		clients [message.UserID].unit.GetComponent<InfantryBasement>().InteractiveExit (NetworkObjects [message.ObjectID].unit);
	}

	public void OnChangeingWeaponMessage (MessageChangingWeapon message){
		if (message.TargetID != MyID)
			clients [message.TargetID].unit.GetComponent<InfantryBasement> ().ChangingWeapon (message.ChangedWeapon);
	}

	public void OnDamageMessage (MessageDamage message){
		Unit attacker = null;
		Unit viticm = null;
		//找攻击方
		foreach (var v in clients) {
			if(v.Key == message.AttackerNetworkID)
				attacker = clients[v.Key].unit;
		}
		foreach (var v in NetworkObjects) {
			if(v.Key == message.AttackerNetworkID)
				attacker = NetworkObjects[v.Key].unit;
		}
		//找被害方
		foreach (var v in clients) {
			if(v.Key == message.ViticmNetworkID)
				viticm = clients[v.Key].unit;
		}
		foreach (var v in NetworkObjects) {
			if(v.Key == message.ViticmNetworkID)
				viticm = NetworkObjects[v.Key].unit;
		}
		//开始实施攻击
		viticm.TakeDamage(message.WeaponPower,attacker);
	}
		
	public void OnPlayerDeadMessage (MessagePlayerDead message) {
		try {
			foreach(int ID in clients.Keys){
				if(ID == message.NetworkID){
					clients [message.NetworkID].NetworkStates.IsAlive = false;
					clients [message.NetworkID].unit.Death ();
					return;
				}
			}
			foreach(int ID in NetworkObjects.Keys){
				if(ID == message.NetworkID){
					NetworkObjects [message.NetworkID].NetworkStates.IsAlive = false;
					NetworkObjects [message.NetworkID].unit.Death ();
					return;
				}
			}
		} catch {
			
		}
	}

    void OnNewClientMessage(MessageClientsStats message)
    {
		Debug.Log (JsonUtility.ToJson (message));
		ChatText text = new ChatText (message.PlayerName,"加入了游戏");
		OnChatting (text);
		OnConnected (message.MapID);

		//自己的请求
		if (MyID == 0) {
			MyID = message.NetworkID;
			foreach (ClientState s in message.clients) {
				if (!clients.ContainsKey (s.NetworkID)) {
					if (s.NetworkID == message.NetworkID)//添加自己
						clients.Add (message.NetworkID, new ClientsProperty (s, null));	
				} else {
					Debug.LogError ("ID为" + s.NetworkID + "的玩家已经存在!请检查脚本");
				}
			}
//			RespawnRequest (); 早期测试用
		}

		//别人的请求
		foreach (ClientState s in message.clients) {
			if (s.NetworkID == MyID || clients.ContainsKey(s.NetworkID))
				continue;

			//新玩家加入,添加玩家
			if (s.IsAlive) {
				Debug.Log (s.Stats.Position);
				GameObject go = GameObject.Instantiate (UnitsManager.Instance.GetUnitObjectByID (s.Stats.ID, false), s.Stats.Position, s.Stats.Rotation) as GameObject;
				go.GetComponent<Unit> ().States = s.Stats;
//				go.GetComponent<Unit> ().States.PerHP = s.Stats.PerHP;
//				go.GetComponent<Unit> ().States.PerLife = s.Stats.PerLife;
				go.GetComponent<Unit> ().Moving (s.Stats.Position, s.Stats.Rotation);
				clients.Add (s.NetworkID, new ClientsProperty (s, go.GetComponent<Unit> ()));
			} else {
				clients.Add (s.NetworkID, new ClientsProperty (s, null));
			}
		}
    }

	public void OnRespawnPlayer (MessageRespawnPlayer message) {
		//TODOnetworkID来锁定目标,UnitID来确定复活的单位
		//判断ID是否为自己
		if (MyID == message.NetworkID && clients [message.NetworkID].NetworkStates.IsAlive == false) {
			GameObject go = GameObject.Instantiate (UnitsManager.Instance.GetUnitObjectByID (message.UnitID, true), message.Position, Quaternion.Euler(message.Rotation));
			clients [message.NetworkID].NetworkStates.IsAlive = true;
			clients [message.NetworkID].unit = go.GetComponent<Unit> ();
			clients [message.NetworkID].unit.States = message.States;
			clients [message.NetworkID].unit.States.PerHP = message.States.MaxHP;
			clients [message.NetworkID].unit.States.PerLife = message.States.MaxLife;
		}
		else if(clients [message.NetworkID].NetworkStates.IsAlive == false) {
			Debug.Log (clients [message.NetworkID]);
			GameObject go = GameObject.Instantiate (UnitsManager.Instance.GetUnitObjectByID (message.UnitID, false), message.Position,Quaternion.Euler(message.Rotation));
			clients [message.NetworkID].NetworkStates.IsAlive = true;
			clients [message.NetworkID].unit = go.GetComponent<Unit> ();
			clients [message.NetworkID].unit.States = message.States;
			clients [message.NetworkID].unit.States.PerHP = message.States.MaxHP;
			clients [message.NetworkID].unit.States.PerLife = message.States.MaxLife;
		}
	}

	public void OnConstructionRespawn (MessageRespawnConstruction message){
		//初始化
		GameObject go = GameObject.Instantiate (ConstructionList.Instance.GetConstructionsByID (message.RequestedBuildingID), message.Position, Quaternion.identity);
		ClientState state = new ClientState ();
		state.NetworkID = message.NetworkID;
		ConstructionBasement u = go.GetComponent<ConstructionBasement> ();
		u.States.FactionID = clients [message.RequesterID].unit.States.FactionID;
		ClientsProperty pro = new ClientsProperty (state,go.GetComponent<Unit>());

		NetworkObjects.Add (message.NetworkID, pro);
		u.Initializator = clients [message.RequesterID].unit;
	}

	public void OnPlayerLeaveGame (MessageLeaveGame leave) {
		ChatText text = new ChatText (clients [leave.ClientID].NetworkStates.NickName, "<color=red>离开了游戏</color>");
		OnChatting (text);

		clients [leave.ClientID].NetworkStates = null;
		try {
			Destroy (clients [leave.ClientID].unit.gameObject);
		} catch {
			
		}
		clients [leave.ClientID].unit = null;
		clients.Remove (leave.ClientID);
	}

	public void OnPlayerChatting (MessageChat chat) {
		ChatText text = new ChatText (clients [chat.SpeakerID].NetworkStates.NickName, chat.Context);
		OnChatting (text);
	}
    #endregion
	#region 请求列表
	public void RegistPlayer () {
		MessageClientsStats request = new MessageClientsStats ();
		request.PlayerName = PlayerDataManager.Instance.MyData.Name;
		Debug.Log (PlayerDataManager.Instance.MyData.Name);
		SendMessage (request);
	}

	public void MovingRequest (Vector3 Position,Quaternion Rotation) {
		MessageMove move = new MessageMove (Position,Rotation.eulerAngles);
		move.NetworkID = MyID;
		SendMessage (move);
	}

	public void RespawnRequest (int ID) {
		MessageRespawnPlayer respawn = new MessageRespawnPlayer ();
		respawn.States = UnitsManager.Instance.GetUnitStatsByID (ID);
		respawn.NetworkID = MyID;
		respawn.UnitID = ID;
		respawn.Position = UnityEngine.Random.onUnitSphere + GameObject.FindGameObjectWithTag(Tags.AlliesSpawnPoint).transform.position;
		respawn.Rotation = Vector3.zero;
		SendMessage (respawn);
	}
		
	public void AttackRequest (Vector3 targetpos,Vector3 StartPos,string ComboList) {
		MessageAttack attack = new MessageAttack ();
		attack.ComboList = ComboList;
		attack.AttackerNetworkID = MyID;
		attack.TargetPosition = targetpos;
		attack.StartPosition = StartPos;
		SendMessage (attack);
	}

	public void BuildingAttackRequest (Unit u,Vector3 targetpos,Vector3 StartPos) {
		MessageBuildingAttack attack = new MessageBuildingAttack (GetNetworkIDByUnit(u),targetpos,StartPos);
		SendMessage (attack);
	}

	public void BuildingFocusRequest (Unit u,Vector3 targetpos) {
		MessageBuildingFocusing focus = new MessageBuildingFocusing (GetNetworkIDByUnit(u),targetpos);
		SendMessage (focus);
	}

	public void OnInteractiveStartRequest (Unit user,Unit Target) {
		MessageInteractiveStart message = new MessageInteractiveStart (GetNetworkIDByUnit (user), GetNetworkIDByUnit (Target));//这一行获取不到返回值,虽然返回值是正常的,我也不清楚怎么回事
		message.UserID = GetNetworkIDByUnit (user);
		message.ObjectID = GetNetworkIDByUnit (Target);
		SendMessage (message);
	}

	public void OnInteractiveExitRequest (Unit user,Unit Target) {
		MessageInteractiveExit message = new MessageInteractiveExit (GetNetworkIDByUnit (user), GetNetworkIDByUnit (Target));
		message.UserID = GetNetworkIDByUnit (user);
		message.ObjectID = GetNetworkIDByUnit (Target);
		SendMessage (message);
	}

	public void HealingRequest (Unit Target,float Amount) {
		MessageHealing heal = new MessageHealing ();
		heal.DoctorNetworkID = MyID;
		heal.Health = Amount;
		heal.PatientNetworkID = GetNetworkIDByUnit(Target);
		SendMessage (heal);
	}

	public void ChangingWeaponRequest (int WeaponSlot){
		MessageChangingWeapon weapon = new MessageChangingWeapon ();
		weapon.ChangedWeapon = WeaponSlot;
		weapon.TargetID = MyID;
		SendMessage (weapon);
	}

	public void PutoutSentryPostRequest (Vector3 Position,int ID) {
		MessageRespawnConstruction respawn = new MessageRespawnConstruction (MyID,ID, Position);
		SendMessage (respawn);
	}

	public void ChatRequest (string context){
		MessageChat chat = new MessageChat (MyID, context);
		SendMessage (chat);
	}
	#endregion

	public int GetNetworkIDByUnit (Unit u) {
		foreach (ClientsProperty pro in clients.Values) {
			if (pro.unit == u) {
				return pro.NetworkStates.NetworkID;
			}
		}
		foreach (ClientsProperty state in NetworkObjects.Values) {
			if (state.unit == u)
				return state.NetworkStates.NetworkID;
		}

		//没找到该物体
		Debug.LogError("单位为"+u+"的物体没有找到!请检查脚本");
		return -1;
	}
}

public class ClientsProperty {
	public ClientState NetworkStates;
	public Unit unit;

	public ClientsProperty (ClientState state,Unit u){
		NetworkStates = state;
		unit = u;
	}
}

public class ChatText {
	public ChatText (string speaker,string context){
		Speaker = speaker;
		Context = context;
	}

	public string Speaker;
	public string Context;
}


