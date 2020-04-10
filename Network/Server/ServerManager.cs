using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using LitJson;
using System.Linq;

public class ServerManager : ServerSingletion<ServerManager>
{
    public static Socket ServerSocket;
	public static Dictionary<ClientKeys, ClientStates> Clients = new Dictionary<ClientKeys, ClientStates>();
	public static Dictionary<int, ClientState> NetworkObjects = new Dictionary<int, ClientState>();

	private List<Socket> checkRead = new List<Socket>();
	private Socket ChildServerSocket;
	private bool IsBossAlive;//暂时性
	private int MapID;
    // Start is called before the first frame update
    void Awake() {
//		EstablishingGame ("127.0.0.1", 2333);	
    }

	public void EstablishingGame (ChildServerInformation info) {
		ServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
		IPAddress ipAdr = IPAddress.Parse(info.Address);
		IPEndPoint ipEp = new IPEndPoint(ipAdr, info.Port);
		ServerSocket.Bind(ipEp);
		//Listen
		ServerSocket.Listen(50);
		Debug.Log ("Success!");

		StartCoroutine(SetupServer(info));
		StartCoroutine (ReciveMessage ());
	}

    // Update is called once per frame
    void Update()
    {
		
    }

	IEnumerator SetupServer (ChildServerInformation info) {
		int Port = PlayerPrefs.GetInt ("SubscribePort");
		MapID = info.MapID;

		ChildServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
		IPAddress ipAdr = Dns.GetHostEntry(PlayerPrefs.GetString ("SubscribeAddress")).AddressList[0];
		IPEndPoint ipEp = new IPEndPoint(ipAdr, Port);
		ChildServerSocket.Connect(ipEp);
		MessageSetupServer setup = new MessageSetupServer ();
		WWW w = new WWW(@"http://icanhazip.com/");
		yield return w;
		Debug.Log ("Complete!");

		info.Address = w.text.Replace ("\n", "");
		setup.information = info;
		Debug.Log (JsonUtility.ToJson (info));
		byte[] b = ServerManager.EncodeMessage (JsonUtility.ToJson (setup));
		ChildServerSocket.Send (b);

		yield return null;
	}

	IEnumerator ReciveMessage ()
	{
		while (true) {
			//检查checkRead
			checkRead.Clear ();
			checkRead.Add (ServerSocket);
			foreach (ClientKeys key in Clients.Keys) {
				checkRead.Add (key.socket);
			}
			//select
			Socket.Select (checkRead, null, null, 2000);
			//检查可读对象
			foreach (Socket s in checkRead) {
				if (s == ServerSocket) {
					ReadListenfd (s);
				} else {
					ReadClientfd (s);
				}
			}
			yield return null;
		}
	}
	#region 新玩家加入
    public void ReadListenfd(Socket listenfd)
    {
		ClientKeys key = new ClientKeys ();
		key.socket = listenfd.Accept ();
		ClientStates state = new ClientStates ();
		state.Network = new NetworkPartment ();
		Clients.Add (key, state);
    }
	#endregion
    public bool ReadClientfd(Socket clientfd)
    {
        //获取目标
        ClientKeys key = null;
        foreach (ClientKeys k in Clients.Keys)
        {
            if (k.socket == clientfd)
            {
                key = k;
                break;
            }
        }
        ClientStates state = Clients[key];

        //接收
        int count = 0;
        try
        {
			state.Network.ReadBuff = new byte[65535];
			count = clientfd.Receive(state.Network.ReadBuff);
			DecodeMessage(clientfd,state.Network.ReadBuff,count);
        }
        catch (SocketException ex)
		{
			MessageLeaveGame message = new MessageLeaveGame (key.NetworkID);
			BroadcastingMessage (JsonUtility.ToJson (message));

            clientfd.Close();
            Clients.Remove(key);
            Debug.Log("[服务端]信息接收失败！连接被迫关闭,错误信息：" + ex.ToString());
            return false;
        }
        //客户端关闭
        if (count == 0)
        {
			MessageLeaveGame message = new MessageLeaveGame (key.NetworkID);
			BroadcastingMessage (JsonUtility.ToJson (message));

            clientfd.Close();
            Clients.Remove(key);
            Debug.Log("[服务端]收到一条连接关闭请求");
            return false;
        }
        return true;
    }

