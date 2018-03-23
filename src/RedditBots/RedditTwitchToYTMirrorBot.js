var cheerio = require('cheerio');
var request = require('request');
var fs = require('fs');
var colors = require('colors');

var progress = require('request-progress');
const jsdom = require("jsdom");
const { JSDOM } = jsdom;

var Youtube = require('youtube-video-api');

var _ = require('underscore');
var path = require('path'); 
snoowrap = require('snoowrap');

var yaml = require('js-yaml')
var config = yaml.safeLoad(fs.readFileSync('config.yaml', 'utf8'))

var reddit = new snoowrap({
  userAgent: 'Node bot for GTA RP Subreddit',
  clientId: config.RedditBots.RedditMainAccount.clientId,
  clientSecret: config.RedditBots.RedditMainAccount.clientSecret,
  refreshToken: config.RedditBots.RedditMainAccount.refreshToken
});

var redditMirrorBot = new snoowrap({
  userAgent: 'Node bot for GTA RP Subreddit',
  clientId: config.RedditBots.RedditBotAccount.clientId,
  clientSecret: config.RedditBots.RedditBotAccount.clientSecret,
  refreshToken: config.RedditBots.RedditBotAccount.refreshToken
});

var botName = config.RedditBots.RedditBotAccount.botName;
clipsToDownload = [];
alreadyDownloaded = [];
clipsToUpload = [];
alreadyUploaded = [];
commentsToPost = [];
alreadyPosted = [];
processedThreads = [];

youtubeClips = [];

function getFileContent(path, callback) { 
	return new Promise( function(resolve, reject)
	{
	    fs.readFile(path, 'utf8', function (err, data)
    	{
	        if (err) throw err;
	        callback(data);
	        resolve('Success');
    	});
	});
}

getFileContent('./YoutubeClips.txt', function(data){
	var fileContents = data.split('\n');
	if(fileContents.length <= 1) return;
	for(line in fileContents)
	{
		lineElements = fileContents[line].split(',');
		youtubeClips[lineElements[0]] = lineElements[1];
	}
}).then(
function(){
	console.log('Clips file read'.green);
});

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

function saveClipsVariable(){
	contents = '';
	for(slug in youtubeClips)
	{
		contents += `${slug},${youtubeClips[slug]}\n`;
	}
	writeFileContent('./YoutubeClips.txt', contents);
}

currentlyDownloading = false;
currentlyUploading = false;

ytube = Youtube({ 
  video: {
    part: 'status,snippet'
  },
  email: config.RedditBots.YoutubeAccount.email,
  password: config.RedditBots.YoutubeAccount.password
});

var YouTubeisAuthenticated = false;
ytube.authenticate(config.RedditBots.YoutubeAccount.url, config.RedditBots.YoutubeAccount.apiToken, function (err, tokens) {
  if (err) return console.error('Cannot auth:', err);
 
  console.log('Auth tokens:'.green, tokens);
  YouTubeisAuthenticated = true;
});

