resource_manifest_version "44febabe-d386-4d18-afbe-5e627f4af937"
description "Main resource"
version "1"

client_scripts {
	"client/Client.net.dll"
}

server_scripts {
    "server/Newtonsoft.Json.net.dll",
    "server/System.Buffers.net.dll",
    "server/System.Runtime.InteropServices.RuntimeInformation.net.dll",
    "server/System.Threading.Tasks.Extensions.net.dll",
    "server/MySqlConnector.net.dll",
	"server/Server.net.dll"
}