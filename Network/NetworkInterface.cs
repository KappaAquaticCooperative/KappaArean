using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using UnityEngine.SceneManagement;
using System;
using System.Text;

public class NetworkInterface : MonoBehaviour {
	public static NetworkInterface Instance;

	public ServerManager server;
	public ClientManager client;

	public RectTransform InterfaceMotherboard;
	public RectTransform ButtonMotherboard;
	public RectTransform InteractiveMotherboard;

	public RectTransform DirectlyConnectBoard;
	public RectTransform SearchGameBoard;
	public RectTransform SetupServerBoard;
	public RectTransform DevelopLogBoard;
	public RectTransform CharacterBoard;
	[Header("玩家档案")]
	public InputField NickNameField;
	public Text MoneyField;
	[Header("订阅服务器")]
	public RectTransform SearchingBoard;
	public RectTransform SubscribeButton;
	public InputField IndexerServerAddressInputField;
	public InputField IndexerServerPortInputField;
	public ChildServerButton ServerButton;
	public Button JoinServerButton;
	public Text IndexerServerOutputer;

	public InputField ClientPortInputField;
	public InputField ClientIPAddressInputField;
	[Header("创建服务器区")]
	public InputField ServerPortInputField;
	public InputField ServerNameInputField;
	public InputField ServerDescriptionInputField;
	public Text SetupServerDebug;

	[Header("战斗UI")]
	public RectTransform BattleUI;
	public Slider HPBar;
	public Text HPText;
	public Slider BossBar;
	public Text BossName;

	public RectTransform AmmoLable;
	public Text AmmoText;

	public Text HitFeedbackUI;
	[Header("兵种选择")]
	public RectTransform ArmsChosen;
	public Camera GameCamera;

	[Header("聊天系统")]
	public Image ChatBoxPanel;
	public Text ChatBoxContent;
	public InputField ChatField;
	public float ChatBoxShowTimer = 0;

	[HideInInspector]
	public bool IsServer = false;
	private string address;
	private int port;
	private string ServerName = "";
	private string ServerDescription = "";
	private Socket SearchingAgentSocket;
	private ChildServerInformation information;
	private byte[] ReadBuff = new byte[65535];
	string ReceiveStr = "";

	private int ChosedMap = 1;

	[HideInInspector]
	public PlayerBase Player;
	[HideInInspector]
	public bool CanPlay = true;
	private Unit Enemy;
	// Use this for initialization
	void Start () {
		NickNameField.text = PlayerDataManager.Instance.MyData.Name;
		MoneyField.text = PlayerDataManager.Instance.MyData.Money.ToString();
		Instance = this;
		DontDestroyOnLoad (gameObject);
		ChangeMapOption (1);
		SceneManager.sceneLoaded += OnSceneLoadComplete;
//		BattleUI.gameObject.SetActive (false);
		ClientManager.Instance.OnChatting += Chatting;
		ClientManager.Instance.OnConnected += LoadScene;
		try{
			IndexerServerAddressInputField.text = PlayerPrefs.GetString ("SubscribeAddress");
			IndexerServerPortInputField.text = PlayerPrefs.GetInt ("SubscribePort").ToString();
		}catch{
			
		}
	}
	
	// Update is called once per frame
	void Update () {
		//玩家处理
		try {
			Player = GameObject.FindGameObjectWithTag (Tags.player).GetComponent<PlayerBase> ();
			ModifyHP ();
			ModifyAmmo ();
			if (Enemy == null) {
				foreach (var v in ClientManager.Instance.clients) {
					if (v.Value.unit.States.FactionID == 2) {
						Enemy = v.Value.unit;
					}
				}
			}
			ModifyEnemyHP();
		} catch {

		}

		//其他处理
		if (Input.GetKeyDown (KeyCode.T))
			ChatBoxShowTimer = 10;
		ModifyChatBox();
	}

	public void HostingGame () {
		IsServer = true;
		address = GetLocalHostIP ();
		port = int.Parse (ServerPortInputField.text);
		ServerName = ServerNameInputField.text;
		ServerDescription = ServerDescriptionInputField.text;
		StartGame ();
	}

	public void JoiningGame () {
		IsServer = false;
		address = ClientIPAddressInputField.text;
		port = int.Parse (ClientPortInputField.text);
		StartGame ();
	}

	public void JoiningGame (string Address,int Port) {
		IsServer = false;
		address = Address;
		port = Port;
		StartGame ();
	}

	void StartGame () {
		ChildServerInformation info = new ChildServerInformation (ServerName,ServerDescription,ClientManager.Version,address,port,ChosedMap);
		if (IsServer) {
			server.gameObject.SetActive (true);
			server.EstablishingGame (info);
			client.gameObject.SetActive (true);
			client.Connection (address, port);
		} else {
			client.gameObject.SetActive (true);
			client.Connection (address, port);
		}
	}

