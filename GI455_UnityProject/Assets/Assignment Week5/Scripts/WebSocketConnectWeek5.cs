using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WebSocketSharp;
using UnityEngine.UI;
using System;

namespace Assignment_Week5.scripts
{
    public class WebSocketConnectWeek5 : MonoBehaviour
    {
        private class MessageData
        {
            public string Username;
            public string Message;
        }

        public struct SocketEvent
        {
            public string eventName;
            public string data;

            public SocketEvent(string eventName, string data)
            {
                this.eventName = eventName;
                this.data = data;
            }
        }

        public GameObject rootConnection;
        public GameObject rootLogin;
        public GameObject rootRegister;
        
        public GameObject rootLoginFail;
        public GameObject rootLoginInputFail;
        public GameObject rootRegisterFail;
        public GameObject rootRegisNotMatch;
        public GameObject rootRegisInputFail;
        public GameObject rootCreateFail;
        public GameObject rootJoinFail;
                
        public GameObject rootLobby;
        public GameObject rootCreateRoom;
        public GameObject rootJoinRoom;
        public GameObject rootMessenger;

        public InputField inputMsg;
        public InputField inputID;
        public InputField inputPassword;
        public InputField inputRegisID;
        public InputField inputRegisName;
        public InputField inputRegisPassword;
        public InputField inputRetryPassword;
        public InputField inputCreateRoom;
        public InputField inputJoinRoom;
        
        private string _join;
        public string Name;

        private MessageData _msgJson;

        public Text roomName;
        public Text sendText;
        public Text receiveText;

        private WebSocket _ws;

        private string _tempMessageString;

        public delegate void DelegateHandle(SocketEvent result);
        public DelegateHandle OnLoginRoom;
        public DelegateHandle OnRegisterRoom;
        public DelegateHandle OnCreateRoom;
        public DelegateHandle OnJoinRoom;
        public DelegateHandle OnLeaveRoom;

        public void Start()
        {
            rootConnection.SetActive(true);
            rootLogin.SetActive(false);
            rootLoginFail.SetActive(false);
            rootRegister.SetActive(false);
            rootRegisterFail.SetActive(false);
            rootLobby.SetActive(false);
            rootCreateRoom.SetActive(false);
            rootCreateFail.SetActive(false);
            rootJoinRoom.SetActive(false);
            rootJoinFail.SetActive(false);
            rootMessenger.SetActive(false);
            rootRegisNotMatch.SetActive(false);
            rootRegisInputFail.SetActive(false);
            rootLoginInputFail.SetActive(false);

            OnCreateRoom += CreateRoomHandler;
            OnJoinRoom += JoinRoomHandler;
            OnLoginRoom += LoginHandler;
            OnRegisterRoom += RegisterHandler;
        }

        private void Update()
        {
            UpdateNotifyMessage();
        }

        public void Connect()
        {
            var url = $"ws://127.0.0.1:5500/";

            _ws = new WebSocket(url);

            _ws.OnMessage += OnMessage;

            _ws.Connect();

            rootConnection.SetActive(false);
            rootLogin.SetActive(true);
        }

        private bool _backwardLoginFail;
        private bool _backwardLoginInputFail;
        private bool _backwardRegisterFail;
        private bool _backwardRegisterInputFail;
        private bool _backwardRegisterNotMatch;
        private bool _backwardCreateFail;
        private bool _backwardJoinFail;

        public void GoBackPage()
        {
            if (_backwardLoginFail)
            {
                rootLogin.SetActive(true);
                rootLoginFail.SetActive(false);
                _backwardLoginFail = false;
            }
            if (_backwardLoginInputFail)
            {
                rootLogin.SetActive(true);
                rootLoginInputFail.SetActive(false);
                _backwardLoginInputFail = false;
            }
            if (_backwardRegisterFail)
            {
                rootRegister.SetActive(true);
                rootRegisterFail.SetActive(false);
                _backwardRegisterFail = false;
            }
            if (_backwardRegisterInputFail)
            {
                rootRegister.SetActive(true);
                rootRegisInputFail.SetActive(false);
                _backwardRegisterInputFail = false;
            }
            if (_backwardRegisterNotMatch)
            {
                rootRegister.SetActive(true);
                rootRegisNotMatch.SetActive(false);
                _backwardRegisterNotMatch = false;
            }
            if (_backwardCreateFail)
            {
                rootCreateRoom.SetActive(true);
                rootCreateFail.SetActive(false);
                _backwardCreateFail = false;
            }

            if (!_backwardJoinFail) return;
            rootJoinRoom.SetActive(true);
            rootJoinFail.SetActive(false);
            _backwardJoinFail = false;
        }

        private void LoginHandler(SocketEvent receive)
        {
            var receiveArr = receive.data.Split('#');
            if (receive.data == "fail")
            {
                _backwardLoginFail = true;
                rootLoginFail.SetActive(true);
            }
            else
            {
                Name = receiveArr[1];
                rootLogin.SetActive(false);
                rootLobby.SetActive(true);
            }
        }

        public void Login(string login)
        {
            if (_ws.ReadyState != WebSocketState.Open) return;
            if (inputID.text == "" || inputPassword.text == "")
            {
                _backwardLoginInputFail = true;
                rootLoginInputFail.SetActive(true);
            }
            else
            {
                login = inputID.text + "#" + inputPassword.text;

                var socketEvent = new SocketEvent("Login", login);

                var toJsonStr = JsonUtility.ToJson(socketEvent);

                _ws.Send(toJsonStr);
            }
        }

        private void RegisterHandler(SocketEvent receive)
        {
            switch (receive.data)
            {
                case "fail":
                    _backwardRegisterFail = true;
                    rootRegisterFail.SetActive(true);
                    break;
                default:
                    rootRegister.SetActive(false);
                    rootLogin.SetActive(true);
                    break;
            }
        }

