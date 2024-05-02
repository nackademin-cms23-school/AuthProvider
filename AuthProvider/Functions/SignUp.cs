using AuthProvider.Models;
using Grpc.Net.Client;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net;

namespace AuthProvider.Functions;

public class SignUp
{
    private readonly ILogger<SignUp> _logger;
    private readonly GrpcChannel _channel;
    private readonly UserService.UserServiceClient _client;

    public SignUp(ILogger<SignUp> logger)
    {
        _logger = logger;
        _channel = GrpcChannel.ForAddress(Environment.GetEnvironmentVariable("GrpcAddress")!);
        _client = new UserService.UserServiceClient(_channel);
    }

    [Function("SignUp")]
    public async Task<OutputResponse> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req)
    {
        HttpResponseData response;
        try
        {
            var ucr = await UnpackCreateUserRequest(req);
            if (ucr != null)
            {
                var existsRequset = new GetByEmailRequest { Email = ucr.Email };
                var existsResponse = await _client.GetByEmailAsync(existsRequset);
                if (string.IsNullOrEmpty(existsResponse.Message))
                {
                    response = req.CreateResponse(HttpStatusCode.Conflict);
                    await response.WriteStringAsync("User slready exist");
                    return new OutputResponse
                    {
                        HttpResponse = response,
                        OutputEvent = null!
                    };
                }

                var createRequest = new CreateRequest
                {
                    FirstName = ucr.FirstName,
                    LastName = ucr.LastName,
                    Email = ucr.Email,
                    Password = ucr.Password,
                };

                var createResponse = await _client.CreateUserAsync(createRequest);
                if(!string.IsNullOrEmpty(createResponse.Message))
                {
                    var verificationRequest = new VerificationRequest { Email = ucr.Email };

                    response = req.CreateResponse(HttpStatusCode.Created);
                    await response.WriteStringAsync("User was created");

                    return new OutputResponse
                    {
                        HttpResponse = response,
                        OutputEvent =  JsonConvert.SerializeObject(createResponse)
                    };

                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"ERROR : SignUp.Run :: {ex.Message}");
        }
        response = req.CreateResponse(HttpStatusCode.BadRequest);
        await response.WriteStringAsync("Unable to create user");

        return new OutputResponse
        {
            HttpResponse = response,
            OutputEvent = null!
        };
    }

    public async Task<CreateUserRequest> UnpackCreateUserRequest(HttpRequestData req)
    {
        try
        {
            var body = await new StreamReader(req.Body).ReadToEndAsync();
            var request = JsonConvert.DeserializeObject<CreateUserRequest>(body);
            if (request != null)
            {
                return request;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"ERROR : SignUp.UnpackCreateUserRequest :: {ex.Message}");
        }
        return null!;
    }


}
