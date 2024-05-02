using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;


namespace AuthProvider.Models;

public class OutputResponse
{
    [ServiceBusOutput("verification_request", Connection = "ServiceBus")]
    public string OutputEvent { get; set; } = null!;

    public HttpResponseData HttpResponse { get; set; } = null!;
}
