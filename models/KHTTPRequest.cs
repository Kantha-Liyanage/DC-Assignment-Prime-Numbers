namespace dc.assignment.primenumbers.models{

    class KHTTPRequest{

        private HTTPMethod httpMethod;
        private string resourceURL;
        private List<KeyValuePair<string,string>> headers;
        private string bodyContent;

        public KHTTPRequest(string requestData){

        }
    }

    enum HTTPMethod{
        GET,
        POST
    }

}