using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ExitGames.Client.Photon.Chat;

public class Chat : MonoBehaviour, IChatClientListener {
	public GameObject loginForm;
	public GameObject chatForm;

	public GameObject messagesContainer;
	public ChatClient chatClient;
	public InputField usernameText;
	public Text connectionStateText;
	public Text messagesText;
	public InputField messageText;
	string worldChat;

	void Start() {
		Application.runInBackground = true;

		loginForm.SetActive(true);

		HandleLoginFormUI();
		HandleChatFormUI();
	}
	
	void Update() {
		if(chatClient != null) {
			chatClient.Service();
			connectionStateText.text = chatClient.State.ToString();
		}
		else {
			connectionStateText.text = "Offline";
		}

		HandleMessageSubmit();
	}

	void HandleMessageSubmit() {
		if(!chatForm.activeSelf) return;

		if(messageText.text != "" && Input.GetKey(KeyCode.Return)) {
			chatClient.PublishMessage(worldChat, messageText.text);
			messageText.text = "";

			StartCoroutine(CoFocusAgain());
		}
	}

	IEnumerator CoFocusAgain() {
		yield return new WaitForSeconds(.1f);

		messageText.Select();
		messageText.ActivateInputField();
	}

	void HandleLoginFormUI() {
		transform.Find("LoginForm/ConnectButton").GetComponent<Button>().onClick.AddListener(Connect);
	}

	void HandleChatFormUI () {
		transform.Find("ChatForm/ExitButton").GetComponent<Button>().onClick.AddListener(() => {
			chatClient.Disconnect();
		});
	}

	void Connect() {
		if(string.IsNullOrEmpty(PhotonNetwork.PhotonServerSettings.ChatAppID)) {
			print("No ChatAppID provided");
			return;
		}

		worldChat = "WORLD_CHAT";

		chatClient = new ChatClient(this);
		chatClient.Connect(PhotonNetwork.PhotonServerSettings.ChatAppID, "0.0.1", new ExitGames.Client.Photon.Chat.AuthenticationValues(usernameText.text));
	}

	public void OnConnected() {
		loginForm.SetActive(false);
		chatForm.SetActive(true);

		messagesText.text = "";

		chatClient.Subscribe(new string[]{ worldChat });
		chatClient.SetOnlineStatus(ChatUserStatus.Online);
	}

	public void OnDisconnected() {
		loginForm.SetActive(true);
		chatForm.SetActive(false);
	}

	public void OnGetMessages(string channelName, string[] senders, object[] messages) {
		for(int i = 0; i < senders.Length; i++) {
			messagesText.text = messagesText.text + "\n"
								+ senders[i] + ": "
								+ messages[i];
		}

		// Scroll to bottom
		StartCoroutine(CoScrollToBottom());
	}

	IEnumerator CoScrollToBottom() {
		yield return new WaitForSeconds(.1f);
		messagesContainer.GetComponent<ScrollRect>().normalizedPosition = new Vector2(0, 0);
	}

	public void OnPrivateMessage(string sender, object message, string channelName) { }

	public void OnSubscribed(string[] channels, bool[] results) {
		messagesText.text += "Chat Online.";
		chatClient.PublishMessage(worldChat, "Joined");
	}

	public void OnUnsubscribed(string[] channels) { }

	public void OnStatusUpdate(string user, int status, bool gotMessage, object message) { }

	public void OnChatStateChange(ChatState state) { }

	public void DebugReturn(ExitGames.Client.Photon.DebugLevel level, string message) { }

	void OnApplicationQuit() {
		if(chatClient != null) {
			chatClient.Disconnect();
		}
	}
}