	void LoadScene (int ID) {
		if(SceneManager.GetActiveScene().buildIndex != ID)
			SceneManager.LoadScene (ID);
	}

	void OnSceneLoadComplete (Scene scene,LoadSceneMode mode){
		BattleUI.gameObject.SetActive (true);
		InterfaceMotherboard.gameObject.SetActive (false);
		GameCamera.transform.SetPositionAndRotation (GameObject.FindGameObjectWithTag (Tags.CameraPoint).transform.position,GameObject.FindGameObjectWithTag (Tags.CameraPoint).transform.rotation);
	}

	public void SearchingServer () {
		SearchingAgentSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
		string Address = IndexerServerAddressInputField.text;
		int Port = int.Parse(IndexerServerPortInputField.text);

		IPAddress ipAd = null;
		try{
			ipAd = Dns.GetHostEntry(Address).AddressList[0];
		}catch(SocketException ex){
			Debug.Log (ex.ErrorCode);
		}
		Debug.Log (ipAd.ToString());
		IPEndPoint ipEp = new IPEndPoint(ipAd, Port);
		SearchingAgentSocket.Connect(ipEp);
		PlayerPrefs.SetString ("SubscribeAddress",Address);
		PlayerPrefs.SetInt ("SubscribePort",Port);
		IndexerServerOutputer.text = "连接索引器成功!";
		MessageSetupClient setup = new MessageSetupClient ();
		setup.Version = ClientManager.Version;
		MessageGetServerIPAddress request = new MessageGetServerIPAddress ();
		byte[] b = ServerManager.EncodeMessage (JsonUtility.ToJson (setup));
		SearchingAgentSocket.Send (b);
		Invoke ("Refresh",0.5f);
	}

	public void Refresh () {
		MessageGetServerIPAddress request = new MessageGetServerIPAddress ();
		byte[] b = ServerManager.EncodeMessage (JsonUtility.ToJson (request));
		IndexerServerOutputer.text = "正在发送...";
		SearchingAgentSocket.Send (b);
		StartCoroutine (OnRefresh ());
	}

	IEnumerator OnRefresh () {
		SearchingAgentSocket.Receive (ReadBuff);
		ReceiveStr = System.Text.Encoding.Default.GetString (ReadBuff);
		HandleResult ();

		yield return null;
	}

	void HandleResult () {
		foreach (RectTransform go in SearchingBoard) {
			Destroy (go.gameObject);
		}

		string[] message = ReceiveStr.Split ('$');
		Debug.Log (ReceiveStr);
		foreach (string text in message) {
			try {
				MessageBase TempBase = JsonUtility.FromJson<MessageBase> (text);
				MessageBase MsgBase = (MessageBase)JsonUtility.FromJson (text, Type.GetType (TempBase.ProtoName));

				if (typeof(MessageReturnServerIPAddress).IsAssignableFrom (Type.GetType (MsgBase.ProtoName))) {
					MessageReturnServerIPAddress request = (MessageReturnServerIPAddress)MsgBase;
					IndexerServerOutputer.text = "收到索引器返回讯息,当前服务器总数为:" + request.Servers.Count.ToString();

					int PosY = 0;
					foreach(ChildServerInformation info in request.Servers){
						ChildServerButton button = GameObject.Instantiate(ServerButton.gameObject,SearchingBoard).GetComponent<ChildServerButton>();
						button.Setup(info);
						button.GetComponent<RectTransform>().anchoredPosition = new Vector2(0,PosY);
						PosY += 40;
					}
				}
			}catch{

			}
		}
	}

	public void ChangeMapOption (int MapID) {
		SetupServerDebug.text = "您选择了地图" + SceneManager.GetSceneByBuildIndex (ChosedMap).name;
		ChosedMap = MapID;
	}

	public void ChatBoxInputting () {
		ClientManager.Instance.CanPlay = false;
		ChatBoxShowTimer = Mathf.Infinity;
	}

	public void ChatBoxInputtingFinished () {
		ClientManager.Instance.CanPlay = true;
		ChatBoxShowTimer = 5;
		ClientManager.Instance.ChatRequest (ChatField.text);
		ChatField.text = "";
	}

	public void Chatting (ChatText text){
		ChatBoxContent.text = text.Speaker + ":\"" + text.Context + "\"" + System.Environment.NewLine + ChatBoxContent.text;
		ChatBoxShowTimer = 5f;
	}

	public void OnDefaultButtonClick () {
		IndexerServerAddressInputField.text = "5e2c284b13791.freetunnel.cc";
		IndexerServerPortInputField.text = "20343";
	}

	public void OnChildServerButtonClick (ChildServerInformation info) {
		IndexerServerOutputer.text = info.Name + "\n" + info.Description + "\n" + info.Version + "\n" + info.Address+":"+info.Port;
		information = info;
	}