    public int GetEmptyPlayerNetworkID()
    {
        //TODO 200是最大玩家数，后面需要修改
        List<int> UsedID = new List<int>();
        for (int i = 1; i < 1000; i++)
        {
            //无玩家连接
            if (Clients.Count == 0)
                return i;

            //检查是否用空位可以循环利用
            foreach (ClientKeys key in Clients.Keys)
            {
                if (key.NetworkID == i)
                    UsedID.Add(i);
            }
			foreach (int j in NetworkObjects.Keys)
			{
				if (j == i)
					UsedID.Add(i);
			}
        }

        //开始检查ID
        for (int i = 1; i < 1000; i++)
        {
            if (!UsedID.Contains(i))
                return i;
        }

        Debug.LogError("ID爆了！服务器已经报废了！赶快重启！");
        return -1;
    }

	public static byte[] EncodeMessage(string Text)
    {
		string s = Text + "$";
		byte[] sendBytes = System.Text.Encoding.Default.GetBytes(s);
        return sendBytes;
    }

	public void DecodeMessage(Socket sender,byte[] Message,int count)
    {
		string message = System.Text.Encoding.Default.GetString(Message);
//		string[] s = message.Split ('"');
//		string type = s [3];
//		//判断数据类型
//		//TODO 你永远也更新不完
//		if (type == "MessageMove") {
//			try {
//				MessageMove move = (MessageMove)JsonUtility.FromJson (message, Type.GetType (s [3]));
//				OnMoveMessage (move);
//			} catch {
//				Debug.Log (message);
//			}
//		} else if (type == "MessageAttack") {
//			MessageAttack Attack = (MessageAttack)JsonUtility.FromJson (message, Type.GetType (s [3]));
//			OnAttackMessage (Attack);
//		} else if (type == "MessageChangingWeapon") {
//			MessageChangingWeapon change = (MessageChangingWeapon)JsonUtility.FromJson (message, Type.GetType (s [3]));
//			OnChangingWeaponMessage (change);
//		}
//		else if (type == "MessageDamage")
//		{
//			MessageDamage Damage = (MessageDamage)JsonUtility.FromJson(message, Type.GetType(s[3]));
//			OnDamageMessage(Damage);
//		}
//		else if (type == "MessagePlayerDead")
//		{
//			MessagePlayerDead dead = (MessagePlayerDead)JsonUtility.FromJson(message, Type.GetType(s[3]));
//			OnPlayerDeadMessage(dead);
//		}
//		else if (type == "MessageClientsStats")
//		{
//			MessageClientsStats Stats = (MessageClientsStats)JsonUtility.FromJson(message, Type.GetType(s[3]));
//			OnNewClientMessage(Stats);
//		}
//		else if (type == "MessageRespawnPlayer")
//		{
//			MessageRespawnPlayer respawn = (MessageRespawnPlayer)JsonUtility.FromJson(message, Type.GetType(s[3]));
//			OnRespawnPlayer(respawn);
//		}
		string[] s = message.Split('$');
		foreach (string text in s) {
			try {
				MessageBase TempBase = JsonUtility.FromJson<MessageBase> (text);
				MessageBase MsgBase = (MessageBase)JsonUtility.FromJson (text, Type.GetType (TempBase.ProtoName));

				if (typeof(MessageMove).IsAssignableFrom (Type.GetType (MsgBase.ProtoName))) {
					MessageMove move = (MessageMove)MsgBase;
					OnMoveMessage (move);
				}else if (typeof(MessageAttack).IsAssignableFrom (Type.GetType (MsgBase.ProtoName))) {
					MessageAttack attack = (MessageAttack)MsgBase;
					OnAttackMessage (attack);
				} else if (typeof(MessageBuildingAttack).IsAssignableFrom (Type.GetType (MsgBase.ProtoName))) {
					MessageBuildingAttack attack = (MessageBuildingAttack)MsgBase;
					OnBuildingAttackMessage (attack);
				} else if (typeof(MessageBuildingFocusing).IsAssignableFrom (Type.GetType (MsgBase.ProtoName))) {
					MessageBuildingFocusing focus = (MessageBuildingFocusing)MsgBase;
					OnBuildingFocusingMessage (focus);
				} else if (typeof(MessageInteractiveStart).IsAssignableFrom (Type.GetType (MsgBase.ProtoName))) {
					MessageInteractiveStart interactive = (MessageInteractiveStart)MsgBase;
					OnInteractiveStartMessage (interactive);
				}  else if (typeof(MessageInteractiveExit).IsAssignableFrom (Type.GetType (MsgBase.ProtoName))) {
					MessageInteractiveExit interactive = (MessageInteractiveExit)MsgBase;
					OnInteractiveExitMessage (interactive);
				}  else if (typeof(MessageChangingWeapon).IsAssignableFrom (Type.GetType (MsgBase.ProtoName))) {
					MessageChangingWeapon Stats = (MessageChangingWeapon)MsgBase;
					OnChangingWeaponMessage (Stats);
				} else if (typeof(MessageDamage).IsAssignableFrom (Type.GetType (MsgBase.ProtoName))) {
					MessageDamage Stats = (MessageDamage)MsgBase;
					OnDamageMessage (Stats);
				} else if (typeof(MessagePlayerDead).IsAssignableFrom (Type.GetType (MsgBase.ProtoName))) {
					MessagePlayerDead dead = (MessagePlayerDead)MsgBase;
					OnPlayerDeadMessage (dead);
				} else if (typeof(MessageClientsStats).IsAssignableFrom (Type.GetType (MsgBase.ProtoName))) {
					MessageClientsStats Stats = (MessageClientsStats)MsgBase;
					OnNewClientMessage (sender,Stats);
				} else if (typeof(MessageRespawnPlayer).IsAssignableFrom (Type.GetType (MsgBase.ProtoName))) {
					MessageRespawnPlayer respawn = (MessageRespawnPlayer)MsgBase;
					OnRespawnPlayer (respawn);
				}  else if (typeof(MessageRespawnConstruction).IsAssignableFrom (Type.GetType (MsgBase.ProtoName))) {
					MessageRespawnConstruction respawn = (MessageRespawnConstruction)MsgBase;
					OnConstructionRespawn (respawn);
				} else if (typeof(MessageChat).IsAssignableFrom (Type.GetType (MsgBase.ProtoName))) {
					MessageChat chat = (MessageChat)MsgBase;
					OnChattingMessage (chat);
				} else {
					Debug.Log ("未知数据类型，该数据为：" + text + "，服务器已做丢包处理.或者说，你忘记处理了？");
				}
			}catch (ArgumentException ex) {
				
			}catch (Exception ex){
				Debug.LogError ("协议包无法解析!出错报告为:"+ ex.ToString ());
				Debug.LogError ("数据包为:"+text);
			}
		}
    }

