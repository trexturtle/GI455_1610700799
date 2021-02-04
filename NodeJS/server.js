var websocket = require('ws');

var callbackInitServer = ()=>{
    console.log("Owen Server is Running");
}

var wss = new websocket.Server({port:5500}, callbackInitServer);

var wsList = [];

wss.on("connection", (ws)=>{

    console.log("Client connected.");
    wsList.push(ws);

    ws.on("message", (data)=>{

        console.log("Send from client :" + data)
        Broardcast(data);
    });

    ws.on("close", ()=>{

        console.log("Client Disconnected.")
    });


});

function ArrayRemove(arr, value)
{
    return arr.fillter((element)=>
    {
        return element != value;
    });
}

function Broardcast(data)
{
    for (var i = 0; i < wsList.length; i++) 
    {
        wsList[i].send(data);
    }
}


