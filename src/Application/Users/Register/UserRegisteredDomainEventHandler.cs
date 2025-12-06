using Application.Abstractions.Email;
using Domain.UserAggregate.Events;
using SharedKernel;

namespace Application.Users.Register;

internal sealed class UserRegisteredDomainEventHandler(IEmailService emailService) 
    : IDomainEventHandler<UserRegisteredDomainEvent>
{
    public async Task Handle(UserRegisteredDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        string subject = "Welcome!";
        string body = $@"
            <h2>Welcome, {domainEvent.DisplayName}!</h2>
            <p>Your account has been successfully created.</p>
            <p>Login: {domainEvent.Login}</p>
            <p>Thank you for joining us!</p>";

        await emailService.SendAsync(domainEvent.Login, subject, body, cancellationToken);
    }
}


