using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq; 
using UnityEngine;
using UnityEngine.Windows.Speech;
using TMPro;

public class RocketScript : MonoBehaviour {
  private Dictionary<string, string[]> wordListAttr = new Dictionary<string, string[]>(); 
  private List<string> wordList = new List<string>{}; 
  private KeywordRecognizer kr;
  private DictationRecognizer dr;
  private bool mode = false;
  private Transform[] allFoods;
  private GameObject listeningFood;
  private GameObject listeningInscription;

  private TextMeshPro dicText;

  void Start() {
    Transform allFoodTr = gameObject.transform.GetChild(1);
    allFoods = allFoodTr.gameObject.GetComponentsInChildren<Transform>(true);
    allFoods = allFoods.Skip(1).ToArray();

    listeningFood = gameObject.transform.GetChild(3).gameObject;
    listeningInscription = gameObject.transform.GetChild(4).gameObject;

    wordListAttr["Olive"] = new string[2]{"Aceituna", "Oliva"};
    wordListAttr["Hamburger"] = new string[2]{"Hamburguesa", "Burguer"};
    wordListAttr["Banana"] = new string[2]{"Plátano", "Banana"};
    wordListAttr["Hotdog"] = new string[1]{"Perrito Caliente"};
    wordListAttr["Cheese"] = new string[1]{"Queso"};
    wordListAttr["Cherry"] = new string[2]{"Cerezas", "Picotas"};
    wordListAttr["Watermelon"] = new string[1]{"Sandía"};

    string[] keys = wordListAttr.Keys.ToArray();
    foreach (string key in keys) {
      foreach (string token in wordListAttr[key]) {
        wordList.Add(token);
      }
    }

    dicText = gameObject.transform.GetChild(2).gameObject.GetComponent<TextMeshPro>();
  }

  void Update() {
    if (Input.GetKeyDown("c")) {
      if (!mode) {
        if (kr != null) {
          if (kr.IsRunning) {
            kr.Stop();
          }
          kr.Dispose();
          kr = null;
        }
        listeningFood.SetActive(false);
        foreach (Transform foodTr in allFoods) {
          foodTr.gameObject.SetActive(false);
        }
      } else {
        if (dr != null) {
          if (dr.Status != SpeechSystemStatus.Stopped) {
            dr.Stop();
          }
          dr.Dispose();
          dr = null;
        }
        listeningInscription.SetActive(false);
        dicText.text = "";
      }
      mode = !mode;
    } else if (Input.GetKeyDown("space")) {
      if (mode) {
        if (dr == null || dr.Status != SpeechSystemStatus.Running) {
          if (dr == null) {
            PhraseRecognitionSystem.Shutdown();
            dr = new DictationRecognizer();
            dr.DictationResult += DicRecognizer;
          }
          dr.Start();
          listeningInscription.SetActive(true);
        }
        else {
          dr.Stop();
          listeningInscription.SetActive(false);
        }
      } else {
        if (kr == null || !kr.IsRunning) {
          if (kr == null) {
            kr = new KeywordRecognizer(wordList.ToArray());
            kr.OnPhraseRecognized += KeyRecognized;
          }
          kr.Start();
          listeningFood.SetActive(true);
        }
        else {
          kr.Stop();
          listeningFood.SetActive(false);
        }
      }
    }
  }

  void KeyRecognized(PhraseRecognizedEventArgs data) {
    if (data.confidence == ConfidenceLevel.High ||
        data.confidence == ConfidenceLevel.Medium) {
      foreach (Transform foodTr in allFoods) {
        GameObject food = foodTr.gameObject;
        foreach (string foodName in wordListAttr[food.name]) {
          if (foodName == data.text) {
            Debug.Log("Detectando " + data.text +
                " con un nivel de confianza " + data.confidence);
            food.SetActive(true);
            break;
          }
          food.SetActive(false);
        }
      }
    }
  }

  void DicRecognizer(string text, ConfidenceLevel confidence) {
    Debug.Log(text + " - " + confidence);
    dicText.text = text;
  }
}
