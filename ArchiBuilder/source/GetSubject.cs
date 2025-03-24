namespace ArchiBuilder;

public static partial class ArchiBuilder
{
    private static (string, string) GetSubject(string subjectLink, CriUser user)
    {
        // Initialisation de la requête
        using HttpClientHandler handler = new HttpClientHandler();
        handler.AllowAutoRedirect = true;
        using HttpClient client = new HttpClient(handler);

        // Récupérer les cookies de session ainsi que le `csrfmiddlewaretoken`
        HttpRequestMessage initialRequest = new HttpRequestMessage(HttpMethod.Get, new Uri(subjectLink));
        HttpResponseMessage initialResponse = client.Send(initialRequest);
        string initialContent = new StreamReader(initialResponse.Content.ReadAsStream()).ReadToEnd();
        string formToken = FindFormToken(initialContent);

        // Connection au site
        FormUrlEncodedContent content = new FormUrlEncodedContent(new[]
        {
            KeyValuePair.Create("csrfmiddlewaretoken", formToken),
            KeyValuePair.Create("username", user.Username),
            KeyValuePair.Create("password", user.Password)
        });

        HttpRequestMessage loginRequest = new HttpRequestMessage(HttpMethod.Post, initialResponse.RequestMessage?.RequestUri);
        loginRequest.Headers.Referrer = initialResponse.RequestMessage?.RequestUri;
        loginRequest.Content = content;
        HttpResponseMessage loginResponse = client.Send(loginRequest);

        if (!loginResponse.IsSuccessStatusCode) throw new ArchiBuilderExceptions.InvalidCredentialsException();

        string forgePageCode = new StreamReader(loginResponse.Content.ReadAsStream()).ReadToEnd();
        
        HttpRequestMessage subjectRequest = new HttpRequestMessage(HttpMethod.Get, new Uri(subjectLink + "/subject.html"));
        subjectRequest.Headers.Referrer = loginResponse.RequestMessage?.RequestUri;
        HttpResponseMessage subjectResponse = client.Send(subjectRequest);
        
        string subjectPageCode = new StreamReader(subjectResponse.Content.ReadAsStream()).ReadToEnd();
        
        return (forgePageCode,subjectPageCode);
    }
}