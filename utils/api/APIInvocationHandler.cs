using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace dc.assignment.primenumbers.utils.api
{
    public class APIInvocationHandler
    {
        public string invokePOST(string url, object obj)
        {
            using (var client = new HttpClient())
            {
                string json = JsonSerializer.Serialize(obj);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = client.PostAsync(url, content).Result;

                if (response.IsSuccessStatusCode)
                {
                    string responseString = response.Content.ReadAsStringAsync().Result;
                    return responseString;
                }
            }
            return null;
        }

        public string invokeGET(string url)
        {
            using (var client = new HttpClient())
            {
                var response = client.GetAsync(url).Result;

                if (response.IsSuccessStatusCode)
                {
                    string responseString = response.Content.ReadAsStringAsync().Result;
                    return responseString;
                }
            }
            return null;
        }
    }
}