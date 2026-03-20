using BankManagementSystem.Data.Repositories;
using BankManagementSystem.DTOs;
using BankManagementSystem.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace BankManagementSystem.Services
{
    public interface IAuthService
    {
        Task<(bool Success, string Message)> RegisterAsync(RegisterDto dto);
        Task<(bool Success, string Message, string Role)> LoginAsync(LoginDto dto, HttpContext httpContext);
        Task LogoutAsync(HttpContext httpContext);
        Task<bool> VerifyPasswordAsync(int userId, string password);
    }

    public class AuthService : IAuthService
    {
        private readonly IUserRepository _users;
        private readonly ICustomerRepository _customers;

        public AuthService(IUserRepository users, ICustomerRepository customers)
        {
            _users = users;
            _customers = customers;
        }

        public async Task<(bool Success, string Message)> RegisterAsync(RegisterDto dto)
        {
            var existing = await _users.GetByEmailAsync(dto.Email);
            if (existing != null) return (false, "Email already registered.");

            var user = new User
            {
                FullName = dto.FullName,
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Role = "Customer"
            };
            await _users.AddAsync(user);
            await _users.SaveAsync();

            var customer = new Customer
            {
                UserId = user.Id,
                Phone = dto.Phone,
                DateOfBirth = dto.DateOfBirth,
                Gender = dto.Gender,
                Status = CustomerStatus.Pending
            };
            await _customers.AddAsync(customer);
            await _customers.SaveAsync();

            return (true, "Registration successful. Please wait for admin approval before logging in.");
        }

        public async Task<(bool Success, string Message, string Role)> LoginAsync(LoginDto dto, HttpContext httpContext)
        {
            var user = await _users.GetByEmailAsync(dto.Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                return (false, "Invalid email or password.", "");

            // FIX: Blocked users cannot log in
            if (!user.IsActive)
                return (false, "Your account has been blocked. Please contact the bank.", "");

            if (user.Role == "Customer")
            {
                var customer = await _customers.GetByUserIdAsync(user.Id);
                if (customer == null) return (false, "Customer record not found.", "");
                if (customer.Status == CustomerStatus.Pending)
                    return (false, "Your registration is pending admin approval.", "");
                if (customer.Status == CustomerStatus.Rejected)
                    return (false, "Your registration was rejected. Please contact the bank.", "");
                if (customer.Status == CustomerStatus.Blocked)
                    return (false, "Your account has been blocked. Please contact the bank.", "");
            }

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Name, user.FullName),
                new(ClaimTypes.Email, user.Email),
                new(ClaimTypes.Role, user.Role)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);
            await httpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            return (true, "Login successful.", user.Role);
        }

        public async Task LogoutAsync(HttpContext httpContext) =>
            await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        public async Task<bool> VerifyPasswordAsync(int userId, string password)
        {
            var user = await _users.GetByIdAsync(userId);
            return user != null && BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
        }
    }
}
