using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Questionnaire.Application.Abstractions.Messaging;
using Questionnaire.Application.Authentication.Commands.Register;
using Questionnaire.Application.Authentication.Queries.Login;
using Questionnaire.Contracts.Authentication;
using Questionnaire.SharedKernel;
using Questionnaire.Api.Common;
using ApplicationAuthenticationResponse = Questionnaire.Application.Authentication.Common.AuthenticationResponse;

namespace Questionnaire.Api.Controllers;

[ApiController]
[Route("auth")]
[AllowAnonymous]
public class AuthenticationController : ApiController
{
    private readonly ISender _sender;

    public AuthenticationController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        RegisterCommand command = new RegisterCommand(request.Login, request.Password, request.Role);
        Result<ApplicationAuthenticationResponse> result = await _sender.Send(command);

        return result.Match(
            authResult => Ok(ApplicationToContractMappers.ToContract(authResult)),
            error => Problem(error));
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        LoginQuery query = new LoginQuery(request.Login, request.Password);
        Result<ApplicationAuthenticationResponse> result = await _sender.Send(query);

        return result.Match(
            authResult => Ok(ApplicationToContractMappers.ToContract(authResult)),
            error => Problem(error));
    }
}