    void BroadcastingMessage(string message)
    {
		string s = message + "$";
		byte[] sendBytes = System.Text.Encoding.Default.GetBytes(s);
		foreach (ClientStates cs in Clients.Values) {
			try {
				cs.Network.socket.BeginSend (sendBytes, 0, sendBytes.Length, 0, SendCallBack, ServerSocket);
			} catch {
				Debug.LogError ("辣鸡socket,bug everyday,发送失败而已,这不影响您的游戏,请放心");
			}
		}
    }

	void SendCallBack (IAsyncResult result){

	}

    public ClientKeys GetClientKeyInformation(int i)
    {
        ClientKeys key = null;
        foreach(ClientKeys k in Clients.Keys)
        {
            if(k.NetworkID == i)
            {
                key = k;
                break;
            }
        }
        return key;
    }

    public ClientKeys GetClientKeyInformation(Socket socket)
    {
        ClientKeys key = null;
        foreach (ClientKeys k in Clients.Keys)
        {
            if (k.socket == socket)
            {
                key = k;
                break;
            }
        }
        return key;
    }

    #region 方法
    public void OnMoveMessage(MessageMove message)
    {
		ClientKeys key = GetClientKeyInformation (message.NetworkID);
		Clients [key].State.Stats.Position = message.Position;
		Clients [key].State.Stats.Rotation = Quaternion.Euler(message.Rotation);
        string s = JsonUtility.ToJson(message);
        BroadcastingMessage(s);
    }

	public void OnAttackMessage (MessageAttack message){
		string s = JsonUtility.ToJson(message);
		BroadcastingMessage(s);
	}

	public void OnBuildingAttackMessage (MessageBuildingAttack message){
		string s = JsonUtility.ToJson(message);
		BroadcastingMessage(s);
	}

