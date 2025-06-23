using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;


namespace wetterapptest
{
    class Program
    {
        
        public class Location
        {
            public string lat { get; set; } 
            public string lon { get; set; } 
        }

        
        public class MeteomaticsResponse
        {
            public Datum[] data { get; set; } // Enthält die Wetterdaten
        }

        public class Datum
        {
            public Parameter[] coordinates { get; set; } // Enthält Koordinaten-bezogene Wetterparameter
        }

        public class Parameter
        {
            public Value[] dates { get; set; } // Enthält Zeitstempel und Messwert
        }

        public class Value
        {
            public string date { get; set; }  // Zeitpunkt der Messung
            public double value { get; set; } // Temperaturwert
        }

        
        static async Task Main(string[] args)
        {
            
            Console.Write("Stadt eingeben: ");
            string city = Console.ReadLine();

            try
            {
                // Koordinaten der Stadt ermitteln
                var (lat, lon) = await GetCoordinatesFromCity(city);
                Console.WriteLine($"Koordinaten: {lat}, {lon}");

                // Temperaturdaten von Meteomatics abrufen
                double temperature = await GetTemperatureFromMeteomatics(lat, lon);
                Console.WriteLine($"Aktuelle Temperatur in {city}: {temperature:F1} °C");
            }
            catch (Exception ex)
            {              
                Console.WriteLine("Fehler: " + ex.Message);
            }            
        }

        // Ruft die geografischen Koordinaten (Breite, Länge) einer Stadt über die OpenStreetMap Nominatim API ab
        public static async Task<(double lat, double lon)> GetCoordinatesFromCity(string city)
        {
            // URL zur Suche nach der Stadt
            string url = $"https://nominatim.openstreetmap.org/search?q={Uri.EscapeDataString(city)}&format=json&limit=1";

            using HttpClient client = new HttpClient();
            // Nominatim verlangt einen User-Agent in der Anfrage
            client.DefaultRequestHeaders.Add("User-Agent", "KoordinatenFetcherApp/1.0");

            // JSON-Antwort von der API abrufen
            string json = await client.GetStringAsync(url);
            var locations = JsonSerializer.Deserialize<Location[]>(json);

            // Prüfen, ob ein Ergebnis zurückkam
            if (locations == null || locations.Length == 0)
                throw new Exception("Ort nicht gefunden.");

            // Koordinaten als Double-Werte parsen (englische Kultur wegen Punkt statt Komma)
            double lat = double.Parse(locations[0].lat, System.Globalization.CultureInfo.InvariantCulture);
            double lon = double.Parse(locations[0].lon, System.Globalization.CultureInfo.InvariantCulture);

            return (lat, lon);
        }

        // Holt die aktuelle Temperatur vom Wetterdienst Meteomatics basierend auf Koordinaten
        public static async Task<double> GetTemperatureFromMeteomatics(double lat, double lon)
        {
            // Zugangsdaten für Meteomatics API
            string username = "lavieggmbh_kaupert_alex";
            string password = "uCUJ5bw77Y";

            // Anfrage-URL: aktueller Zeitpunkt ("now"), 2m-Temperatur in °C, für gegebene Koordinaten
            string requestUrl = $"https://api.meteomatics.com/now/t_2m:C/{lat.ToString(System.Globalization.CultureInfo.InvariantCulture)},{lon.ToString(System.Globalization.CultureInfo.InvariantCulture)}/json";

            using HttpClient client = new HttpClient();

            // Zugangsdaten in Base64 für HTTP Basic Auth encodieren
            var byteArray = Encoding.ASCII.GetBytes($"{username}:{password}");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

            // HTTP GET-Anfrage senden
            HttpResponseMessage response = await client.GetAsync(requestUrl);

            // Fehlerbehandlung bei fehlgeschlagener Antwort
            if (!response.IsSuccessStatusCode)
            {
                string err = await response.Content.ReadAsStringAsync();
                throw new Exception($"Fehler bei Meteomatics-Anfrage: {response.StatusCode}\n{err}");
            }

            // JSON-Antwort einlesen und deserialisieren
            string json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<MeteomaticsResponse>(json);

            // Temperaturwert aus dem verschachtelten Objekt extrahieren
            double temp = result.data[0].coordinates[0].dates[0].value;
            return temp;
        }
    }
}
