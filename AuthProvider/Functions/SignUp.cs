using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace AuthProvider.Functions;

public class SignUp
{
    private readonly ILogger<SignUp> _logger;

    public SignUp(ILogger<SignUp> logger)
    {
        _logger = logger;
    }

    [Function("SignUp")]
    public IActionResult Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest req)
    {
        try
        {

        }
        catch (Exception ex)
        {
            _logger.LogError($"ERROR : SignUp.Run :: {ex.Message}");
        }
        return new BadRequestResult();
    }

    public async Task<CreateRequest>
}