	public void OnBuildingFocusingMessage (MessageBuildingFocusing message) {
		NetworkObjects [message.AttackerNetworkID].Stats.Rotation = ClientManager.Instance.NetworkObjects [message.AttackerNetworkID].unit.transform.rotation;
		string s = JsonUtility.ToJson(message);
		BroadcastingMessage(s);
	}

	public void OnInteractiveStartMessage(MessageInteractiveStart message){
		string s = JsonUtility.ToJson(message);
		BroadcastingMessage(s);
	}

	public void OnInteractiveExitMessage(MessageInteractiveExit message){
		string s = JsonUtility.ToJson(message);
		BroadcastingMessage(s);
	}

	public void OnChangingWeaponMessage (MessageChangingWeapon message){
		string s = JsonUtility.ToJson(message);
		BroadcastingMessage(s);
	}

	public void OnDamageMessage (MessageDamage message){
		string s = JsonUtility.ToJson(message);
		BroadcastingMessage(s);
	}

	public void OnPlayerDeadMessage (MessagePlayerDead message) {
		foreach (ClientKeys key in Clients.Keys) {
			if (key.NetworkID == message.NetworkID) {
				Clients [key].State.IsAlive = false;
				if (Clients [key].State.Stats.IsBoss == true)
					IsBossAlive = false;
			}
		}

		string s = JsonUtility.ToJson(message);
		BroadcastingMessage(s);
	}
		
	public void OnRespawnPlayer (MessageRespawnPlayer message) {
		foreach (ClientKeys key in Clients.Keys) {
			if (key.NetworkID == message.NetworkID) {
				if (!IsBossAlive) {
					message.UnitID = 4;
					message.Position = GameObject.FindGameObjectWithTag (Tags.BossSpawnPoint).transform.position;
					IsBossAlive = true;
				}

				Clients [key].State.IsAlive = true;
				UnitStates states = UnitsManager.Instance.GetUnitStatsByID (message.UnitID);
				Clients [key].State.Stats = states;
				Clients [key].State.Stats.PerHP = states.PerHP;
				Clients [key].State.Stats.PerLife = states.PerLife;
				message.States = states;
				//开始广播
				string s = JsonUtility.ToJson(message);
				BroadcastingMessage(s);
				return;
			}
		}

		//没有对应的玩家
		Debug.LogError("ID为"+message.NetworkID+"的玩家不存在!该玩家重生请求被拒绝,请检查脚本有没有写错!");
	}

	public void OnConstructionRespawn (MessageRespawnConstruction message) {
		int NetworkID = GetEmptyPlayerNetworkID ();
		message.NetworkID = NetworkID;
		ClientState state = new ClientState ();
		state.Stats = new UnitStates ();
		state.Stats.Position = message.Position;
		state.NetworkID = NetworkID;
		state.IsAlive = true;
		NetworkObjects.Add (NetworkID, state);

		string s = JsonUtility.ToJson(message);
		BroadcastingMessage(s);
	}

	public void OnNewClientMessage(Socket sender,MessageClientsStats message)
    {
		//获取客户端信息
		Debug.Log("[服务器]收到一份连接请求");
		foreach (ClientKeys tempkey in Clients.Keys) {
			if (tempkey.socket == sender) {
				Clients.Remove (tempkey);
				break;
			}
		}
		Socket clientfd = sender;

		//自己用的元素
		ClientKeys key = new ClientKeys();
		ClientStates states = new ClientStates ();
		ClientState state = new ClientState();
		NetworkPartment part = new NetworkPartment();
		//发给玩家的信息
		MessageClientsStats stats = new MessageClientsStats();

		int ID = GetEmptyPlayerNetworkID();
		key.NetworkID = ID;
		state.NetworkID = ID;

		key.socket = clientfd;
		part.socket = clientfd;

		//交付玩家信息
		foreach(ClientStates s in Clients.Values)
		{
			ClientState FakeStates = new ClientState();
			FakeStates.NetworkID = s.State.NetworkID;
			FakeStates.IsAlive = s.State.IsAlive;
			FakeStates.Stats = s.State.Stats;
			FakeStates.NickName = s.State.NickName;
			Debug.Log (FakeStates);
			Debug.Log (stats);
			stats.clients.Add(FakeStates);
		}
		stats.NetworkID = ID;
		stats.PlayerName = message.PlayerName;
		stats.clients.Add (state);
		stats.MapID = MapID;

		//开始整合
		state.NickName = message.PlayerName;
		states.State = state;
		states.Network = part;

		//添加客户端
		Clients.Add(key, states);
		//广播消息
		BroadcastingMessage(JsonUtility.ToJson(stats));
    }

