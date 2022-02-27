namespace dc.assignment.primenumbers.models{

    class KHTTPRequest{

        private HTTPMethod _httpMethod;
        private string _resourceURL;
        private List<KeyValuePair<string,string>> _headers = new List<KeyValuePair<string, string>>();
        private List<KeyValuePair<string,string>> _params = new List<KeyValuePair<string, string>>();
        private string _bodyContent;

        public KHTTPRequest(string requestData){
            //HTTP Method
            int i = requestData.IndexOf('/');
            string str1 = requestData.Substring(0, i);
            str1 = str1.Trim();
            switch(str1){
                case "GET": _httpMethod = HTTPMethod.GET; break;
                case "POST": _httpMethod = HTTPMethod.POST; break;
                default: _httpMethod = HTTPMethod.ERROR;break;
            }

            //Resource URL
            int j = requestData.IndexOf("HTTP/");
            str1 = requestData.Substring(i+1, j-i-1);
            string completeURL = str1.Trim();
            if(completeURL.IndexOf('?')>-1){
                string[] urlParts = completeURL.Split('?');
                _resourceURL = urlParts[0];

                //Params
                string[] paramsStr = urlParts[1].Split("&");
                for(int x=0;x<paramsStr.Length;x++){
                    string[] keyValueStr = paramsStr[x].Split('=');
                    _params.Add(new KeyValuePair<string, string>(keyValueStr[0], keyValueStr[1].Trim()));
                }
            }
            else{
                _resourceURL = completeURL;
            }

            //Other headers and body
            string[] parts = requestData.Split("\r\n");
            //Headers
            for(int k=1;k<(parts.Length-2);k++){
                string[] keyValueStr = parts[k].Split(':');
                _headers.Add(new KeyValuePair<string, string>(keyValueStr[0], keyValueStr[1].Trim()));
            }
            //Body
            if(httpMethod == HTTPMethod.POST){
                _bodyContent = parts[parts.Length-1];
            }
        }

        public HTTPMethod httpMethod { get => _httpMethod; }
        public string resourceURL { get => _resourceURL; }
        public List<KeyValuePair<string,string>> headers { get => _headers; }
        public List<KeyValuePair<string,string>> urlParams { get => _params; }
        public string bodyContent { get => _bodyContent; }
    }

    enum HTTPMethod{
        GET,
        POST,
        ERROR
    }

}