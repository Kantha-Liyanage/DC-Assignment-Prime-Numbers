using System;
using System.Net.Http;
using System.Text;

namespace dc.assignment.primenumbers.utils.api
{
    public class APIInvocationHandler
    {
        public string invokePOST(string url, string json)
        {
            using (var client = new HttpClient())
            {
                try
                {
                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    var response = client.PostAsync(url, content).Result;

                    if (response.IsSuccessStatusCode)
                    {
                        string responseString = response.Content.ReadAsStringAsync().Result;
                        return responseString;
                    }
                }
                catch (Exception er)
                {
                    Console.WriteLine("Error: " + er.Message);
                }
            }
            return null;
        }

        public string invokeGET(string url)
        {
            using (var client = new HttpClient())
            {
                try
                {
                    var response = client.GetAsync(url).Result;

                    if (response.IsSuccessStatusCode)
                    {
                        string responseString = response.Content.ReadAsStringAsync().Result;
                        return responseString;
                    }
                }
                catch (Exception er)
                {
                    Console.WriteLine("Error: " + er.Message);
                }
            }
            return null;
        }
    }
}