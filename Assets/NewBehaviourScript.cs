using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.ShaderKeywordFilter;
using UnityEngine;
using UnityEngine.UI;

public class NewBehaviourScript : MonoBehaviour
{
    public float FM = 44100f;
    int TM;
    [Range(20, 20000)]
    public float frecuencia = 100;
    public float TiempoSegundos = 2.0f;
    AudioSource audioSource;
    int timeIndex = 0;
    public Slider selector, level;
    public TextMeshProUGUI textoSeleccion, textonivel;

    //int funcion = 0;  // Variable para seleccionar la forma de onda

    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0;
        audioSource.Stop();
    }

    void Update()
    {
        // Manejar la entrada para cambiar la forma de onda
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (!audioSource.isPlaying)
            {
                timeIndex = 0;
                audioSource.Play();
                funcion = 0; // Seno
            }
            else
            {
                audioSource.Stop();
            }
        }
        else if (Input.GetKeyDown(KeyCode.A))
        {
            if (!audioSource.isPlaying)
            {
                timeIndex = 0;
                audioSource.Play();
                funcion = 1; // Cuadrada
            }
            else
            {
                audioSource.Stop();
            }
        }
        else if (Input.GetKeyDown(KeyCode.T))
        {
            if (!audioSource.isPlaying)
            {
                timeIndex = 0;
                audioSource.Play();
                funcion = 2; // Triangular
            }
            else
            {
                audioSource.Stop();
            }
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            if (!audioSource.isPlaying)
            {
                timeIndex = 0;
                audioSource.Play();
                funcion = 3; // Diente de sierra
            }
            else
            {
                audioSource.Stop();
            }
        }
        TM = (int)(FM / frecuencia);
        funcion = (int)selector.value;
    }

    public int funcion = 0; 
    // Función para generar una onda senoidal
    public float CreateSeno(int timeIndex, float frecuencia)
    {
        return Mathf.Sin(2 * Mathf.PI * timeIndex * frecuencia / FM);
    }

    // Función para generar una onda cuadrada
    public float CreateSquare(int timeIndex, float frecuencia)
    {
        return Mathf.Sign(Mathf.Sin(2 * Mathf.PI * timeIndex * frecuencia / FM));
    }

    // Función para generar una onda triangular
    public float CreateTriangle(int timeIndex, float frecuencia)
    {
        float m1 = 1 / (TM / 4.0f);
        float m2 = -2 / ((TM * (3 / 4.0f) - (TM - 4.0f)));
        float m3 = 1 / (TM - (TM * (3 / 4.0f)));

        float b1 = 1 - (m1 * (TM / 4.0f));
        float b2 = 1 - (m2 * (TM / 4.0f));
        float b3 = 0 - (m3 * TM);

        if (timeIndex <= (TM / 4.0f))
        {
            return (m1 * timeIndex + b1);
        }
        else if (timeIndex > (TM / 4.0f) && timeIndex <= (TM * (3 / 4.0f)))
        {
            return (m2 * timeIndex + b2);
        }
        else
        {
            return (m3 * timeIndex + b3);
        }
    }

    // Función para generar una onda diente de sierra
    public float CreateSawTooth(int timeIndex, float frecuencia)
    {
        float m1 = 1 / ((TM / 2.0f));
        float m2 = 1 / (TM - ((TM) / 2.0f));

        float b1 = 1 - (m1 * (TM / 2));
        float b2 = 0 - (m2 * TM);

        if (timeIndex <= (TM / 2))
        {
            return (m1 * timeIndex + b1);
        }
        else
        {
            return (m2 * timeIndex + b2);
        }
    }

    public void KeyboardDown(float f)
    {
        frecuencia = f;
        if (!audioSource.isPlaying)
        {
            timeIndex = 0;
            audioSource.Play();
            funcion = 0;
        }
    }

    public void KeyboardUp()
    {
        frecuencia = 0;
        audioSource.Stop();
        timeIndex = 0;
    }

    public void Selection()
    {
        funcion = (int)selector.value;
        switch (funcion)
        {
            case 0:
                textoSeleccion.SetText("Sine Wave");
                break;
            case 1:
                textoSeleccion.SetText("Square Wave");
                break;
            case 2:
                textoSeleccion.SetText("Triangle Wave");
                break;
            case 3:
                textoSeleccion.SetText("SawTooth Wave");
                break;
            default:
                Debug.Log("¿Qué te pasa? ¿No sabes programar?");
                break;
        }
    }

    //int Vref = 32768;
    float DBFStoLinear(float dBfs)
    {
        return Mathf.Pow(10f, dBfs / 20f);
    }

    float nivel = 1;
    public void amplitud()
    {
        nivel = DBFStoLinear(level.value);
        textonivel.SetText(Mathf.Round(level.value).ToString());
    }

    // Función que se llama cada vez que se necesita procesar el audio
    void OnAudioFilterRead(float[] data, int channels)
    {
        float x = 0;
         // Generar onda senoidal
        for (int i = 0; i < data.Length; i += channels)
        {
            switch (funcion)
            {
                case 0:
                    x = CreateSeno(timeIndex, frecuencia);
                    if (timeIndex>=(FM*TiempoSegundos))
                    {
                        timeIndex = 0;
                    }
                    break;
                case 1:
                    x = CreateSquare(timeIndex, frecuencia);
                    if (timeIndex >= (FM * TiempoSegundos))
                    {
                        timeIndex = 0;
                    }
                    break;
                case 2:
                    x = CreateTriangle(timeIndex, frecuencia);
                    if (timeIndex >= TM)
                    {
                        timeIndex = 0;
                    }
                    break;
                case 3:
                    x = CreateSawTooth(timeIndex, frecuencia);
                    if (timeIndex >= TM)
                    {
                        timeIndex = 0;
                    }
                    break;
                default:
                    Debug.Log("¿Qué te pasa? ¿No sabes programar?");
                    break;
            }

            data[i] = x * nivel;

            if (channels == 2)
                data[i + 1] = x * nivel;

            timeIndex++;
        }
    } 
}
