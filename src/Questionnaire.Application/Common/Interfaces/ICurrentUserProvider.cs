using System.Security.Claims;

namespace Questionnaire.Application.Common.Interfaces;

public interface ICurrentUserProvider
{
    int UserId { get; }
    string[] Roles { get; }
}