using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WebSocketSharp;
using Random = UnityEngine.Random;

namespace ProgramChat
{
    public class WebSocketConnection : MonoBehaviour
    {
        public Transform panelBox;
        
        public GameObject textBoxPrefab;
        public GameObject connectUI;
        public GameObject chatUI;
        
        public InputField ipAddressInput;
        public InputField portInput;
        public InputField usernameInput;
        public InputField chatTextInput;
        
        private WebSocket _webSocket;
        // Start is called before the first frame update
        void Start()
        {

        }
    
        // Update is called once per frame
        void Update()
        {
            //if (Input.GetKeyDown(KeyCode.Return))
            //{
            //    
            //}
        }

        public void SendChatInfo()
        {
            if (_webSocket.ReadyState == WebSocketState.Open)
            {
                //_webSocket.Send(("Random Number : " + Random.Range(0, 999999)));
                _webSocket.Send(usernameInput.text + " : " + chatTextInput.text);
                chatTextInput.text = "";
            }
        }

        private void OnDestroy()
        {
            if (_webSocket != null)
            {
                _webSocket.Close();
            }
        }

        public void OnMessage(object sender, MessageEventArgs messageEventArgs)
        {
            var newChatBox = Instantiate(textBoxPrefab, panelBox);
                newChatBox.GetComponent<Text>().text = messageEventArgs.Data + "\n";
                string[] messages = messageEventArgs.Data.Split(':');

                newChatBox.transform.SetSiblingIndex(0);

                if (usernameInput.text + " " == messages[0])
                {
                    newChatBox.GetComponent<Text>().alignment = TextAnchor.LowerRight;
                }

                Debug.Log("Receive msg : " + messageEventArgs.Data);
        }

        public void UserInput()
        {
            //127.0.0.1//5500
            _webSocket = new WebSocket("ws://" + ipAddressInput.text + ":" + portInput.text + "/"); //Connect to Node Js

            _webSocket.OnMessage += OnMessage;
            
            _webSocket.Connect();
            
            if (_webSocket.ReadyState == WebSocketState.Open)
            {
                connectUI.SetActive(false);
                chatUI.SetActive(true);
            }
            //_webSocket.Send("I'm coming here");
        }
    }
}

