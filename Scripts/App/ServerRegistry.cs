
using System.Collections;
using UnityEngine;
using System;
using Newtonsoft.Json;
using System.Net;
using System.Net.Sockets;
using System.Linq;





[Serializable]
public class ServerEntry{
    public string name;
    public string address;
    public int port;
    public string protocol;

}

public class ServerRegistry : MonoBehaviour{

    public RESTInterface serverInterface;
    public ClientConfig _config;
    public ServerEntry  defaultServerEntry;

    void Start(){
       InitInterface();
    }

    public void SetConfig(ClientConfig config){
        _config = config;
       InitInterface();
        
    }

    public void InitInterface(){
        if(serverInterface ==null) serverInterface = new RESTInterface();
        serverInterface.protocol = _config.serverRegistryConfig.protocol;
        serverInterface.url = _config.serverRegistryConfig.url;
        serverInterface.port = _config.serverRegistryConfig.port;
        serverInterface.usePort= _config.serverRegistryConfig.usePort;
        serverInterface.usePortWorkAround= _config.serverRegistryConfig.usePortWorkAround;
        if (_config.serverRegistryConfig.defaultServer!= ""){
            var address = _config.serverRegistryConfig.defaultServer;
            defaultServerEntry = new ServerEntry(){name = address, address = address, port=_config.port, protocol=_config.protocol };
        }
    }

    public void RegisterServer(){
        Debug.Log("register server");
        var serverEntry = getServerEntry();
        string data = "";
        var setting = new JsonSerializerSettings();
        data = JsonConvert.SerializeObject(serverEntry, setting);
        StartCoroutine(serverInterface.sendRequestCoroutine("dance_servers/add", data, PrintResponse));
    }

    public void UnregisterServer(){
        var serverEntry = getServerEntry();
        string data = "";
        var setting = new JsonSerializerSettings();
        data = JsonConvert.SerializeObject(serverEntry, setting);
        StartCoroutine(serverInterface.sendRequestCoroutine("dance_servers/remove", data, PrintResponse));
    }

    public void FillServerList(System.Action<string> HandleServerList){
        
        StartCoroutine(serverInterface.sendGETRequestCoroutine("dance_servers", HandleServerList));
    }

    public ServerEntry getServerEntry(){
        var url = LocalIPAddress();
        var serverEntry = new ServerEntry(){
            name = url,
            address = url,
            port = _config.port,
            protocol = _config.protocol
        };
        return serverEntry;
    }
        

    //https://answers.unity.com/questions/1731994/get-the-device-ip-address-from-unity.html
    public static string LocalIPAddress()
    {
         return Dns.GetHostEntry(Dns.GetHostName())
             .AddressList.First(
                 f => f.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
             .ToString();
    }    

    public void PrintResponse(string responseText){
        Console.WriteLine(responseText);
    }
}