	public void OnChattingMessage (MessageChat message){
		string s = JsonUtility.ToJson(message);
		BroadcastingMessage(s);
	}
    #endregion
	#region 请求
	public void DamageRequest (Unit Attacker,DamageProperty property,Unit Victim) {
		MessageDamage damage = new MessageDamage ();
		//找施暴者
		foreach (ClientsProperty p in ClientManager.Instance.clients.Values) {
			if (Attacker == p.unit) {
				damage.AttackerNetworkID = p.NetworkStates.NetworkID;
			}
		}
		foreach (ClientsProperty p in ClientManager.Instance.NetworkObjects.Values) {
			if(Attacker== p.unit)
				damage.AttackerNetworkID = p.NetworkStates.NetworkID;
		}

		//找受害人
		foreach (ClientsProperty p in ClientManager.Instance.clients.Values) {
			if (Victim == p.unit) {
				damage.ViticmNetworkID = p.NetworkStates.NetworkID;
			}
		}
		foreach (ClientsProperty p in ClientManager.Instance.NetworkObjects.Values) {
			if(Victim== p.unit)
				damage.ViticmNetworkID = p.NetworkStates.NetworkID;
		}

		damage.WeaponPower = property;
		ClientManager.Instance.SendMessage (damage);
	}

	public void KillPlayerRequest (int VictimID) {
		MessagePlayerDead dead = new MessagePlayerDead (VictimID);
		ClientManager.Instance.SendMessage (dead);
	}

	public void SynchronizingUnitStates (int id,UnitStates state){
		foreach (ClientStates s in Clients.Values) {
			if (id == s.State.NetworkID) {
				s.State.Stats = state;
				Debug.Log (s.State.Stats.PerHP);
			}
		}
		foreach (ClientState s in NetworkObjects.Values) {
			if(id == s.NetworkID)
				s.Stats = state;
		}
	}

	public int GetNetworkID (Unit Requester) {
		foreach (ClientsProperty p in ClientManager.Instance.clients.Values) {
			if (Requester == p.unit)
				return p.NetworkStates.NetworkID;
		}
		return -1;
	}
	#endregion
}

[System.Serializable]
public class ClientKeys
{
    public int NetworkID;
    public Socket socket;
}

[System.Serializable]
public class ClientStates
{
	public ClientState State;
	public NetworkPartment Network;
}

[System.Serializable]
public class ClientState {
	public int NetworkID;
	public string NickName;

	public bool IsAlive;
	public UnitStates Stats;
}

public class NetworkPartment {//这部分代码不能作为json数据传输
	public Socket socket;
	public byte[] ReadBuff = new byte[65535];
}

[System.Serializable]
public class ChildServerInformation
{
	public ChildServerInformation (string name,string description,string version,string address,int port,int mapID){
		Name = name;
		Description = description;
		Version = version;
		Address = address;
		Port = port;
		MapID = mapID;
	}

	public string Name;
	public string Description;
	public string Version;
	public string Address;
	public int Port;
	public int MapID;
}

[System.Serializable]
public class MessageBase
{
    public string ProtoName = "";
}

#region 消息模块
public class MessageMove : MessageBase
{
    public MessageMove() { ProtoName = "MessageMove"; }
	public MessageMove(Vector3 position,Vector3 rotation) { ProtoName = "MessageMove"; Position = position;Rotation = rotation;}

    public int NetworkID;
    public Vector3 Position;
	public Vector3 Rotation;
}

public class MessageAttack : MessageBase
{
	public MessageAttack() { ProtoName = "MessageAttack"; }

	public int AttackerNetworkID;
	public Vector3 TargetPosition;
	public Vector3 StartPosition;
	public string ComboList;
}

public class MessageChangingWeapon : MessageBase
{
	public MessageChangingWeapon() { ProtoName = "MessageChangingWeapon"; }

	public int ChangedWeapon;
	public int TargetID;
}

