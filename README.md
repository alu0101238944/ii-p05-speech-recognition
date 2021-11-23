# Práctica 5: Reconocimiento de Voz <!-- omit in toc -->

## Tabla de contenidos <!-- omit in toc -->
- [Objetivo](#objetivo)
  - [Definición](#definición)
  - [Evidencia gráfica](#evidencia-gráfica)
- [Apartados](#apartados)
  - [Apartado 1: KeywordRecognizer](#apartado-1-keywordrecognizer)
  - [Apartado 2: DictationRecognizer](#apartado-2-dictationrecognizer)
- [Fichero RocketScript.cs](#fichero-rocketscriptcs)

## Realizado por: <!-- omit in toc -->

- José Daniel Escánez Expósito (alu0101238944)

# Objetivo

## Definición

El objetivo de esta práctica es aprender a utilizar las herramientas de reconocimiento de voz que Unity3D ofrece para `Windows 10`. Se debe importar el paquete `UnityEngine.Windows.Speech`, del que se utilizarán las clases `KeywordRecognizer` y `DictationRecognizer`. En la entrega se solicitan todos los [scripts](./scripts) desarrollados.

## Evidencia gráfica

[![Alt text](https://img.youtube.com/vi/lApzUgs7vtk/0.jpg)](https://www.youtube.com/watch?v=lApzUgs7vtk)

# Apartados

Se generó una escena con estética espacial (utilizando diferentes assets gratuitos de la store de Unity), haciendo uso de sobras, luces, partículas, y otros elementos visuales. Se modificó el terreno para darle una apariencia realista (haciendo uso de mapas de altura) y otros aspectos ambientales (como cambiar el `SkyBoxMaterial`).

Esta escena tiene 2 modos, uno por cada apartado. Se inicia en el primero por defecto. Para cambiar de uno a otro, se debe pulsar la tecla `C`. Para iniciar el reconocimiento en cada uno de ellos, se debe pulsar la tecla `espacio` y volver a pulsar para detenerlo. En caso de reconocer las palabras, se accionará el evento correspondiente.

```cs
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
            if (PhraseRecognitionSystem.Status != SpeechSystemStatus.Stopped) {
              PhraseRecognitionSystem.Shutdown();
            }
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
```

## Apartado 1: KeywordRecognizer

- Se utilizó esta clase `KeywordRecognizer` para detectar el nombre de diferentes alimentos (pudiendo utilizar algunos sinónimos) en el dispositivo de entrada de audio del usuario. La finalidad es cargar en la escena un modelo 3D del alimento nombrado.

```cs
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
```

## Apartado 2: DictationRecognizer

- Se utilizó esta clase `DictationRecognizer` para detectar una frase dicha por el usuario en su dispositivo de entrada de audio. La finalidad es cargar un texto en la escena con la frase dicha.

```cs
  void DicRecognizer(string text, ConfidenceLevel confidence) {
    Debug.Log(text + " - " + confidence);
    dicText.text = text;
  }
```

# Fichero RocketScript.cs

Este fichero se encuentra también [disponible](./scripts/RocketScript.cs) en el directorio de [scripts](./scripts).

```cs
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
            if (PhraseRecognitionSystem.Status != SpeechSystemStatus.Stopped) {
              PhraseRecognitionSystem.Shutdown();
            }
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
```