	public void OnJoinServerButtonClick () {
		NetworkInterface.Instance.DeActiveAllInterface ();
		try{
			NetworkInterface.Instance.JoiningGame(information.Address,information.Port);
		}catch (Exception ex){
			NetworkInterface.Instance.ActiveAllInterface ();
			information = null;
			IndexerServerOutputer.text = "<color=red>加入游戏失败!错误信息为:</color>"+"\n" + ex.ToString();
		}
	}

	public void DeActiveAllInterface () {
		InterfaceMotherboard.gameObject.SetActive (false);
	}

	public void ActiveAllInterface () {
		InterfaceMotherboard.gameObject.SetActive (true);
	}

	public void ShowSerchingBoard () {
		DeactiveInteractiveBoard ();
		SearchGameBoard.gameObject.SetActive (true);
	}

	public void ShowDirectlyConnectBoard () {
		DeactiveInteractiveBoard ();
		DirectlyConnectBoard.gameObject.SetActive (true);
	}

	public void ShowSetupServerBoard () {
		DeactiveInteractiveBoard ();
		SetupServerBoard.gameObject.SetActive (true);
	}

	public void ShowDevelopLogBoard () {
		DeactiveInteractiveBoard ();
		DevelopLogBoard.gameObject.SetActive (true);
	}

	public void ShowCharacterBoard () {
		DeactiveInteractiveBoard ();
		CharacterBoard.gameObject.SetActive (true);
	}

	public void ActiveButtonMotherboard () {
		ButtonMotherboard.gameObject.SetActive (true);
		SearchGameBoard.gameObject.SetActive (false);
		DirectlyConnectBoard.gameObject.SetActive (false);
		DevelopLogBoard.gameObject.SetActive (false);
		SetupServerBoard.gameObject.SetActive (false);
		CharacterBoard.gameObject.SetActive (false);
	}

	public void DeactiveInteractiveBoard () {
		ButtonMotherboard.gameObject.SetActive (false);
		SearchGameBoard.gameObject.SetActive (false);
		DirectlyConnectBoard.gameObject.SetActive (false);
		DevelopLogBoard.gameObject.SetActive (false);
		SetupServerBoard.gameObject.SetActive (false);
		CharacterBoard.gameObject.SetActive (false);
	}

	public void ModifyChatBox () {
		ChatBoxShowTimer -= Time.deltaTime;
		ChatBoxShowTimer = Mathf.Clamp (ChatBoxShowTimer, -1, Mathf.Infinity);

		if (ChatBoxShowTimer <= 0) {
			ClientManager.Instance.CanPlay = true;
			ChatBoxPanel.gameObject.SetActive (false);
		} else {
			ChatBoxPanel.gameObject.SetActive (true);
		}
	}

	public void ModifyName () {
		PlayerDataManager.Instance.MyData.Name = NickNameField.text;
	}

	public void ModifyMoney(){
		MoneyField.text = PlayerDataManager.Instance.MyData.Money.ToString();
	}

	void ModifyHP () {
		HPBar.maxValue = Player.States.MaxHP;
		HPBar.value = Player.States.PerHP;
		HPText.text = Player.States.PerHP.ToString();
	}

	void ModifyEnemyHP () {
		BossBar.maxValue = Enemy.States.MaxHP;
		BossBar.value = Enemy.States.PerHP;
		//TODO boss的名字记得处理
	}

	void ModifyAmmo () {
		if (Player.CurrentWeapon.Property.Action == WeaponAction.Melee)
			AmmoLable.gameObject.SetActive (false);
		else
			AmmoLable.gameObject.SetActive (true);
		AmmoText.text = Player.CurrentWeapon.Property.AmmoInClip + "/" + Player.CurrentWeapon.Property.MaxAmmo;
	}

	public void HitFeedback(Unit Target,float Damage){
		AudioManager.Instance.PlayHitFeedbackSound ();
		HitFeedbackNumberUI text = GameObject.Instantiate (HitFeedbackUI.gameObject,BattleUI).GetComponent<HitFeedbackNumberUI> ();
		text.transform.position =Camera.main.WorldToScreenPoint (Target.transform.position);
		text.Setup (Damage);
	}

	public void ActiveArms () {
		ArmsChosen.gameObject.SetActive (true);
		GameCamera.gameObject.SetActive (true);
	}

	public void DeActiveArms () {
		ArmsChosen.gameObject.SetActive (false);
		GameCamera.gameObject.SetActive (false);
	}

	public static string GetLocalHostIP() {
		string hostName = Dns.GetHostName();
		IPAddress[] ipAddrs = Dns.GetHostAddresses(hostName);
		foreach(IPAddress adress in ipAddrs)
		{
			string[] checker = adress.ToString().Split('.');
			if(checker.Length == 4)
				return adress.ToString();
		}

		return null;
	}
}
