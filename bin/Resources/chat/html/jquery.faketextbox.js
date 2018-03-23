(function ($) {
    $.fn.fakeTextbox = function () {

        return this.each(function () {

            var $me = $(this),
                cursorTimer,
                $tb = $('<input type="text" id="ftb" class="fake" />');

            if ($me.data('ftbftw')) {
                console.log('already initialized');
                return;
            }

            $me.data('ftbftw', 1);

            $tb.insertAfter($me);

            function appendCaret(toHere, position, selStart, selEnd) {
                if (position === selStart) {
                    toHere += "</div><div class='caret'>";
                }
                if (position === selEnd) {
                    toHere += "</div><div>";
                }
                return toHere;
            }

            function syncTextbox() {
                var tbVal = $tb.val().replace('<', '&lt;');
                var tbLen = tbVal.length;
                var selStart = $tb.get(0).selectionStart;
                var selEnd = $tb.get(0).selectionEnd;
                var newOut = '<div>';

                for (var i = 0; i < tbLen; i++) {
                    newOut = appendCaret(newOut, i, selStart, selEnd);
                    newOut += tbVal[i];
                }

                $me.html(colorize(appendCaret(newOut, i, selStart, selEnd) + '</div>'));
                if (selStart != selEnd) {
                    $('#chatInput .caret').addClass('selection');
                    $('#chatInput .caret').css('display', 'inline');
                    $('#chatInput .caret').css('box-shadow', 'none');
                }
                else
                {
                    $('#chatInput .caret').css('display', 'inline-block');
                    $('#chatInput .caret').css('top', '4px');
                    $('#chatInput .caret').css('position', 'relative');
                    $('#chatInput .caret').css('box-shadow', '0px 0px 1px 1px #000');
                    $('#chatInput .caret').height('18px');
                }
            }

            $me.click(function () {
                $tb.focus();
            });
            setInterval(syncTextbox, 10); 

            $tb.bind("change keypress keyup", function()
            {
                setTimeout(syncTextbox, 1); //
            })
                .blur(function () {
                clearInterval(cursorTimer);
                cursorTimer = null;
                var $cursor = $('.caret', $me);
                $cursor.css({
                    visibility: 'visible'
                });
                $me.removeClass('focused');
            }).focus(function () {
                if (!cursorTimer) {
                    $me.addClass('focused');
                    cursorTimer = window.setInterval(function () {
                        var $cursor = $('.caret', $me);
                        if ($cursor.hasClass('selection') || $cursor.css('visibility') === 'hidden') {
                            $cursor.css({
                                visibility: 'visible'
                            });
                        } else {
                            $cursor.css({
                                visibility: 'hidden'
                            });
                        }
                    }, 500);
                }
            });

            this.doFocus = function()
            {
                $tb.focus();
            };

            this.onPress = function(f)
            {
                $tb.bind('keypress', f);
            };

            this.getTextBox = function()
            {
                return $tb;
            };

            syncTextbox();

            if ($me.hasClass('initFocus')) {
                $tb.focus();
            }
        });
    };
}(jQuery));
