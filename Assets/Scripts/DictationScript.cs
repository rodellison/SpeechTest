//using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Windows.Speech;
using System.Text;
using System;
using System.Collections.Generic;
using System.Linq;

public class DictationScript : MonoBehaviour
{
	[SerializeField]
	private Text m_Hypotheses;

	[SerializeField]
	private Text m_Recognitions;

	[SerializeField]
	private string[] m_Keywords;

	private DictationRecognizer m_DictationRecognizer;
	private GrammarRecognizer m_GrammerRecognizer;
	private KeywordRecognizer m_KeyWordRecognizer;

	void Start() {		
		PhraseRecognitionSystem.OnStatusChanged += SpeechSystemStatusFn;	
	}

	void SpeechSystemStatusFn(SpeechSystemStatus status) {
		Debug.Log("Speech System Status: " + status);
	}

	#region DictationRecognition
	public void StartDictation() {

		if (PhraseRecognitionSystem.Status != SpeechSystemStatus.Stopped)
		{
			PhraseRecognitionSystem.Shutdown();
		}			
			
		m_DictationRecognizer = new DictationRecognizer();
		m_DictationRecognizer.DictationResult += (string text, ConfidenceLevel confidence) => {
			m_Recognitions.text += text + "\n";
		};
		m_DictationRecognizer.DictationHypothesis += ((string text) => {
			m_Hypotheses.text += text + "\n";
		});
		m_DictationRecognizer.DictationComplete += ((DictationCompletionCause completionCause) => {
			if (completionCause != DictationCompletionCause.Complete)
				Debug.LogErrorFormat("Dictation completed unsuccessfully: {0}.", completionCause);
		});
		m_DictationRecognizer.DictationError += ((string error, int hresult) => {
			Debug.LogErrorFormat("Dictation error: {0}; HResult = {1}.", error, hresult);
		});	

		m_DictationRecognizer.Start();
		m_Recognitions.text = "";
		m_Hypotheses.text = "";
		enableUI("StartDictation");

	}

	public void EndDictation() {

		m_DictationRecognizer.Dispose();
		m_DictationRecognizer.Stop();
		m_DictationRecognizer = null;   
		Debug.Log("Dictation stopped");
		enableUI("EndDictation");

	}

	#endregion 

	#region GrammerRecognition
	public void StartGrammer() {

		if (PhraseRecognitionSystem.Status != SpeechSystemStatus.Stopped)
		{
			PhraseRecognitionSystem.Shutdown();
		}			

		try {
			m_GrammerRecognizer = new GrammarRecognizer(Application.streamingAssetsPath + "/SRGS/AmexGrammer.xml", ConfidenceLevel.Low);	
		} catch (Exception ex) {			
			Debug.Log(ex.Message);
		}
			
		m_GrammerRecognizer.OnPhraseRecognized += ((PhraseRecognizedEventArgs args) => {
			Debug.Log("Phrase recognized..");
			StringBuilder builder = new StringBuilder();
			builder.AppendFormat("{0} ({1}){2}", args.text, args.confidence, Environment.NewLine);
			m_Recognitions.text += builder.ToString();
		});

		m_Recognitions.text = "";

		m_GrammerRecognizer.Start();

		m_Hypotheses.text = m_GrammerRecognizer.IsRunning? "True": "False";
		m_Hypotheses.text += "\n" + m_GrammerRecognizer.GrammarFilePath;
		enableUI("StartGrammer");

	}

	public void EndGrammer() {

		if (m_GrammerRecognizer.IsRunning)
		{
			Debug.Log("Stopping Grammer Recognizer");
			m_GrammerRecognizer.Stop();
		}
				
		m_GrammerRecognizer.Dispose();
		m_GrammerRecognizer = null;
		Debug.Log("Grammer stopped");
		enableUI("EndGrammer");

	}

	#endregion

	#region KeywordsRecognition
	public void StartKeywordRecognizer() {

		if (PhraseRecognitionSystem.Status != SpeechSystemStatus.Stopped)
		{
			PhraseRecognitionSystem.Shutdown();
		}			

		m_KeyWordRecognizer = new KeywordRecognizer(m_Keywords, ConfidenceLevel.Low);		
				
		m_KeyWordRecognizer.OnPhraseRecognized += ((PhraseRecognizedEventArgs args) => {
			Debug.Log("Keyword recognized..");
			StringBuilder builder = new StringBuilder();
			builder.AppendFormat("{0} ({1}){2}", args.text, args.confidence, Environment.NewLine);
			m_Recognitions.text += builder.ToString();
		});

		m_Recognitions.text = "";
		m_KeyWordRecognizer.Start();
		m_Hypotheses.text = "Listening for: \n" + string.Join(", ", m_Keywords);
		enableUI("StartKeyword");

	}

	public void EndKeywordRecognizer() {

		if (m_KeyWordRecognizer.IsRunning)
		{
			Debug.Log("Stopping Keyword Recognizer");
			m_KeyWordRecognizer.Stop();
		}

		m_KeyWordRecognizer.Dispose();
		m_KeyWordRecognizer = null;
		Debug.Log("Keyword stopped");
		enableUI("EndKeyword");

	}

	#endregion

	#region UI
	public void enableUI(string btnClickedName) {
		
		switch (btnClickedName)
		{
			case "StartDictation":
				GameObject.Find("EndDictationButton").GetComponent<Button>().interactable = true;
				GameObject.Find("StartGrammerButton").GetComponent<Button>().interactable = false;	
				GameObject.Find("StartKeywordButton").GetComponent<Button>().interactable = false;
				GameObject.Find("StartDictationButton").GetComponent<Button>().interactable = false;
				break;
				case "StartGrammer":
				GameObject.Find("EndGrammerButton").GetComponent<Button>().interactable = true;
				GameObject.Find("StartGrammerButton").GetComponent<Button>().interactable = false;
				GameObject.Find("StartKeywordButton").GetComponent<Button>().interactable = false;
				GameObject.Find("StartDictationButton").GetComponent<Button>().interactable = false;
				break;
			case "StartKeyword":
				GameObject.Find("EndKeywordButton").GetComponent<Button>().interactable = true;
				GameObject.Find("StartGrammerButton").GetComponent<Button>().interactable = false;
				GameObject.Find("StartKeywordButton").GetComponent<Button>().interactable = false;
				GameObject.Find("StartDictationButton").GetComponent<Button>().interactable = false;
				break;
			case "EndDictation":
			case "EndGrammer":
			case "EndKeyword":
				GameObject.Find("EndDictationButton").GetComponent<Button>().interactable = false;
				GameObject.Find("EndGrammerButton").GetComponent<Button>().interactable = false;
				GameObject.Find("EndKeywordButton").GetComponent<Button>().interactable = false;
				GameObject.Find("StartGrammerButton").GetComponent<Button>().interactable = true;	
				GameObject.Find("StartKeywordButton").GetComponent<Button>().interactable = true;
				GameObject.Find("StartDictationButton").GetComponent<Button>().interactable = true;
				break;
			default:
				Console.WriteLine("Switch default case");
				break;
		}
		
	}
	#endregion


	}