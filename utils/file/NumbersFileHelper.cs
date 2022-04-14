using System.Text.Json;
using dc.assignment.primenumbers.dto;
using dc.assignment.primenumbers.utils.api;

namespace dc.assignment.primenumbers.utils.file
{
    public class NumbersFileHelper
    {
        private APIInvocationHandler apiInvocationHandler;
        private static string SERVICE_BASE_URL = "http://127.0.0.1:8282";
        public NumbersFileHelper()
        {
            // API handler
            this.apiInvocationHandler = new APIInvocationHandler();
        }

        public int getNextNumber()
        {
            string jsonStr = this.apiInvocationHandler.invokeGET(SERVICE_BASE_URL + "/getNextNumber");
            NextNumberDTO? dto = JsonSerializer.Deserialize<NextNumberDTO>(jsonStr);
            return dto.number;
        }

        public bool completeNumber(int theNumber, bool isPrime)
        {
            var obj = new
            {
                number = theNumber,
                isPrime = isPrime
            };

            string jsonStr = this.apiInvocationHandler.invokePOST(SERVICE_BASE_URL + "/completeNumber", obj);
            return true;
        }

    }
}