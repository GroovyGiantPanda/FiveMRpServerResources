resource_manifest_version "44febabe-d386-4d18-afbe-5e627f4af937"
description "Chat"
version "1"

client_script "client/Chat.Client.net.dll"
server_script "server/Chat.Server.net.dll"

ui_page "html/chat.html"

files({
    "html/chat.html",
    "html/chat.css",
    "html/chat.js",
    "html/jquery.faketextbox.js"
})