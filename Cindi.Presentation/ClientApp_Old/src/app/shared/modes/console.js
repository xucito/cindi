// CodeMirror, copyright (c) by Marijn Haverbeke and others
// Distributed under an MIT license: https://codemirror.net/LICENSE

(function(mod) {
  if (typeof exports == "object" && typeof module == "object")
    // CommonJS
    mod(require("codemirror"));
  else if (typeof define == "function" && define.amd)
    // AMD
    define(["codemirror"], mod);
  // Plain browser env
  else mod(CodeMirror);
})(function(CodeMirror) {
  "use strict";

  CodeMirror.defineMode("CindiConsole", function() {
    var words = {};
    function define(style, dict) {
      for (var i = 0; i < dict.length; i++) {
        words[dict[i]] = style;
      }
    }
    var commonKeywords = ["GET", "POST", "PUT"];

    CodeMirror.registerHelper("hintWords", "shell", commonKeywords);

    define("keyword", commonKeywords);

    function tokenBase(stream, state) {
      if (stream.eatSpace()) return null;

      var sol = stream.sol();
      var ch = stream.next();
      var current = stream.current();

      if (ch == ":") {
        state.tokens.unshift(colon);
      }
    }

    function tokenString(quote, style) {
      var close = quote == "(" ? ")" : quote == "{" ? "}" : quote;
      return function(stream, state) {
        var next,
          escaped = false;
        var sol;
        while ((next = stream.next()) != null) {
          if (next === close && !escaped) {
            state.tokens.shift();
            break;
          }

          stream.backUp(1);
          state.tokens.unshift(tokenColor());
          return null;
        }
      };
    }

    function tokenColor() {
      var next;
      return function(stream, state) {
        stream.eatWhile(/[\w-]/);
		state.tokens.shift();
		
        var ch = stream.next();
        return "string";
      };
    }

    function tokenStringStart(quote, style) {
      return function(stream, state) {
        state.tokens[0] = tokenString(quote, style);
        stream.next();
        return tokenize(stream, state);
      };
    }

    function tokenize(stream, state) {
      return (state.tokens[0] || tokenBase)(stream, state);
    }

    //Custom functions
    var colon = function(stream, state) {
      if (state.tokens.length > 1) stream.eat(":");
      var ch = stream.next();
      if (stream.string.includes(':')) {
        //stream.backUp(1);
        state.tokens[0] = tokenString(ch, "log");
        return tokenize(stream, state);
      }
    };

    return {
      startState: function() {
        return { tokens: [] };
      },
      token: function(stream, state) {
        return tokenize(stream, state);
      },
      closeBrackets: "()[]{}''\"\"``",
      lineComment: "#",
      fold: "brace"
    };
  });

  CodeMirror.defineMIME("text/x-sh", "CindiConsole");
});
