using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Questionnaire.Application.Authentication.Commands.Register;
using Questionnaire.Application.Authentication.Queries.Login;
using Questionnaire.Contracts.Authentication;

namespace Questionnaire.Api.Controllers;

[ApiController]
[Route("auth")]
[AllowAnonymous]
public class AuthenticationController : ApiController
{
    private readonly ISender _mediator;

    public AuthenticationController(ISender mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        var command = new RegisterCommand(request.Login, request.Password, request.Role);
        var result = await _mediator.Send(command);

        return result.Match(
       authResult => Ok(new AuthenticationResponse(
           authResult.User.Id,
           authResult.User.Login,
           authResult.Token)),
       errors => Problem(errors));
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var query = new LoginQuery(request.Login, request.Password);
        var result = await _mediator.Send(query);

        return result.Match(
        authResult => Ok(new AuthenticationResponse(
            authResult.User.Id,
            authResult.User.Login,
            authResult.Token)),
        errors => Problem(errors));
    }
}