function handleNewSubmissions(){
redditMirrorBot.getSubreddit('GTAVRPclips').getHot().then(
function (submissions) {
	console.log('Checking hot submissions'.grey);
	for (submission in submissions)
	{
		if(submissions[submission] && submissions[submission].id) {
			redditMirrorBot.getSubmission(submissions[submission].id).expandReplies({limit: 1, depth: 1}).then(
				function(r)
				{
					commenters = _.map(r.comments, function(c)
					{
						return (c && c.author && c.author.name) ? c.author.name : '';
					});
					if(!processedThreads.includes(r.id))
					{
						processedThreads.push(r.id);
						if(commenters.includes(botName))
						{
							console.log(`Already commented on "${r.title}"`.grey);
						}
						else
						{
							console.log(`Will comment on "${r.title}" if it's a Twitch clip`.green);
							if(r.url) slug = _.last(r.url.split('/')).split('?')[0];
							if(r.title &&  r.author &&  r.author.name &&  r.domain == 'clips.twitch.tv' &&  r.url && !alreadyUploaded.includes(slug) && !alreadyDownloaded.includes(slug))
							{
								if(!(slug in youtubeClips))
								{
									clipsToDownload.push({threadId: r.id, url: r.url, slug: slug, title:  r.title, poster:  r.author.name});
									console.log(`Added clip ${slug} to download queue`.green);
									handleDownloadQueue();
								}
								else
								{
									commentsToPost.push({threadId: r.id, url: r.url, slug: slug, title:  r.title, poster:  r.author.name, videoId: youtubeClips[slug]});
									console.log(`Sending already uploaded clip ${slug} straight to comment queue`.green);
									handleCommentQueue();
								}
							}
							else{
								console.log(`"${r.title}" is not a valid Twitch clip or something`.red);
							}	
						}
					}
				}
			);
		}
	}
});
}
function deleteClipIfExists(clipItem)
{
	return new Promise(function(resolve, reject)
	{
		fs.exists(`./clips/${clipItem.slug}.mp4.tmp`, function(exists)
		{
			if(exists)
				fs.unlink(`./clips/${clipItem.slug}.mp4.tmp`, function(err)
				{
					console.log(`Old temp file ./clips/${clipItem.slug}.mp4.tmp deleted`.green);
					resolve(clipItem);
				});
			else
				resolve(clipItem);
		});
	});
}

function getTwitchClipFileUrl(clipItem) {
	return new Promise(function(resolve, reject)
	{
		request.get({url: `https://clips.twitch.tv/${clipItem.slug}`}, function(err, res, body)
		{
			try
			{
				const $ = cheerio.load(body);
				const window = (new JSDOM(``, { runScripts: "outside-only" })).window;
				window.eval('Twitch = {Clips:{viewClip: function(){}}}');
				window.eval($('script').last().html());
				clipItem.clipUrl = window.clipInfo.quality_options[0].source;
				clipItem.streamer = window.clipInfo.broadcaster_display_name;
			}
			catch(err)
			{
				console.log(`There was an error trying to fetch and parse clip info for ${clipItem.title}`.red);
			}
			resolve(clipItem);
		});
	});
};

function downloadTwitchClip(clipItem)
{
	return new Promise(function(resolve, reject)
	{
		fs.exists(`./clips/${clipItem.slug}.mp4`,
		function(exists)
		{ 
			if (exists)
			{ 
				console.log(`./clips/${clipItem.slug}.mp4 already downloaded!`.grey);
				clipsToUpload.push(clipItem);
				resolve(clipItem);
			}
			else
			{
				 progress(request(clipItem.clipUrl))
			    // .on('progress', function(state){console.log(`Progress for file ${slug}.mp4 is ${state.percent}`)})
			    .on('response', function(response){console.log(`Response code for file ${clipItem.slug}.mp4 (${clipItem.url} with title "${clipItem.title}") is ${response.statusCode}`.green)})
			    .on('error', function(error){console.log(`Error for file ${clipItem.slug}.mp4 is ${error}`.red)})
			    .on('end', function(error)
		    	{
		    		console.log(`Download complete for file ${clipItem.slug}.mp4`.green); 
					clipsToUpload.push(clipItem);
					resolve(clipItem);
				})
			    .pipe(fs.createWriteStream(`./clips/${clipItem.slug}.mp4.temp`));
			}
		});
	});
};

function renameTwitchClip(clipItem)
{
	return new Promise(function(resolve, reject)
	{
		fs.rename(`./clips/${clipItem.slug}.mp4.temp`, `./clips/${clipItem.slug}.mp4`, function(err)
		{
			if(!err) console.log(`Twitch clip ${clipItem.slug}.mp4 renamed`);
			currentlyDownloading = false;
			resolve(clipItem);
		});
	});
}

