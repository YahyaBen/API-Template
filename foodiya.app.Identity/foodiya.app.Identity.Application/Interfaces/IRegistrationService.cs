using foodiya.app.Identity.Contracts.Registration;

namespace foodiya.app.Identity.Application.Interfaces;

public interface IRegistrationService
{
    Task<RegistrationChallengeResponse> RegisterAsync(
        RegisterRequest request,
        CancellationToken cancellationToken);

    Task<RegistrationConfirmationResponse> ConfirmAsync(
        ConfirmRegistrationRequest request,
        CancellationToken cancellationToken);

    Task<RegistrationChallengeResponse> ResendAsync(
        ResendRegistrationOtpRequest request,
        CancellationToken cancellationToken);
}
