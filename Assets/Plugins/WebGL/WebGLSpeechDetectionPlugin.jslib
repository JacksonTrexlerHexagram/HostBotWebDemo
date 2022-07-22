mergeInto(LibraryManager.library, {

  WebGLSpeechDetectionPluginIsAvailable: function() {
    return !!(window.SpeechRecognition || window.webkitSpeechRecognition);
  },
  
  WebGLSpeechDetectionPluginInit: function() {
    console.log("WebGLSpeechDetectionPlugin: Init");
    window.SpeechRecognition = window.SpeechRecognition || window.webkitSpeechRecognition;
    if (SpeechRecognition == undefined) {
      return;
    }
    if (document.mWebGLSpeechDetectionPluginRecognition != undefined) {
      return;
    }
    document.mWebGLSpeechDetectionPluginResults = [];
    document.mWebGLSpeechDetectionPluginRecognition = new SpeechRecognition();
    document.mWebGLSpeechDetectionPluginRecognition.interimResults = true;
    document.mWebGLSpeechDetectionPluginDetect = function(e) {
      const results = Array.from(e.results);
      if (results == undefined) {
        return;
      }
      var jsonData = {};
      jsonData.results = [];
      for (var resultIndex = 0; resultIndex < results.length; ++resultIndex) {
        //console.log(results[resultIndex]);
        // SpeechRecognitionResult
        var speechRecognitionResult = {};
        speechRecognitionResult.isFinal = results[resultIndex].isFinal;
        speechRecognitionResult.alternatives = []; 
        for (var setIndex = 0; setIndex < results[resultIndex].length; ++setIndex) {
          //console.log(results[resultIndex][setIndex]);
          // SpeechRecognitionAlternative 
          var speechRecognitionAlternative = {};
          speechRecognitionAlternative.confidence = results[resultIndex][setIndex].confidence;
          speechRecognitionAlternative.transcript = results[resultIndex][setIndex].transcript;
          speechRecognitionResult.alternatives.push(speechRecognitionAlternative);
        }
        speechRecognitionResult.length = speechRecognitionResult.alternatives.length;
        jsonData.results.push(speechRecognitionResult);
      }
      //console.log(JSON.stringify(jsonData, undefined, 2));
      document.mWebGLSpeechDetectionPluginResults.push(JSON.stringify(jsonData));
    };
    document.mWebGLSpeechDetectionPluginRecognition.addEventListener('result', document.mWebGLSpeechDetectionPluginDetect);
    document.mWebGLSpeechDetectionPluginRecognition.addEventListener('end', document.mWebGLSpeechDetectionPluginRecognition.start);
    document.mWebGLSpeechDetectionPluginRecognition.stop();
    document.mWebGLSpeechDetectionPluginRecognition.start();
  },

  WebGLSpeechDetectionPluginStart: function() {
    if (document.mWebGLSpeechDetectionPluginRecognition == undefined) {
      return;
    }
    document.mWebGLSpeechDetectionPluginRecognition.start();
  },

  WebGLSpeechDetectionPluginAbort: function() {
    if (document.mWebGLSpeechDetectionPluginRecognition == undefined) {
      return;
    }
    document.mWebGLSpeechDetectionPluginRecognition.abort();
  },

  WebGLSpeechDetectionPluginStop: function() {
    if (document.mWebGLSpeechDetectionPluginRecognition == undefined) {
      return;
    }
    document.mWebGLSpeechDetectionPluginRecognition.stop();
  },

  WebGLSpeechDetectionPluginGetNumberOfResults: function() {
    if (document.mWebGLSpeechDetectionPluginResults == undefined) {
      document.mWebGLSpeechDetectionPluginResults = [];
    }  
    //console.log("GetNumberOfResults length="+document.mWebGLSpeechDetectionPluginResults.length);
    return document.mWebGLSpeechDetectionPluginResults.length;
  },

  WebGLSpeechDetectionPluginGetResult: function() {
    if (document.mWebGLSpeechDetectionPluginResults == undefined) {
      document.mWebGLSpeechDetectionPluginResults = [];
    }
    //console.log("GetResult:");

    if (document.mWebGLSpeechDetectionPluginResults.length == 0) {
      returnStr = "No results available";
    } else {
      returnStr = document.mWebGLSpeechDetectionPluginResults[0];
    }

    document.mWebGLSpeechDetectionPluginResults = document.mWebGLSpeechDetectionPluginResults.splice(1);

	var bufferLength = lengthBytesUTF8(returnStr) + 1;
    var buffer = _malloc(bufferLength);
	if (stringToUTF8 == undefined) {
		writeStringToMemory(returnStr, buffer);
	} else {
		stringToUTF8(returnStr, buffer, bufferLength);
	}
    return buffer;
  },
  
  WebGLSpeechDetectionPluginGetLanguages: function() {
    //console.log("GetLanguages:");

    document.mWebGLSpeechDetectionLanguages =
	{
		"languages": [{
			"name": "Arabic",
			"dialects": [{
				"name": "ar-IL",
				"display": "Arabic (Israel)"
			}, {
				"name": "ar-JO",
				"display": "Arabic (Jordan)"
			}, {
				"name": "ar-AE",
				"display": "Arabic (United Arab Emirates)"
			}, {
				"name": "ar-BH",
				"display": "Arabic (Bahrain)"
			}, {
				"name": "ar-DZ",
				"display": "Arabic (Algeria)"
			}, {
				"name": "ar-SA",
				"display": "Arabic (Saudi Arabia)"
			}, {
				"name": "ar-IQ",
				"display": "Arabic (Iraq)"
			}, {
				"name": "ar-KW",
				"display": "Arabic (Kuwait)"
			}, {
				"name": "ar-MA",
				"display": "Arabic (Morocco)"
			}, {
				"name": "ar-TN",
				"display": "Arabic (Tunisia)"
			}, {
				"name": "ar-OM",
				"display": "Arabic (Oman)"
			}, {
				"name": "ar-PS",
				"display": "Arabic (State of Palestine)"
			}, {
				"name": "ar-QA",
				"display": "Arabic (Qatar)"
			}, {
				"name": "ar-LB",
				"display": "Arabic (Lebanon)"
			}, {
				"name": "ar-EG",
				"display": "Arabic (Egypt)"
			}]
		}, {
			"name": "Afrikaans",
			"dialects": [{
				"name": "af-ZA",
				"display": "Afrikaans (South Africa)"
			}]
		}, {
			"name": "Bahasa Indonesia",
			"dialects": [{
				"name": "id-ID",
				"display": "Indonesian (Indonesia)"
			}]
		}, {
			"name": "Bahasa Melayu",
			"dialects": [{
				"name": "ms-MY",
				"display": "Malay (Malaysia)"
			}]
		}, {
			"name": "Català",
			"dialects": [{
				"name": "ca-ES",
				"display": "Catalan (Spain)"
			}]
		}, {
			"name": "Čeština",
			"dialects": [{
				"name": "cs-CZ",
				"display": "Czech (Czech Republic)"
			}]
		}, {
			"name": "Dansk",
			"dialects": [{
				"name": "da-DK",
				"display": "Danish (Denmark)"
			}]
		}, {
			"name": "Deutsch",
			"dialects": [{
				"name": "de-DE",
				"display": "German (Germany)"
			}]
		}, {
			"name": "English",
			"dialects": [{
				"name": "en-AU",
				"description": "Australia"
			}, {
				"name": "en-CA",
				"description": "Canada"
			}, {
				"name": "en-IN",
				"description": "India"
			}, {
				"name": "en-NZ",
				"description": "New Zealand"
			}, {
				"name": "en-ZA",
				"description": "South Africa"
			}, {
				"name": "en-GB",
				"description": "United Kingdom"
			}, {
				"name": "en-US",
				"description": "United States"
			}]
		}, {
			"name": "Español",
			"dialects": [{
				"name": "es-AR",
				"description": "Argentina",
				"display": "Spanish (Argentina)"
			}, {
				"name": "es-BO",
				"description": "Bolivia",
				"display": "Spanish (Bolivia)"
			}, {
				"name": "es-CL",
				"description": "Chile",
				"display": "Spanish (Chile)"
			}, {
				"name": "es-CO",
				"description": "Colombia",
				"display": "Spanish (Colombia)"
			}, {
				"name": "es-CR",
				"description": "Costa Rica",
				"display": "Spanish (Costa Rica)"
			}, {
				"name": "es-EC",
				"description": "Ecuador",
				"display": "Spanish (Ecuador)"
			}, {
				"name": "es-SV",
				"description": "El Salvador",
				"display": "Spanish (El Salvador)"
			}, {
				"name": "es-ES",
				"description": "España",
				"display": "Spanish (Spain)"
			}, {
				"name": "es-US",
				"description": "Estados Unidos",
				"display": "Spanish (United States)"
			}, {
				"name": "es-GT",
				"description": "Guatemala",
				"display": "Spanish (Guatemala)"
			}, {
				"name": "es-HN",
				"description": "Honduras",
				"display": "Spanish (Honduras)"
			}, {
				"name": "es-MX",
				"description": "México",
				"display": "Spanish (Mexico)"
			}, {
				"name": "es-NI",
				"description": "Nicaragua",
				"display": "Spanish (Nicaragua)"
			}, {
				"name": "es-PA",
				"description": "Panamá",
				"display": "Spanish (Panama)"
			}, {
				"name": "es-PY",
				"description": "Paraguay",
				"display": "Spanish (Paraguay)"
			}, {
				"name": "es-PE",
				"description": "Perú",
				"display": "Spanish (Peru)"
			}, {
				"name": "es-PR",
				"description": "Puerto Rico",
				"display": "Spanish (Puerto Rico)"
			}, {
				"name": "es-DO",
				"description": "República Dominicana",
				"display": "Spanish (Dominican Republic)"
			}, {
				"name": "es-UY",
				"description": "Uruguay",
				"display": "Spanish (Uruguay)"
			}, {
				"name": "es-VE",
				"description": "Venezuela",
				"display": "Spanish (Venezuela)"
			}]
		}, {
			"name": "Euskara",
			"dialects": [{
				"name": "eu-ES",
				"display": "Basque (Spain)"
			}]
		}, {
			"name": "Filipino",
			"dialects": [{
				"name": "fil-PH",
				"display": "Filipino (Philippines)"
			}]
		}, {
			"name": "Français",
			"dialects": [{
				"name": "fr-FR",
				"display": "French (France)"
			}]
		}, {
			"name": "Galego",
			"dialects": [{
				"name": "gl-ES",
				"display": "Galician (Spain)"
			}]
		}, {
			"name": "Hrvatski",
			"dialects": [{
				"name": "hr-HR",
				"display": "Croatian (Croatia)"
			}]
		}, {
			"name": "IsiZulu",
			"dialects": [{
				"name": "zu-ZA",
				"display": "Zulu (South Africa)"
			}]
		}, {
			"name": "Íslenska",
			"dialects": [{
				"name": "is-IS",
				"display": "Icelandic (Iceland)"
			}]
		}, {
			"name": "Italiano",
			"dialects": [{
				"name": "it-IT",
				"description": "Italia",
				"display": "Italian (Italy)"
			}, {
				"name": "it-CH",
				"description": "Svizzera"
			}]
		}, {
			"name": "Lietuvių",
			"dialects": [{
				"name": "lt-LT",
				"display": "Lithuanian (Lithuania)"
			}]
		}, {
			"name": "Magyar",
			"dialects": [{
				"name": "hu-HU",
				"display": "Hungarian (Hungary)"
			}]
		}, {
			"name": "Nederlands",
			"dialects": [{
				"name": "nl-NL",
				"display": "Dutch (Netherlands)"
			}]
		}, {
			"name": "Norsk bokmål",
			"dialects": [{
				"name": "nb-NO",
				"display": "Norwegian Bokmål (Norway)"
			}]
		}, {
			"name": "Polski",
			"dialects": [{
				"name": "pl-PL",
				"display": "Polish (Poland)"
			}]
		}, {
			"name": "Português",
			"dialects": [{
				"name": "pt-BR",
				"description": "Brasil",
				"display": "Portuguese (Brazil)"
			}, {
				"name": "pt-PT",
				"description": "Portugal",
				"display": "Portuguese (Portugal)"
			}]
		}, {
			"name": "Română",
			"dialects": [{
				"name": "ro-RO",
				"display": "Romanian (Romania)"
			}]
		}, {
			"name": "Slovenščina",
			"dialects": [{
				"name": "sl-SI",
				"display": "Slovenian (Slovenia)"
			}]
		}, {
			"name": "Slovenčina",
			"dialects": [{
				"name": "sk-SK",
				"display": "Slovak (Slovakia)"
			}]
		}, {
			"name": "Suomi",
			"dialects": [{
				"name": "fi-FI",
				"display": "Finnish (Finland)"
			}]
		}, {
			"name": "Svenska",
			"dialects": [{
				"name": "sv-SE",
				"display": "Swedish (Sweden)"
			}]
		}, {
			"name": "Tiếng Việt",
			"dialects": [{
				"name": "vi-VN",
				"display": "Vietnamese (Vietnam)"
			}]
		}, {
			"name": "Türkçe",
			"dialects": [{
				"name": "tr-TR",
				"display": "Turkish (Turkey)"
			}]
		}, {
			"name": "Ελληνικά",
			"display": "Greek",
			"dialects": [{
				"name": "el-GR",
				"display": "Greek (Greece)"
			}]
		}, {
			"name": "български",
			"display": "Bulgarian",
			"dialects": [{
				"name": "bg-BG",
				"display": "Bulgarian (Bulgaria)"
			}]
		}, {
			"name": "Pусский",
			"display": "Russian",
			"dialects": [{
				"name": "ru-RU",
				"display": "Russian (Russia)"
			}]
		}, {
			"name": "Српски",
			"display": "Serbian",
			"dialects": [{
				"name": "sr-RS",
				"display": "Serbian (Serbia)"
			}]
		}, {
			"name": "Українська",
			"display": "Ukrainian",
			"dialects": [{
				"name": "uk-UA",
				"display": "Ukrainian (Ukraine)"
			}]
		}, {
			"name": "한국어",
			"display": "Korean",
			"dialects": [{
				"name": "ko-KR",
				"display": "Korean (South Korea)"
			}]
		}, {
			"name": "中文",
			"display": "Chinese",
			"dialects": [{
				"name": "cmn-Hans-CN",
				"description": "普通话 (中国大陆)",
				"display": "Mandarin (Simplified, China)"
			}, {
				"name": "cmn-Hans-HK",
				"description": "普通话 (香港)",
				"display": "Mandarin (Simplified, Hong Kong)"
			}, {
				"name": "cmn-Hant-TW",
				"description": "中文 (台灣)",
				"display": "Mandarin (Traditional, Taiwan)"
			}, {
				"name": "yue-Hant-HK",
				"description": "粵語 (香港)",
				"display": "Cantonese (Traditional, Hong Kong)"
			}]
		}, {
			"name": "日本語",
			"display": "Japanese",
			"dialects": [{
				"name": "ja-JP",
				"display": "Japanese (Japan)"
			}]
		}, {
			"name": "हिन्दी",
			"display": "Hindi",
			"dialects": [{
				"name": "hi-IN",
				"display": "Hindi (India)"
			}]
		}, {
			"name": "ภาษาไทย",
			"display": "Thai",
			"dialects": [{
				"name": "th-TH",
				"display": "Thai (Thailand)"
			}]
		}]
	};

    //console.log(JSON.stringify(document.mWebGLSpeechDetectionLanguages, undefined, 2));
    var returnStr = JSON.stringify(document.mWebGLSpeechDetectionLanguages);

    var bufferLength = lengthBytesUTF8(returnStr) + 1;
	var buffer = _malloc(bufferLength);
	if (stringToUTF8 == undefined) {
		writeStringToMemory(returnStr, buffer);
	} else {
		stringToUTF8(returnStr, buffer, bufferLength);
	}
    return buffer;
  },
  
  WebGLSpeechDetectionPluginSetLanguage: function(dialect) {
    //console.log("SetLanguage: "+Pointer_stringify(dialect));

    if (document.mWebGLSpeechDetectionPluginRecognition == undefined) {
      return;
    }

    document.mWebGLSpeechDetectionPluginRecognition.lang = Pointer_stringify(dialect);
  }
});
