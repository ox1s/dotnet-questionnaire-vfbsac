using Questionnaire.Domain.Entities;

namespace Questionnaire.Application.Common.Interfaces;

public interface IJwtTokenGenerator
{
    string GenerateToken(User user);
}