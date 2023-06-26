namespace Frontend.Classes
{
    public class HttpClass
    {
        private static HttpClass? _instance;
        private Uri BackendUrl { get; set; }
        private HttpClass(string url) 
        {
            BackendUrl = new(url);
        }
        public static HttpClass Init(string url)
        {
            if (_instance == null)
                _instance = new HttpClass(url);
            return _instance;
        }
        public static HttpClass GetInstance() 
        {
            if (_instance != null)
                return _instance;
            throw new NullReferenceException("Объект не инициализированн");
        }

        public async Task<HttpResponseMessage> Post(string path, object content)
        {
            var httpClient = new HttpClient();
            var response = await httpClient.PostAsJsonAsync(BackendUrl + path, content);
            Console.WriteLine(BackendUrl + path + '/' + content);
            return response;
        }
    
        public async Task<HttpResponseMessage> Get(string path, object content)
        {
            var httpClient = new HttpClient();
            var response = await httpClient.GetAsync(BackendUrl + path + content);
            return response;
        }
    }
}
