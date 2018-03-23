var Discord = require('discord.js')
var co = require('co')
const whisperLog = require('simple-node-logger').createSimpleLogger('whispers.log')
const crawlLog = require('simple-node-logger').createSimpleLogger('forumCrawl.log')
const client = new Discord.Client()
const fs = require('fs')
const _ = require('lodash')
var ping = require('ping')
var yaml = require('js-yaml')

const greenColorId = 50000
const redColorId = 16711680
var numTeamSpeakUsers = 'N/A'

var numForumUsersOnline = 'N/A'
var numTotalForumMembers = 'N/A'
var numPlayersOnline = 'N/A'
var playerList = 'N/A'

var config = yaml.safeLoad(fs.readFileSync('config.yaml', 'utf8'))

Object.size = function(obj) {
    var size = 0, key;
    for (key in obj) {
        if (obj.hasOwnProperty(key)) size++;
    }
    return size;
};

client.on('ready', () => {
  console.log('I am ready!')
  init();
});

client.login(config.DiscordBot.DiscordAccount.Token)

var guild = null
const whitelistRoleId = config.DiscordBot.DiscordAccount.WhitelistRoleId
// r => r.name === 'Whitelist'

client.on('message', message => {
	if(message.channel.type == 'dm' && message.author.username != config.DiscordBot.DiscordAccount.BotUsername)
	{
    	co(function*(){
    	member = yield guild.fetchMember(message.author.id)
    	var isWhitelisted = Array.from(member.roles.filter(r => r.id === whitelistRoleId).keys()).length > 0
    	if(isWhitelisted)
    	{
    		whisperLog.info(`${message.author.username} whispered me and is whitelisted`)
    		message.reply(`It is highly recommended to download the below shortcut file and run that to connect directly to the server.\nCHANGE FILE ENDING \`.download\` to \`.LNK\`!\n\nIP is no longer given out as a direct link; ask around on how to use the shortcut if you are having issues.`, {files: [config.DiscordBot.PathToLNKfile]})
    	}
    	else
    	{
    		message.reply(`${_.sample(['Meow?', 'Meow!', 'Meow.', 'BeepBoop.'])} (If you are looking for the IP, you need a whitelist tag first.)`)
    		whisperLog.info(`${message.author.username} whispered me and is NOT whitelisted`)
    	}
    })
	}

});

var embedMessages = {'FiveM Server' : {}, 'TeamSpeak' : {}, 'Forums' : {}, 'Twitch Streams' : {}}
var statusChannel = null

function init()
{
	co(function* () {
		guild = client.guilds.find(v => v.name === `TheFamilyRP`)
		statusChannel = guild.channels.find(v => v.name === 'status' && v.type === 'text')
		if(!statusChannel)
			console.error('Could not find channel!')
		messages = yield statusChannel.fetchMessages()
		messagesSimple = messages
			.filter(p => p.embeds.length > 0)
			.map(m => ({[m.embeds[0].title != undefined ? m.embeds[0].title : 'Twitch Streams']: m.id}))
			.forEach((v, k) => embedMessages[Object.keys(v)[0]].id = v[Object.keys(v)[0]])
			console.log(messagesSimple)
		updateEmbeds()
		setInterval(updateEmbeds, 30000)
	})
}

function updateEmbeds()
{
	Object.keys(embedMessages).forEach(k => {
		co(function* () {
			if(!embedMessages[k].id)
			{
				console.log(`Could not find message for ${k}! Creating.`)
				t = yield statusChannel.send({embed: {color: 3447003, "author": {"name": k}}})
				embedMessages[k].id = t.id
				embedMessages[k].message = t
			}
			else
			{
				embedMessages[k].message = yield statusChannel.fetchMessage(embedMessages[k].id)
				console.log(`Found message for ${k}!`)
			}
			if(embedMessages[k].embedJson)
			{
				console.log(embedMessages[k].embedJson)
				embedMessages[k].message.edit(embedMessages[k].embedJson)
				console.log(`Updated to new JSON for ${k}`)
			}
			else
				console.log(`Lacking JSON for ${k}`)
		})
	})
}
 
var hosts = ['ts3.thefamilyrp.com', 'www.thefamilyrp.com', config.ServerIp]
var hostsLastStatus = [null, null, null]
var hostsLastStatusChangeTime = [null, null, null]

if(fs.existsSync('state.json'))
{
	var jsonState = JSON.parse(fs.readFileSync('state.json', 'utf8'))
	hostsLastStatus = jsonState.hostsLastStatus
	hostsLastStatusChangeTime = jsonState.hostsLastStatusChangeTime.map(t => new Date(t))
}

