/*jshint esversion: 6 */ 

function colorize(str) // replaces all ^# with text coloring
{
    var s = "<span>" + (str.replace(/\^([0-9])/g, (str, color) => `</span><span class="color-${color}">`)) + "</span>";
    return s.replace(/<span[^>]*><\/span[^>]*>/g, '');
}

$(function()
{
    var chatHideTimeout;
    var inputShown = false;
    var chatHistory = [''];
    var currentHistoryNum = 0;
    var buf = $('#chatBuffer'); // the chat window with everybody's messages
    var chatBoxEnabled = true; // enables hiding of chat window during cutscene etc.

    function startHideChat() // Hides chat after 7 seconds if nothing happens
    {
        if (chatHideTimeout) clearTimeout(chatHideTimeout);
        if (inputShown) return;

        chatHideTimeout = setTimeout(function() {
            if (inputShown) return;
            $('#chat').animate({ opacity: 0 }, 300);
            $('#PgDnIndicator').animate({ opacity: 0 }, 300);
        }, 7000);
    }

    handleResult = function(elem, wasEnter) // Either escape or enter was pressed
    {
        $('#chatInputHas').hide(); // hide chat input
        inputShown = false; // chat input is hidden
        startHideChat(); // start chat hide timer
        var obj = {}; // create chat object to pass on
        currentHistoryNum = 0; // move to front of chat history
        if (wasEnter && $(elem).val() !== '') // only if the button pressed was enter and the input box is not empty do we want to send
        {
            obj = { message: $(elem).val() }; // populate chat object to pass on
            if(chatHistory[currentHistoryNum] === '') // if most recent item is empty
                                                      // (otherwise it should already be in history so no need to save it)
                chatHistory[currentHistoryNum] = $(elem).val(); // set it to the current chat input value
        }
        $.post('http://chat/chatResult', JSON.stringify(obj)); // pass chat message on regardless
                                                                                                   // (it is what resets controls etc.)
        $('#ftb').val(''); // set input to empty
        triggerChange(); // update input box
    };

    function triggerChange() // I didn't want to edit jquery.faketextbox.js right now, so I trigger the text box refresh with this
    {                       
        var l = $('#ftb').val().length;
        $('#ftb')[0].setSelectionRange(l, l); // Also making sure caret is at end of the message
        $('#ftb').change(); // triggers all change events registered on object
    }

    $('#chatInput').fakeTextbox(); // creates the fake textbox
    $('#chatInput')[0].onPress(function(e)
    {
        if (e.which == 13) handleResult(this, true); // enter pressed
    });

    $(document).keyup(function(e)
    {
        if (e.keyCode == 27)
        {
            // chatHistory.shift(); // removes the first (will be either empty string or current message) chatHistory item
            handleResult($('#chatInput')[0].getTextBox(), false); // escape pressed; clears chatbox etc.
        }
    });

    function scroll(pixels)
    {
        buf.scrollTop(buf.scrollTop() + pixels); // scrolls the selected amount of pixels
        if(buf.scrollTop() + buf.height() == buf[0].scrollHeight) // are we at bottom of window?
        {
            $('#PgDnIndicator').animate({ opacity: 0 }, 0); // hide new message indicator
            $('#PgDnIndicator').removeClass('animated'); // stop animation
        }
        else
        {
            $('#PgDnIndicator').stop().css('opacity', '1'); // make down arrow visible
        }
    }

    function scrollToBottom() // scroll to bottom of all messages
    {
        buf.scrollTop(buf[0].scrollHeight - buf.height()); // scrolls to bottom
        $('#PgDnIndicator').removeClass('animated'); // stops down arrow bounce
        $('#PgDnIndicator').animate({ opacity: 0 }, 0); // hides page down indicator
    }

    function previousLineEntered()
    {
        elem = $('#ftb'); // the faketextbox element (basically a hidden <input type="text" />)
        if (chatHistory.length - 1 > currentHistoryNum) // if we still have history items to traverse
        {
            if (currentHistoryNum === 0) chatHistory[currentHistoryNum] = elem.val(); // if we are currently at the front of the chat history,
                                                                                      // save the current input before traversing older history
            currentHistoryNum++; // move to next item
            elem.val(chatHistory[currentHistoryNum]); // show next item
            triggerChange(); // update text box
        }
    }

    function nextLineEntered()
    {
        elem = $('#ftb'); // the faketextbox element
        if (currentHistoryNum === 0 && $(elem).val() !== '') // if at front of chat history with a non-empty line
                                                             // and we press the down arrow (next history item), 
                                                             // we instead give the user a fresh empty line
        {
            chatHistory.unshift(''); // add empty line
            elem.val(chatHistory[currentHistoryNum]); // show empty line
        }
        else if (currentHistoryNum > 0) // if we are not at front of chat history, just traverse like asked
        {
            currentHistoryNum--; // more recent history item
            elem.val(chatHistory[currentHistoryNum]); // show history item
        }
        triggerChange(); // update text box
    }

    $(document).keydown(function(e)
    {
        elem = $('#ftb'); // the faketextbox element
        if (e.keyCode == 9){e.preventDefault(); return false;}   // Tab key; disable
        else if (e.keyCode == 33) scroll(-48);                   // Page Up; up one page of chat
        else if (e.keyCode == 34) scroll(48);                    // Page Down; down one page of chat
        else if (e.keyCode == 35) scrollToBottom();              // End; bottom of chat
        else if (e.keyCode == 38) e.preventDefault();            // Arrow key up; disable default (go to beginning of input)
        else if (e.keyCode == 40) e.preventDefault();            // Arrow key down; disable default (go to end of input)
    });

    $(document).keyup(function(e)  // some things we have to do on keyup (input text box has not updated with latest key yet on keydown)
    {
        elem = $('#ftb'); // the faketextbox element
        if (e.keyCode == 38) previousLineEntered();              // Arrow key up; previous item
        else if (e.keyCode == 40) nextLineEntered();             // Arrow key down; next item
        else if (currentHistoryNum > 0 && elem.val().length > 0) // if any other key was pressed (e.g. character) and the text box
        {                                                        // is not empty now (e.g. backspace) and we are editing a historical item
            currentHistoryNum = 0; // we move to front of history
            if (chatHistory[currentHistoryNum].length === 0) // if this history item is empty, we can just use this one
                chatHistory[currentHistoryNum] = elem.val();
            else
                chatHistory.unshift(elem.val()); // otherwise we create a new one
        }
        else if (currentHistoryNum === 0) // if we already are at the most recent line ...
        {
            chatHistory[currentHistoryNum] = elem.val(); // ... we just update it
        }
    });

    window.addEventListener('message', function(event) // incoming message
    {
        var item = event.data;

        if (item.meta && item.meta == 'openChatBox') // event to open chatbox
        {
            inputShown = true; // helper bool
            chatBoxEnabled = true;   // if player presses T we re-enable chat; one waý for people to ask people how the * they get out of
                                     // hidden GUI mode if they somehow miraculously end up in it unintentionally

            $('#chat').stop().css('opacity', '1'); // show whole chat element (includes almost everything)
            $('#chatInputHas').show(); // show text input element
            $('#chatInput')[0].doFocus(); // focus text input box

            currentHistoryNum = 0; // we  move to the front of chat history
            if (chatHistory[currentHistoryNum] !== '') // only if this item is not empty do we need to create a new one
                chatHistory.unshift(''); // creates a new item
            return;
        }
        else if (item.meta && item.meta == 'focusChatBox'){ $('#chatInput')[0].doFocus(); return; } // focus input box if needed
        else if (item.meta && item.meta == 'forceCloseChatBox') // closes chat input box
        {

            currentHistoryNum = 0; // we  move to the front of chat history
            chatHistory.shift(); // remove most recent chat history item (will be either current item or an empty item)
            handleResult($('#chatInput')[0].getTextBox(), false); // clears chat box, starts hiding it etc.
            return;
        }
        else if (item.meta && item.meta == 'disableChatBox') // disables chat
        {
            currentHistoryNum = 0; // we  move to the front of chat history
            handleResult($('#chatInput')[0].getTextBox(), false); // clears chat box, starts hiding it etc.
            $('#chat').animate({ opacity: 0 }, 0);
            $('#PgDnIndicator').animate({ opacity: 0 }, 0);
            chatBoxEnabled = false;
            return;
        }
        else if (item.meta && item.meta == 'enableChatBox') // enables chat
        {
            currentHistoryNum = 0; // we  move to the front of chat history
            scrollToBottom();
            $('#chat').stop().css('opacity', '1'); // show whole chat element (includes almost everything)
            chatBoxEnabled = true;
            return;
        }
        else if (item.meta && item.meta == 'scrollDown') { scroll(48); return; } // scrolls one page down
        else if (item.meta && item.meta == 'scrollUp') { scroll(-48); return; } // scrolls one page up
        else if (item.meta && item.meta == 'scrollBottom') { scrollToBottom(); return; } // scrolls to the bottom of chat
        else if (item.meta && item.meta == 'previousLineEntered') { previousLineEntered(); return; } // goes up one line (not used currently)
        else if (item.meta && item.meta == 'nextLineEntered') { nextLineEntered(); return; } // goes down one line (not used currently)
        if(!item.html)
        {
            // some sanitizing
            var name = item.name.replace('<', '&lt;');
            var message = item.message.replace('<', '&lt;');
            var color = item.color.replace('<', '&lt;');

            // add colors
            // disables colorization in /me and system messages
            if(name != '')
            {
                name = colorize(name);
                message = colorize(message);
            }

            // add name
            var nameStr = '';
            if (name !== '')
                nameStr = '<strong style="color:' + color + ';">' + name + ': </strong>';
            else
                message = '<span style="color:' + color + ';">' + message + '</span>';
        }
        else
        {
            nameStr = '';
            message = item.html;
        }

        // if we are at the bottom, save it so we know whether to auto-scroll after
        var isAtBottom = (buf.scrollTop() + buf.height() == buf[0].scrollHeight);

        // add new message line
        buf.find('ul').append('<li>' + nameStr + message + '</li>');

        // scroll to bottom if we were at bottom before
        if (isAtBottom)
            scrollToBottom();
        else // else we want to indicate that we have a new message waiting at the bottom!
        {   
            $('#PgDnIndicator').stop().css('opacity', '1'); // show down arrow
            $('#PgDnIndicator').addClass('animated'); // add bounce animation
        }
        
        if(chatBoxEnabled)
            $('#chat').stop().css('opacity', '1'); // make sure chat is visible

        startHideChat(); // start chat hide timer
    }, false);
});