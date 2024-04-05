using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class WeatherController : MonoBehaviour
{
    // Your WeatherAPI.com API key
    public string apiKey = "16882ae5823e44c7955170303242003";

    // Location to fetch weather data for
    public string location = "Cluj-Napoca";

    // URL for the current weather endpoint
    private string apiUrl;

    private string weatherJsonData;

    void Start()
    {
        apiUrl = $"http://api.weatherapi.com/v1/current.json?key={apiKey}&q={location}";

        // Start the coroutine to fetch weather data
        StartCoroutine(FetchWeatherData());
    }

    IEnumerator FetchWeatherData()
    {
        // Create UnityWebRequest to fetch data
        using (UnityWebRequest webRequest = UnityWebRequest.Get(apiUrl))
        {
            // Send the request
            yield return webRequest.SendWebRequest();

            // Check for errors
            if (webRequest.result == UnityWebRequest.Result.ConnectionError ||
                webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Error: " + webRequest.error);
            }
            else
            {
                // Read the response
                weatherJsonData = webRequest.downloadHandler.text;

                // Output the response
                Debug.Log(weatherJsonData);
            }
        }
    }
}