        public void Register(string register)
        {
            if (_ws.ReadyState != WebSocketState.Open) return;
            if (inputRegisID.text == "" || inputRegisPassword.text == "" || inputRegisName.text == "" || inputRetryPassword.text == "")
            {
                _backwardRegisterInputFail = true;
                rootRegisInputFail.SetActive(true);
            }
            else
            {
                if (inputRetryPassword.text == inputRegisPassword.text)
                {
                    register = inputRegisID.text + "#" + inputRegisPassword.text + "#" + inputRegisName.text;

                    var socketEvent = new SocketEvent("Register", register);

                    var toJsonStr = JsonUtility.ToJson(socketEvent);

                    _ws.Send(toJsonStr);
                }
                else
                {
                    _backwardRegisterNotMatch = true;
                    rootRegisNotMatch.SetActive(true);
                }
            }
        }

        public void GotoRegister()
        {
            rootLogin.SetActive(false);
            rootRegister.SetActive(true);
        }

        public void GoToCreateRoom()
        {
            rootLobby.SetActive(false);
            rootCreateRoom.SetActive(true);
        }

        private void CreateRoomHandler(SocketEvent receive)
        {
            if (receive.data == "fail")
            {
                _backwardCreateFail = true;
                rootCreateFail.SetActive(true);
            }
            else
            {
                rootMessenger.SetActive(true);
                rootCreateRoom.SetActive(false);
            }
        }

        private void JoinRoomHandler(SocketEvent receive)
        {
            if (receive.data == "success")
            {
                rootMessenger.SetActive(true);
                rootCreateRoom.SetActive(false);
            }
            else
            {
                _backwardJoinFail = true;
                rootJoinFail.SetActive(true);
            }
        }

        public void CreateRoom(string roomName)
        {
            if (_ws.ReadyState == WebSocketState.Open)
            {
                var socketEvent = new SocketEvent("CreateRoom", "Room : " + inputCreateRoom.text);

                var toJsonStr = JsonUtility.ToJson(socketEvent);

                _ws.Send(toJsonStr);
            }

            this.roomName.text = "Room : " + inputCreateRoom.text;
        }

        public void GoToJoinRoom()
        {
            rootLobby.SetActive(false);
            rootJoinRoom.SetActive(true);
        }

        public void JoinRoom(string roomName)
        {
            if (_ws.ReadyState == WebSocketState.Open)
            {
                var socketEvent = new SocketEvent("JoinRoom", "Room : " + inputJoinRoom.text);

                var toJsonStr = JsonUtility.ToJson(socketEvent);

                _ws.Send(toJsonStr);
            }
            
            _join = inputJoinRoom.text;
            this.roomName.text = "Room : " + inputJoinRoom.text;
        }

        public void LeaveRoom()
        {
            if (_ws.ReadyState == WebSocketState.Open)
            {
                var socketEvent = new SocketEvent("LeaveRoom", _join);

                var toJsonStr = JsonUtility.ToJson(socketEvent);

                _ws.Send(toJsonStr);
            }
            receiveText.text = "";
            sendText.text = "";
            rootMessenger.SetActive(false);
            rootCreateRoom.SetActive(false);
            rootJoinRoom.SetActive(false);
            rootLobby.SetActive(true);
        }

        private void Disconnect()
        {
            if (_ws != null)
                _ws.Close();
        }

        public void SendMessage()
        {
            if (inputMsg.text == "" || _ws.ReadyState != WebSocketState.Open)
                return;

            var newMessageData = new MessageData();
            newMessageData.Username = Name;
            newMessageData.Message = inputMsg.text;

            var toJsonStr = JsonUtility.ToJson(newMessageData);

            SocketEvent socketEvent = new SocketEvent("SendMessage", toJsonStr);

            var _toJsonStr = JsonUtility.ToJson(socketEvent);

            _ws.Send(_toJsonStr); 

            inputMsg.text = "";
        }

        private void OnDestroy()
        {
            Disconnect();
        }

        private void UpdateNotifyMessage()
        {
            if (string.IsNullOrEmpty(_tempMessageString) != false) return;
            var receiveMessageData = JsonUtility.FromJson<SocketEvent>(_tempMessageString);
                

            switch (receiveMessageData.eventName)
            {
                case "Login":
                {
                    OnLoginRoom?.Invoke(receiveMessageData);
                    break;
                }
                case "Register":
                {
                    OnRegisterRoom?.Invoke(receiveMessageData);
                    break;
                }
                case "CreateRoom":
                {
                    OnCreateRoom?.Invoke(receiveMessageData);
                    break;
                }
                case "JoinRoom":
                {
                    OnJoinRoom?.Invoke(receiveMessageData);
                    break;
                }
                case "SendMessage":
                {
                    var _receiveMessageData = JsonUtility.FromJson<MessageData>(receiveMessageData.data);

                    if (_receiveMessageData.Username == Name)
                    {
                        print(_receiveMessageData.Username);
                        receiveText.text += "\n";
                        sendText.text += _receiveMessageData.Username + ": " + _receiveMessageData.Message + "\n";
                    }
                    else
                    {
                        sendText.text += "\n";
                        receiveText.text += _receiveMessageData.Username + ": " + _receiveMessageData.Message + "\n";
                    }

                    break;
                }
                case "LeaveRoom":
                {
                    OnLeaveRoom?.Invoke(receiveMessageData);
                    break;
                }
            }

            _tempMessageString = "";
        }

        private void OnMessage(object sender, MessageEventArgs messageEventArgs)
        {
            _tempMessageString = messageEventArgs.Data;
            Debug.Log(messageEventArgs.Data);
        }
    }
}