public class MessageDamage : MessageBase
{
	public MessageDamage() { ProtoName = "MessageDamage"; }

	public int AttackerNetworkID;
	public DamageProperty WeaponPower;
	public int ViticmNetworkID;
}

public class MessageHealing : MessageBase
{
	public MessageHealing() { ProtoName = "MessageHealing"; }

	public int DoctorNetworkID;
	public float Health;
	public int PatientNetworkID;
}

public class MessagePlayerDead : MessageBase
{
	public MessagePlayerDead(int VictimID) { ProtoName = "MessagePlayerDead"; NetworkID = VictimID;}

	public int NetworkID;
}

public class MessageBuildingAttack : MessageBase
{
	public MessageBuildingAttack(int ID,Vector3 targetpoint,Vector3 startpoint) { 
		ProtoName = "MessageBuildingAttack"; 
		AttackerNetworkID = ID;
		TargetPosition = targetpoint;
		StartPosition = startpoint;
	}

	public int AttackerNetworkID;
	public Vector3 TargetPosition;
	public Vector3 StartPosition;
}

public class MessageBuildingFocusing : MessageBase
{
	public MessageBuildingFocusing(int ID,Vector3 targetpoint) { 
		ProtoName = "MessageBuildingFocusing"; 
		AttackerNetworkID = ID;
		TargetPosition = targetpoint;
	}

	public int AttackerNetworkID;
	public Vector3 TargetPosition;
}

public class MessageInteractiveStart : MessageBase{
	public MessageInteractiveStart (int UserID,int ObjectID) {
		ProtoName = "MessageInteractiveStart"; 
		UserID = UserID;
		ObjectID = ObjectID;
	}

	public int UserID;
	public int ObjectID;
}

public class MessageInteractiveExit : MessageBase{
	public MessageInteractiveExit (int UserID,int ObjectID) {
		ProtoName = "MessageInteractiveExit"; 
		UserID = UserID;
		ObjectID = ObjectID;
	}

	public int UserID;
	public int ObjectID;
}

public class MessageLeaveGame : MessageBase
{
	public MessageLeaveGame(int ID) { ProtoName = "MessageLeaveGame";ClientID = ID; }

	public int ClientID;
}

public class MessageRespawnPlayer : MessageBase
{
	public MessageRespawnPlayer() { ProtoName = "MessageRespawnPlayer"; }

	public int UnitID;
	public int NetworkID;
	public UnitStates States;
	public Vector3 Position;
	public Vector3 Rotation;
}

public class MessageRespawnConstruction : MessageBase{
	public MessageRespawnConstruction() { ProtoName = "MessageRespawnConstruction"; }
	public MessageRespawnConstruction(int requester,int buildingid,Vector3 position) { 
		ProtoName = "MessageRespawnConstruction";
		RequesterID = requester;
		RequestedBuildingID = buildingid;
		Position = position;
	}

	public int RequesterID;
	public int NetworkID;
	public int RequestedBuildingID;
	public Vector3 Position;
}

public class MessageClientsStats : MessageBase
{
	public MessageClientsStats() { ProtoName = "MessageClientsStats"; }

    public int NetworkID;
	public int MapID;
	public string PlayerName;
	public List<ClientState> clients = new List<ClientState>();
	public List<ClientState> networkobjects = new List<ClientState>();
}

public class MessageChat : MessageBase {
	public MessageChat (int ID,string context) {
		ProtoName = "MessageChat";
		SpeakerID = ID;
		Context = context;
	}

	public int SpeakerID;
	public string Context;
}


//以下内容和游戏无关和索引器有关
public class MessageSetupServer : MessageBase
{
	public MessageSetupServer() { ProtoName = "MessageSetupServer"; }

	public ChildServerInformation information;
}

public class MessageSetupClient : MessageBase
{
	public MessageSetupClient() { ProtoName = "MessageSetupClient"; }

	public string Version;
}

public class MessageGetServerIPAddress : MessageBase
{
	public MessageGetServerIPAddress() { ProtoName = "MessageGetServerIPAddress"; }
}
	
public class MessageReturnServerIPAddress : MessageBase
{
	public MessageReturnServerIPAddress() { ProtoName = "MessageReturnServerIPAddress"; }

	public List<ChildServerInformation> Servers = new List<ChildServerInformation>();
}
	
#endregion