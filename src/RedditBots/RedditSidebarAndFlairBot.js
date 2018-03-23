//var socket = require('socket.io-client')('https://soe.pepzwee.com/', {query: 'email='});
var _ = require('underscore');
var fs = require('fs');
var path = require('path'); 
var snoowrap = require('snoowrap');
var colors = require('colors');

var yaml = require('js-yaml')
var config = yaml.safeLoad(fs.readFileSync('config.yaml', 'utf8'))

var reddit = new snoowrap({
  userAgent: 'Node bot for GTA RP Subreddit',
  clientId: config.RedditBots.RedditMainAccount.clientId,
  clientSecret: config.RedditBots.RedditMainAccount.clientSecret,
  refreshToken: config.RedditBots.RedditMainAccount.refreshToken
});

function pad(n, width, z) {
  z = z || '0';
  n = n + '';
  return n.length >= width ? n : new Array(width - n.length + 1).join(z) + n;
}

var templateReplacements = {
//	'{{STATUS_public}}' : null,
//	'{{STATUS_whitelist1}}' : null,
//	'{{STATUS_whitelist2}}' : null,
//	'{{STATUS_TIMESTAMP}}' : null,
	'{{TWITCH_TABLE}}' : null,
	'{{TWITCH_TIMESTAMP}}' : null,
	'{{CONTRIBUTOR_TABLE}}' : null
};

var rankingFlairs =
['gemstone-diamond',
'gemstone-pink',
'gemstone-orange',
'gemstone-orange',
'gemstone-orange',
'gemstone-green',
'gemstone-green',
'gemstone-green',
'gemstone-green',
'gemstone-green',
'gemstone-green',
'gemstone-green',
'gemstone-green',
'gemstone-green'];


function getFileContent(path, callback) { 
	return new Promise(function(resolve, reject)
	{
	    fs.readFile(path, 'utf8', function (err, data)
    	{
	        if (err) throw err;
	        callback(data);
	        resolve('Success');
    	});
	});
}

function writeFileContent(path, contents) { 
	return new Promise(function(resolve, reject)
	{
	    fs.writeFile(path, contents, function (err, data)
		{
	        if (err) throw err;
	        resolve('Success');
	    });
    });
}

// socket.on('servers', function (data) {
// 	// var servers = ['public', 'whitelist1'];
// var servers = ['public', 'whitelist1', 'whitelist2'];

// 	for (var server in servers)
// 		if (data && data.data) {
// 			if(data.data[servers[server]] == null) data.data[servers[server]] = []
// 			console.log(`${servers[server]}: ${data.data[servers[server]].length}`.gray);
// 			templateReplacements[`{{STATUS_${servers[server]}}}`] = `${data.data[servers[server]].length}/24`;
// 		}
// 		else
// 			console.log('Invalid response'.red);
// 	var curDate = new Date();
// 	templateReplacements[`{{STATUS_TIMESTAMP}}`] = `UTC ^${pad(curDate.getUTCHours(),2)}:${pad(curDate.getUTCMinutes(),2)}`;
// });

var request = require('request');
 
