var websocket = require('ws');
const sqlite = require('sqlite3').verbose();

var websocketServer = new websocket.Server({port:5500}, ()=>{
    console.log("Owen's Server is running");
});

var wsList = [];
var roomList = [];

var db = new sqlite.Database('./database/chatDB.db', sqlite.OPEN_CREATE | sqlite.OPEN_READWRITE, (err)=>{

    if(err) throw err;

    console.log('Connected to database.');

    websocketServer.on("connection", (ws, rq)=>{
    
        console.log('Client connected');

            ws.on("message", (data)=>{
                console.log("Send from client : "+data);

                var toJsonObj = {
                    eventName:"Login",
                    data:"test1111#111111#test1"
                }
                toJsonObj = JSON.parse(data); 
  
                var splitStr = toJsonObj.data.split('#');
                var userID = splitStr[0];
                var password = splitStr[1];
                var name = splitStr[2];

                var sqlSelect = "SELECT * FROM UserData WHERE UserID='"+userID+"' AND Password='"+password+"'";
                var sqlInsert = "INSERT INTO UserData (UserID, Password, Name) VALUES ('"+userID+"', '"+password+"', '"+name+"')";

                if (toJsonObj.eventName == "Login") 
                {
                    db.all (sqlSelect, (err, rows)=>{
                        if (err)
                        {
                            console.log("[0]" + err);
                        }
                        else
                        {
                            if (rows.length > 0)
                            {
                                console.log("=====[1]=====");
                                console.log(rows);
                                console.log("=====[1]=====");

                                var callbackMsg = {
                                    eventName:"Login",
                                    data:"success#"+rows[0].Name
                                }

                                var toJsonStr = JSON.stringify(callbackMsg);
                                console.log("[2]" + toJsonStr);
                                console.log("Login success!");
                                ws.send(toJsonStr);
                            }
                            else
                            {
                                var callbackMsg = {
                                    eventName:"Login",
                                    data:"fail"
                                }

                                var toJsonStr = JSON.stringify(callbackMsg);
                                console.log("[3]" + toJsonStr);
                                console.log("Login fail!");
                                ws.send(toJsonStr);
                            }
                        }
                    });
                }
                else if (toJsonObj.eventName == "Register")
                {
                    db.all (sqlInsert, (err, rows)=>{
                        if (err)
                        {
                            var callbackMsg = {
                                eventName:"Register",
                                data:"fail"
                            }
                
                            var toJsonStr = JSON.stringify(callbackMsg);
                            console.log("[4]" + toJsonStr);
                            console.log("Register fail!");
                            ws.send(toJsonStr);
                        }
                        else
                        {
                            var callbackMsg = {
                                eventName:"Register",
                                data:"success"
                            }
                
                            var toJsonStr = JSON.stringify(callbackMsg);
                            console.log("[5]" + toJsonStr);
                            console.log("Register success!");
                            ws.send(toJsonStr);
                        }
                    });
                }

                //Lobby
                else if (toJsonObj.eventName == "CreateRoom") 
                {
                    var isFoundRoom = false;

                    for (var i = 0; i < roomList.length; i++)
                    {
                        if (roomList[i].roomName == toJsonObj.data)
                        {
                            isFoundRoom = true;
                            break;
                        }
                    }

                    if (isFoundRoom == true)
                    {
                        var callbackMsg = {
                            eventName:"CreateRoom",
                            data:"fail",
                        }

                        console.log("eventName " + callbackMsg.eventName + " result " + callbackMsg.data);
                        var toJsonStr = JSON.stringify(callbackMsg);
                        ws.send(toJsonStr);

                        console.log("Client create room fail.");
                    }
                    else
                    {

                        var newRoom ={
                            roomName: toJsonObj.data,
                            wsList: []
                        }

                        newRoom.wsList.push(ws);
                        roomList.push(newRoom);

                        var callbackMsg = {
                            eventName:"CreateRoom",
                            data:"success",
                        }

                        console.log("eventName " + callbackMsg.eventName + " result " + callbackMsg.data);
                        var toJsonStr = JSON.stringify(callbackMsg);
                        ws.send(toJsonStr);

                        console.log("Client create room success.");
                    }

                    console.log("Client request CreateRoom ["+toJsonObj.data+"]");

                }
                else if (toJsonObj.eventName == "JoinRoom")
                {
                    var isFoundRoom = false;

                    for (var i = 0; i < roomList.length; i++)
                    {
                        if (roomList[i].roomName == toJsonObj.data)
                        {   
                            isFoundRoom = true;
                            roomList[i].wsList.push(ws);
                            break;
                        }
                    }

                    if (isFoundRoom == true)
                    {
                        var callbackMsg = {
                            eventName:"JoinRoom",
                            data:"success",
                        }
                        console.log("eventName " + callbackMsg.eventName + " result " + callbackMsg.data);
                        var toJsonStr = JSON.stringify(callbackMsg);
                        ws.send(toJsonStr);

                        console.log("Client join room success.");
                        console.log("Client request JoinRoom ["+toJsonObj.data+"]");
                    }
                    else
                    {
                        var callbackMsg = {
                            eventName:"JoinRoom",
                            data:"fail",
                        }

                        console.log("eventName " + callbackMsg.eventName + " result " + callbackMsg.data);
                        var toJsonStr = JSON.stringify(callbackMsg);
                        ws.send(toJsonStr);

                        console.log("Client join room fail.");
                    }

                }
                else if (toJsonObj.eventName == "LeaveRoom") 
                {
                    var isLeaveSuccess = false; 
                    for (var i = 0; i < roomList.length; i++) 
                    {
                        for (var j = 0; j < roomList[i].wsList.length; j++) 
                        {
                            if (ws == roomList[i].wsList[j]) 
                            {
                                roomList[i].wsList.splice(j, 1); 

                                if (roomList[i].wsList.length <= 0) 
                                {
                                    roomList.splice(i, 1); 
                                }

                                isLeaveSuccess = true;
                                break;
                            }
                        }
                    }

                    if (isLeaveSuccess)
                    {
                        var callbackMsg = {
                            eventName:"LeaveRoom",
                            data:"success"
                        }

                        var toJsonStr = JSON.stringify(callbackMsg);
                        ws.send(toJsonStr);
    
                        console.log("Leave room success.");
                    }
                    else
                    {
                        var callbackMsg = {
                            eventName:"LeaveRoom",
                            data:"fail"
                        }

                        var toJsonStr = JSON.stringify(callbackMsg);
                        ws.send(toJsonStr);
    
                        console.log("Leave room fail.");
                    }
                }

                else if (toJsonObj.eventName == "SendMessage")
                {
                    console.log("Send message from client : "+data);
                    Boardcast(ws, data);
                }
            });
    

        ws.on("close", ()=>{
            wsList = ArrayRemove(wsList, ws);
            console.log("Client disconnected.");

            for(var i = 0; i < roomList.length; i++)
            {
                for(var j = 0; j < roomList[i].wsList.length; j++)
                {
                    if(ws == roomList[i].wsList[j])
                    {
                        roomList[i].wsList.splice(j, 1);

                        if(roomList[i].wsList.length <= 0)
                        {
                            roomList.splice(i, 1);
                        }
                        break;
                    }
                }
            }
        });
    });
});

function ArrayRemove(arr, value)
{
    return arr.filter((element)=>{
        return element != value;
    })
}

function Boardcast(ws, message)
{
    var selectRoomIndex = -1;

    for (var i = 0; i < roomList.length; i++)
    {
        for (var j = 0; j < roomList[i].wsList.length; j++)
        {
            if (ws == roomList[i].wsList[j])
            {
                selectRoomIndex = i;
                console.log(selectRoomIndex);
                break;
            }
        }
    }

    for (var i = 0; i < roomList[selectRoomIndex].wsList.length; i++)
    {
        roomList[selectRoomIndex].wsList[i].send(message);
    }
}