function checkPing(){
	hosts.forEach(function (host, k) {
	    ping.promise.probe(host).then(function (r) {
	    	console.log(r.host)
	        console.log(r.time)
	        console.log(r.alive)
	        if(hostsLastStatus[k] != r.alive)
	        {
	        	hostsLastStatusChangeTime[k] = new Date()
	        	hostsLastStatus[k] = r.alive
	        }

			var dateDiff = new Date().getTime()-hostsLastStatusChangeTime[k].getTime()
			console.log(dateDiff)
			var diffDays = Math.floor(dateDiff/(1000*60*60*24))
			dateDiff -= diffDays*1000*60*60*24
			var diffHours = Math.floor(dateDiff/(1000*60*60))
			dateDiff -= diffHours*1000*60*60
			var diffMinutes = Math.floor(dateDiff/(1000*60))
			if(diffDays === 0)
				var duration = diffHours > 0 ? `${diffHours}h ${diffMinutes}m` : `${diffMinutes}m`
			else
				var duration = diffHours > 0 ? `${diffDays}d ${diffHours}h` : `${diffDays}d`

			if(r.host == 'ts3.thefamilyrp.com')
			{
				embedMessages['TeamSpeak'].embedJson = {
					"embed": {
						"color": r.alive ? greenColorId : redColorId,
						"timestamp": new Date().toISOString(),
						"title": "TeamSpeak",
						"fields": [
							{
								"name": "Status",
								"value": r.alive ? "Up" : "Down",
								"inline": true
							},
							{
								"name": "Duration",
								"value": duration,
								"inline": true
							},
							{
								"name": "Ping",
								"value": `${r.time}ms`,
								"inline": true
							},
							{
								"name": "Connected Users",
								"value": numTeamSpeakUsers,
								"inline": true
							// }
							},
							{
								"name": "Address",
								"value": `xxx.thefamilyrp.com`,
								"inline": true
							}
						]
					}
				}
			}
			if(r.host == config.ServerIp)
			{
				embedMessages['FiveM Server'].embedJson = {
					"embed": {
						"color": r.alive ? greenColorId : redColorId,
						"timestamp": new Date().toISOString(),
						"title": "FiveM Server",
						"fields": [
							{
								"name": "Status",
								"value": r.alive ? "Up" : "Down",
								"inline": true
							},
							{
								"name": "Duration",
								"value": duration,
								"inline": true
							},
							{
								"name": "Ping",
								"value": `${r.time}ms`,
								"inline": true
							},
							{
								"name": "Players Online",
								"value": numPlayersOnline,
								"inline": true
							},
							{
								"name": "Address",
								"value": "CHANGED! Whitelisted? Whisper me",
								"inline": true
							},
							{
								"name": "Player List",
								"value": playerList
							}
						]
					}
				}
			}
			if(r.host == 'www.thefamilyrp.com')
			{
				embedMessages['Forums'].embedJson = {
					"embed": {
						"color": r.alive ? greenColorId : redColorId,
						"timestamp": new Date().toISOString(),
						"title": "Forums",
						"fields": [
							{
								"name": "Status",
								"value": r.alive ? "Up" : "Down",
								"inline": true
							},
							{
								"name": "Duration",
								"value": duration,
								"inline": true
							},
							{
								"name": "Ping",
								"value": `${r.time}ms`,
								"inline": true
							},
							{
								"name": "Registered Users",
								"value": numTotalForumMembers,
								"inline": true
							},
							{
								"name": "Online Users",
								"value": numForumUsersOnline,
								"inline": true
							},
							{
								"name": "Address",
								"value": `https://www.thefamilyrp.com`,
								"inline": true
							}
						]
					}
				}
			}
		})
	})

	fs.writeFile("state.json", JSON.stringify({hostsLastStatus: hostsLastStatus, hostsLastStatusChangeTime: hostsLastStatusChangeTime}), 'utf8', function (err) {
	    if (err)
	        return console.log(err)
	    console.log("The file was saved!")
	})
}

checkPing()
setInterval(checkPing, 3000)

var request = require('request')
 
var accessToken = config.DiscordBot.TwitchAccount.AccessToken
var clientId = config.DiscordBot.TwitchAccount.ClientId