var accessToken = config.RedditBots.TwitchAccount.accessToken;
var clientId = config.RedditBots.TwitchAccount.clientId;
var blacklist = config.RedditBots.TwitchAccount.streamBlacklist;
setInterval(function(){
request.get({url: 'https://api.twitch.tv/kraken/streams?game=Grand Theft Auto V&language=en',  headers: {'Accept': 'application/vnd.twitchtv.v5+json', 'Authorization': `OAuth ${accessToken}`, 'Client-ID': clientId}},
	function(err, res, body) {
 	var JSONsuccess = false;
 	var r;
	//console.dir(JSON.parse(body).streams.slice(0, 11), {depth: null, colors: true})
 	try{
		r = JSON.parse(body).streams.slice(0, 11);
		JSONsuccess = true;
	}
	catch(err)
	{
		console.log('JSON issue! Trying again next time.'.red);
	}
	if(JSONsuccess)
	{
	 	templateReplacements[`{{TWITCH_TABLE}}`] = '';
		r = _.map(r, function(r)
			{
				var q = {
						displayName: r.channel.display_name,
						viewers: r.viewers,
						status: r.channel.status,
						url: r.channel.url
					};

				var dateDiff = new Date().getTime()-new Date(r.created_at).getTime();
				var diffHours = Math.floor(dateDiff/(1000*60*60));
				dateDiff -= diffHours*1000*60*60;
				var diffMinutes = Math.floor(dateDiff/(1000*60));
				q.uptime = diffHours > 0 ? `${diffHours}h ${diffMinutes}m` : `${diffMinutes}m`;
				if(!blacklist.includes(q.displayName))
				{
					var cleanedStatus = q.status.replace(/[^\x00-\xFF]/g, '').replace(/[\"\(\)\[\]\|]/g, '\\\\');
					templateReplacements[`{{TWITCH_TABLE}}`] += `[${q.displayName}](${q.url} "${cleanedStatus}")|${q.viewers}|${q.uptime}\n`;
				}
				return q;
			});

		var curDate = new Date();
		templateReplacements[`{{TWITCH_TIMESTAMP}}`] = `UTC ^${pad(curDate.getUTCHours(), 2)}:${pad(curDate.getUTCMinutes(), 2)}`;
	}
});  
}, 30000);

setInterval(function() {
reddit.getSubreddit('GTAVRPclips').getHot().then(
	function(d)
	{
		var flairs = _.map(d, function(s){return {id: s.id, flair: s.link_flair_text, streamerName: s.media ? s.media.oembed.author_name : null};});
		flairs.forEach(
			function(f)
			{
				if(f.flair === null && f.streamerName !== null)
				{
					f.cssName = f.streamerName.replace('_', '-');
					console.log(`adding flair ${f.streamerName} to submission ID ${f.id}`.green);
					reddit.getSubmission(f.id).assignFlair({text: f.streamerName, css_class: `streamer-${f.cssName}`});
				}
			}
		);
});
}, 60000);

function sidebarTemplateCallback(t)
{
	var sidebarTemplate = t.content_md;
	for(var replaceString in templateReplacements) {
		if(templateReplacements[replaceString] === null) return;
		sidebarTemplate = sidebarTemplate.replace(replaceString, templateReplacements[replaceString]);
	}
	reddit.getSubreddit('GTAVRPclips').getWikiPage('config/sidebar').edit({text: sidebarTemplate});
	// console.log(sidebarTemplate)
	console.log('Updated sidebar'.green);
}

setInterval(function(){
	reddit.getSubreddit('GTAVRPclips').getWikiPage('sidebar_template').fetch().then(sidebarTemplateCallback);
}, 50000);

var threadKarma = {};

getFileContent('./threadKarma.txt', function(data){
	var fileContents = data.split('\n');
	if(fileContents.length <= 1) return;
	for(var line in fileContents)
	{
		var lineElements = fileContents[line].split(',');
		threadKarma[lineElements[0]] = {username: lineElements[1], karma: lineElements[2]};
	}
})
.then(function()
{
	console.log('Karma file read'.green);
});

function saveThreadKarmaVariable(){
	var contents = '';
	for(var thread in threadKarma)
	{
		contents += `${thread},${threadKarma[thread].username},${threadKarma[thread].karma}\n`;
	}
	writeFileContent('./threadKarma.txt', contents);
}

function getAllSubmissionKarma()
{
	var userKarma = {};
	reddit.getSubreddit('GTAVRPclips').getTop({time: 'all'}).fetchAll().then(
	function (submissions)
	{
		console.log(`Going through all submissions; ${submissions.length} in total`.blue);
		for (var submission in submissions)
		{
			if(submissions[submission] && submissions[submission].id)
			{
				var r = submissions[submission];
				threadKarma[r.id] = {username: r.author.name || '', karma: r.score};
			}
		}
		saveThreadKarmaVariable();
		for(var thread in threadKarma)
		{
			var n = threadKarma[thread];
			userKarma[n.username] = !(n.username in userKarma) ? n.karma : userKarma[n.username] + n.karma;
		}
		var t = [];
		_(userKarma).each(function(v, k)
		{
			var f = {};
			f.username = k;
			f.karma = v;
			t.push(f);
		});
		userKarma = t;
		userKarma = _(userKarma).sortBy(function(user){return -user.karma;});
		userKarma = _(userKarma).filter(function(v){return /*v.username !== 'Boxxi' &&*/ v.username !== undefined && v.username !== 'undefined';});
		userKarma = userKarma.slice(0, 10);
		console.log(userKarma);
		var ranking = 1;
		var tempFlairs = [];
		templateReplacements[`{{CONTRIBUTOR_TABLE}}`] = '';
		for(var user in userKarma)
		{
			templateReplacements[`{{CONTRIBUTOR_TABLE}}`] += `${ranking}|[](#${rankingFlairs[ranking-1]}) [${userKarma[ranking-1].username}](http://www.reddit.com/user/${userKarma[ranking-1].username})|${userKarma[ranking-1].karma}\n`;
			tempFlairs[ranking-1] = {name: userKarma[ranking-1].username, text: `This user is the top #${ranking} contributor in this subreddit`, cssClass: rankingFlairs[ranking-1]};
			ranking++;
		}
		console.log(tempFlairs);
		console.log(`${templateReplacements['\{\{CONTRIBUTOR_TABLE\}\}']}`.grey);
		console.log('Setting flairs'.grey);
		reddit.getSubreddit('GTAVRPclips').setMultipleUserFlairs(tempFlairs).then(console.log('Flairs set'.green));
		var curDate = new Date();
		templateReplacements[`{{CONTRIBUTOR_TIMESTAMP}}`] = `UTC ^${pad(curDate.getUTCHours(),2)}:${pad(curDate.getUTCMinutes(),2)}`;
	});
}

getAllSubmissionKarma();
setInterval(getAllSubmissionKarma, 60*60*1000);