using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace HomepageAzureFunction.Functions;

public class SaveUrlFunction
{
    private const string Username = "your-username";  // Replace with your username
    private const string Password = "your-password";  // Replace with your password

    [Function("SaveUrlFunction")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData req,
        FunctionContext executionContext)
    {
        var logger = executionContext.GetLogger("SaveUrlFunction");
        logger.LogInformation("SaveUrlFunction HTTP trigger invoked.");

        // Handle Basic Authentication
        if (!Authenticate(req, out string authMessage))
        {
            var unauthorizedResponse = req.CreateResponse(System.Net.HttpStatusCode.Unauthorized);
            await unauthorizedResponse.WriteStringAsync(authMessage);
            return unauthorizedResponse;
        }

        // Read the request body
        string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        var data = JsonSerializer.Deserialize<UrlRequest>(requestBody);

        // Validate the input
        if (string.IsNullOrWhiteSpace(data?.Url))
        {
            var badRequestResponse = req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
            await badRequestResponse.WriteStringAsync("Invalid URL");
            return badRequestResponse;
        }

        logger.LogInformation($"Received URL: {data.Url}");

        // Here you could add code to save the URL in a database or Azure Storage.
        // For now, let's return a successful response.

        var response = req.CreateResponse(System.Net.HttpStatusCode.OK);
        await response.WriteStringAsync("URL successfully saved!");
        return response;
    }

    private bool Authenticate(HttpRequestData req, out string message)
    {
        if (!req.Headers.TryGetValues("Authorization", out var authHeaderValues))
        {
            message = "Authorization header is missing.";
            return false;
        }

        var authHeader = authHeaderValues.ToString();
        if (authHeader.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
        {
            var encodedCredentials = authHeader.Substring("Basic ".Length).Trim();
            var decodedCredentials = Encoding.UTF8.GetString(Convert.FromBase64String(encodedCredentials));
            var credentials = decodedCredentials.Split(':');

            if (credentials.Length == 2 && credentials[0] == Username && credentials[1] == Password)
            {
                message = "Authentication successful.";
                return true;
            }
        }

        message = "Invalid credentials.";
        return false;
    }
}

public class UrlRequest
{
    public string Url { get; set; }
}
