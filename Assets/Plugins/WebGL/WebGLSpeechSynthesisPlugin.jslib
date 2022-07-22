mergeInto(LibraryManager.library, {
  WebGLSpeechSynthesisPluginIsAvailable: function() {
    if (typeof speechSynthesis === "undefined") {
	  return false;
	} else {
	  return true;
	}
  },
  WebGLSpeechSynthesisPluginGetVoices: function() {
    var returnStr = "";
	if (typeof speechSynthesis === "undefined") {
	  // not supported
	} else {
      var voices = document.mWebGLSpeechSynthesisPluginVoices;
      if (voices != undefined) {
	    var jsonData = {};
	    jsonData.voices = [];
	    for (var voiceIndex = 0; voiceIndex < voices.length; ++voiceIndex) {
	      var voice = voices[voiceIndex];
	      var speechSynthesisVoice = {};
	      speechSynthesisVoice._default = voice.default; //default is reserved word
	      speechSynthesisVoice.lang = voice.lang;
	      speechSynthesisVoice.localService = voice.localService;
	      speechSynthesisVoice.name = voice.name;
	      speechSynthesisVoice.voiceURI = voice.voiceURI;
	      jsonData.voices.push(speechSynthesisVoice);
	    }
	    //console.log(JSON.stringify(jsonData, undefined, 2));
	    returnStr = JSON.stringify(jsonData);
	  }
	}
	var bufferLength = lengthBytesUTF8(returnStr) + 1;
	var buffer = _malloc(bufferLength);
	if (stringToUTF8 == undefined) {
		writeStringToMemory(returnStr, buffer);
	} else {
		stringToUTF8(returnStr, buffer, bufferLength);
	}
    return buffer;
  },
  WebGLSpeechSynthesisPluginInit: function() {
    console.log("WebGLSpeechSynthesisPlugin: Init");
    if (typeof speechSynthesis === "undefined") {
      return;
    }	
    if (document.mWebGLSpeechSynthesisPluginVoices != undefined) {
      return; //already initialized
    }
	var initVoices = function() {
		var voices = speechSynthesis.getVoices();
		if (voices.length == 0) {
		  setTimeout(function() { initVoices() }, 10);
		  return;
		}
		document.mWebGLSpeechSynthesisPluginVoices = voices;
		//console.log(document.mWebGLSpeechSynthesisPluginVoices);
	}
	initVoices();
  },
  WebGLSpeechSynthesisPluginCreateSpeechSynthesisUtterance: function() {
    if (typeof speechSynthesis === "undefined") {
      return -1;
    }
    if (document.mWebGLSpeechSynthesisPluginUtterances == undefined) {
	  document.mWebGLSpeechSynthesisPluginUtterances = [];
	}
	var index = document.mWebGLSpeechSynthesisPluginUtterances.length;
	var instance = new SpeechSynthesisUtterance();
	document.mWebGLSpeechSynthesisPluginUtterances.push(instance);
	return index;
  },
  WebGLSpeechSynthesisPluginSetUtterancePitch: function(index, pitch) {
    if (typeof speechSynthesis === "undefined") {
	  return;
	}
    if (document.mWebGLSpeechSynthesisPluginUtterances == undefined) {
	  return;
	}
    if (document.mWebGLSpeechSynthesisPluginUtterances.length <= index) {
	  return;
	}
	var instance = document.mWebGLSpeechSynthesisPluginUtterances[index];
	if (instance == undefined) {
	  return;
	}
	var strPitch = Pointer_stringify(pitch);
	instance.pitch = parseFloat(strPitch);
  },
  WebGLSpeechSynthesisPluginSetUtteranceRate: function(index, rate) {
    if (typeof speechSynthesis === "undefined") {
	  return;
	}
    if (document.mWebGLSpeechSynthesisPluginUtterances == undefined) {
	  return;
	}
    if (document.mWebGLSpeechSynthesisPluginUtterances.length <= index) {
	  return;
	}
	var instance = document.mWebGLSpeechSynthesisPluginUtterances[index];
	if (instance == undefined) {
	  return;
	}
	var strRate = Pointer_stringify(rate);
	instance.rate = parseFloat(strRate);
  },
  WebGLSpeechSynthesisPluginSetUtteranceText: function(index, text) {
    if (document.mWebGLSpeechSynthesisPluginUtterances == undefined) {
	  return;
	}
    if (document.mWebGLSpeechSynthesisPluginUtterances.length <= index) {
	  return;
	}
	var instance = document.mWebGLSpeechSynthesisPluginUtterances[index];
	if (instance == undefined) {
	  return;
	}
	instance.text = Pointer_stringify(text);
  },
  WebGLSpeechSynthesisPluginSetUtteranceVoice: function(index, voiceURI) {
    if (document.mWebGLSpeechSynthesisPluginUtterances == undefined) {
	  return;
	}
    if (document.mWebGLSpeechSynthesisPluginUtterances.length <= index) {
	  return;
	}
	var instance = document.mWebGLSpeechSynthesisPluginUtterances[index];
	if (instance == undefined) {
	  return;
	}
	var voices = document.mWebGLSpeechSynthesisPluginVoices;
	if (voices == undefined) {
	  return;
	}
	var strVoice = Pointer_stringify(voiceURI);
	//console.log("SetUtteranceVoice: "+utterLang);
	for (var voiceIndex = 0; voiceIndex < voices.length; ++voiceIndex) {
	  var voice = voices[voiceIndex];
	  if (voice == undefined) {
	    continue;
	  }
	  if (voice.voiceURI == strVoice) {
	    instance.voice = voice;
	    return;
	  }
	}
  },
  WebGLSpeechSynthesisPluginSetUtteranceVolume: function(index, volume) {
    if (document.mWebGLSpeechSynthesisPluginUtterances == undefined) {
	  return;
	}
    if (document.mWebGLSpeechSynthesisPluginUtterances.length <= index) {
	  return;
	}
	var instance = document.mWebGLSpeechSynthesisPluginUtterances[index];
	if (instance == undefined) {
	  return;
	}
	instance.volume = volume;
  },
  WebGLSpeechSynthesisPluginSpeak: function(index) {
    if (typeof speechSynthesis === "undefined") {
	  return;
	}
    if (document.mWebGLSpeechSynthesisPluginUtterances == undefined) {
	  return;
	}
    if (document.mWebGLSpeechSynthesisPluginUtterances.length <= index) {
	  return;
	}
	var instance = document.mWebGLSpeechSynthesisPluginUtterances[index];
	if (instance == undefined) {
	  return;
	}
	instance.onend = function (event) {
		var jsonData = {};
		jsonData.index = index;
		jsonData.elapsedTime = event.elapsedTime;
		jsonData.type = event.type;
		//console.log(JSON.stringify(jsonData, undefined, 2));
		if (document.mWebGLSpeechSynthesisPluginOnEnd == undefined) {
			document.mWebGLSpeechSynthesisPluginOnEnd = [];
		}
		document.mWebGLSpeechSynthesisPluginOnEnd.push(JSON.stringify(jsonData));
	}
	speechSynthesis.speak(instance);
  },
  WebGLSpeechSynthesisPluginHasOnEnd: function() {
	if (document.mWebGLSpeechSynthesisPluginOnEnd == undefined) {
		document.mWebGLSpeechSynthesisPluginOnEnd = [];
	}
	return (document.mWebGLSpeechSynthesisPluginOnEnd.length > 0);
  },
  WebGLSpeechSynthesisPluginGetOnEnd: function() {
    var returnStr = "";
	if (document.mWebGLSpeechSynthesisPluginOnEnd == undefined) {
		document.mWebGLSpeechSynthesisPluginOnEnd = [];
	}
	if (document.mWebGLSpeechSynthesisPluginOnEnd.length == 0) {
		returnStr = "No results available";
    } else {
		returnStr = document.mWebGLSpeechSynthesisPluginOnEnd[0];
	}
    document.mWebGLSpeechSynthesisPluginOnEnd = document.mWebGLSpeechSynthesisPluginOnEnd.splice(1);
	var bufferLength = lengthBytesUTF8(returnStr) + 1;
	var buffer = _malloc(bufferLength);
    if (stringToUTF8 == undefined) {
		writeStringToMemory(returnStr, buffer);
	} else {
		stringToUTF8(returnStr, buffer, bufferLength);
	}
    return buffer;
  },
  WebGLSpeechSynthesisPluginCancel: function() {
    if (typeof speechSynthesis === "undefined") {
	  return;
	}
    speechSynthesis.cancel();
  }
});