function handleDownloadQueue()
{
	if (clipsToDownload.length > 0)
	{
		if (currentlyDownloading) return;
		clipItem = clipsToDownload.pop();
		if(!alreadyDownloaded.includes(clipItem.slug) && !alreadyUploaded.includes(clipItem.slug) && !(clipItem.slug in youtubeClips))
		{
			currentlyDownloading = true;
			alreadyDownloaded.push(clipItem.slug);
			deleteClipIfExists(clipItem)
			.then(getTwitchClipFileUrl)
			.then(downloadTwitchClip)
			.then(renameTwitchClip)
			.then(function(clipItem)
			{
				console.log('Checking upload and download queue'.grey);
				currentlyDownloading = false;
				handleDownloadQueue();
				handleUploadQueue();
			});
		}
	} else {
		console.log('No more items to download.'.grey);
	}
}

function uploadTwitchClip(clipItem)
{
	return new Promise(function(resolve, reject)
	{
		if(YouTubeisAuthenticated)
		{
			var params = {
			  resource: {
			    snippet: {
			      title: `${clipItem.title.slice(0,100-clipItem.streamer.length-3)} (${clipItem.streamer})`,
			      description: `Credits: http://www.twitch.tv/${clipItem.streamer} and ${clipItem.poster}`,
			      tags: ['TFRP', 'State of Emergency', 'LIRIK', 'GTA', 'GTA RP', 'GTA Roleplay']
			    },
			    status: {
			      privacyStatus: 'public'
			    }
			  }
			};
					console.log(`Uploading video: "${clipItem.title}"`.green);
			ytube.upload(`./clips/${clipItem.slug}.mp4`, params, function (err, video) {
				if (!err){
					console.log(`Video was uploaded: "${clipItem.title}" (${video.id})`.green);
					clipItem.videoId = video.id;
					youtubeClips[clipItem.slug] = clipItem.videoId;
					saveClipsVariable();
					fs.unlink(`./clips/${clipItem.slug}.mp4`, function(){});
					// if(!commentsToPost.includes(clipItem)) 
					commentsToPost.push(clipItem);
					resolve('Success');
				}
				else
				{
					console.log('Video upload error:'.red, err);
					reject('Error');
				}
			});
		}
	});
}

function handleUploadQueue()
{
	console.log(`clipsToUpload = ${clipsToUpload}`.grey);
	if (clipsToUpload.length > 0 && !currentlyUploading)
	{
		clipItem = clipsToUpload.pop();
		if(clipItem.slug in youtubeClips)
			{
				console.log(`${clipItem.slug} already uploaded; skipping`.grey);
				fs.unlink(`./clips/${clipItem.slug}.mp4`, function(res){});
			}
		saveClipsVariable();
		if(!alreadyUploaded.includes(clipItem.slug) && !(clipItem.slug in youtubeClips))
		{
			currentlyUploading = true;
			alreadyUploaded.push(clipItem.slug);
			uploadTwitchClip(clipItem).then(
			function(res)
			{
				currentlyUploading = false;
				handleUploadQueue();
				handleCommentQueue();
			});
		}
	}
}

function postComment(clipItem)
{
	return new Promise(function(resolve, reject)
	{
		redditMirrorBot
		.getSubmission(clipItem.threadId)
		.reply(`A YouTube mirror for this clip is available [here](http://www.youtube.com/watch?v=${clipItem.videoId}).`)
		// new Promise(function(resolve, reject){resolve();})
		.then(function(err)
		{
			console.log(`Posted comment for "${clipItem.title}"`.green);
			resolve('Success');
		});
	});
}

function handleCommentQueue()
{
	console.log(`commentsToPost: ${commentsToPost}`);
	while (commentsToPost.length > 0)
	{
		clipItem = commentsToPost.pop();
		if(!alreadyPosted.includes(clipItem))
		{
			console.log(`attempting to post comment: ${clipItem.title}`.grey);
			alreadyPosted.push(clipItem);
			postComment(clipItem)
			.then(function()
			{
				handleCommentQueue();
			});
		}
	}
}

handleNewSubmissions();
setInterval(handleNewSubmissions, 30000);