function checkTwitch()
{
	request.get({url: 'https://api.twitch.tv/kraken/streams/followed',  headers: {'Accept': 'application/vnd.twitchtv.v5+json', 'Authorization': `OAuth ${accessToken}`, 'Client-ID': clientId}},
		function(err, res, body) {
	 	var JSONsuccess = false
	 	var r
	 	try{
			r = JSON.parse(body).streams.slice(0, 30)
			JSONsuccess = true
		}
		catch(err)
		{
			console.log('JSON issue! Trying again next time.')
		}
		if(JSONsuccess)
		{
			r = r.map(function(r)
			{
				var q = {
						displayName: r.channel.display_name,
						viewers: r.viewers,
						status: r.channel.status,
						url: r.channel.url,
						game: r.game
					}

				var dateDiff = new Date().getTime()-new Date(r.created_at).getTime()
				var diffHours = Math.floor(dateDiff/(1000*60*60))
				dateDiff -= diffHours*1000*60*60
				var diffMinutes = Math.floor(dateDiff/(1000*60))
				q.uptime = diffHours > 0 ? `${diffHours}h ${diffMinutes}m` : `${diffMinutes}m`
				var cleanedStatus = q.status.replace(/[^\x00-\xFF]/g, '').replace(/[\"\(\)\[\]\|]/g, '\\\\')
				return q
			})
			
			tJson = r.map((m) =>
			({
				"name": m.displayName,
				"value": `[${m.status}](${m.url})\n**Viewers:** ${m.viewers}\n**Uptime:** ${m.uptime}\n**Game:** ${m.game}\n\n\n`
			}))
			
			embedMessages['Twitch Streams'].embedJson =
			{
				"content": "```Currently Streaming```",
				"embed": {
					"color": 1,
					"timestamp": new Date().toISOString(),
					// "title": "Twitch Streams",
					"fields": tJson
				}
			}
		}
	});
}

checkTwitch()
setInterval(checkTwitch, 5000) 

var tsAuthenticated = false
const net = require('net')
const nclient = net.createConnection({ port: 25639 }, () => {
  console.log('connected to TS client')
  nclient.write(`auth apikey=${config.DiscordBot.TeamSpeakApi.ApiKey}\r\n`)
  nclient.write('clientlist\r\n')
});

nclient.on('data', (data) => {
	let s = data.toString()
	if(!tsAuthenticated)
	{
		tsAuthenticated = true
		setInterval(() => {nclient.write('clientlist\r\n')}, 4000)
	}
	if(s.includes('BoxxiPlays'))
		parseNewTsClientList(s)
});

nclient.on('end', () => {
  console.log('disconnected from TS client')
});

function parseNewTsClientList(s)
{
	try
	{ 
		let clients = s.split('|');
		clients = clients.map((c) => c.split(/[\r\n ]/));
		clients = clients.map((c) => c[c.findIndex(d => d.includes('client_nickname='))].replace('client_nickname=', ''))
		console.log(clients)
		numTeamSpeakUsers = clients.length
	}
	catch(e)
	{
		console.log('TeamSpeak error')
		numTeamSpeakUsers = 'N/A'
	}
}

var request = require('request')

function updateForumStats()
{
	forumUrl = 'http://www.thefamilyrp.com/'
	request(forumUrl, function(error, response, html){
	    if(!error){
			let r = /<div class="ipsDataItem_main ipsPos_middle">\s*<strong>Total Members<\/strong>\s*<\/div>\s*<div class="ipsDataItem_stats ipsDataItem_statsLarge">\s*<span class="ipsDataItem_stats_number">([0-9]*)<\/span>\s*<\/div>\s*<\/li>/
			let f = r.exec(html)
			try
			{
				if(f && f[1] && f !== null)
				{
					console.log(f[1])
					numTotalForumMembers = f[1]

					r = /([0-9]*) Members, ([0-9]*) Anonymous, ([0-9]*) Guests/
					f = r.exec(html)
					numForumUsersOnline = Number(f[1]) + Number(f[2]) + Number(f[3])
					console.log(numForumUsersOnline)
				}
				else
				{
					crawlLog.info('Could not fetch forum figures')
					numForumUsersOnline = 'N/A'
				}
			}
			catch(e)
			{
				crawlLog.info('Halting error: could not fetch forum figures')
				numForumUsersOnline = 'N/A'
			}
	    }
	})
}

updateForumStats()
setInterval(updateForumStats, 30000)


function updateFiveMServerStats()
{
	server = `http://${config.ServerIp}:${config.ServerPort}/players.json`
	request(server, function(error, response, html){
	    if(!error){
	    	console.log(`------------${html}------------`)
			var r = JSON.parse(html)
			if(r)
			{
				numPlayersOnline = r.length
				playerList = r.map(p => `${p.name}`).join(', ')
			}
			else
			{
				numPlayersOnline = 'N/A'
				playerList = '-'
			}
			console.log(`${numPlayersOnline} players on server`)
	    }
	    else
	    {
				numPlayersOnline = 0
				playerList = '-'
	    }
	})
}

updateFiveMServerStats()
setInterval(updateFiveMServerStats, 60000)