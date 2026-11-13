using BMS.Application.DTOs;

namespace BMS.Application.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDto> Register(RegisterDto dto);
        Task<AuthResponseDto> Login(LoginDto dto);
    }
}
