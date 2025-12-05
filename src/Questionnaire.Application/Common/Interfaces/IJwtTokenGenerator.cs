using Questionnaire.Application.Common;

namespace Questionnaire.Application.Common.Interfaces;

public interface IJwtTokenGenerator
{
    string GenerateToken(UserTokenData user);
}