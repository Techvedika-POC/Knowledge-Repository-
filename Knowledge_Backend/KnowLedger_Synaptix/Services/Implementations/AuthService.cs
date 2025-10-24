using BCrypt.Net;
using KnowLedger_Synaptix.Dtos;
using KnowLedger_Synaptix.Models;
using KnowLedger_Synaptix.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace KnowLedger_Synaptix.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly Knowledge_Repository_dbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;

        // constructor
        public AuthService(Knowledge_Repository_dbContext context, IConfiguration configuration, ILogger<AuthService> logger)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// Registers a new user, hashes the password, assigns the default "Contributor" role, 
        /// and sets audit fields. Optionally allows creation by an admin.
        /// </summary>
        public async Task<bool> RegisterAsync(RegisterDto dto, Guid? createdBy )
        {
            // Check if a user with the same email already exists
            if (await _context.Users.AnyAsync(u => u.Email.ToLower() == dto.Email.ToLower()))
                return false;

            // Retrieve department by name (optional)
            var department = await _context.Departments
                .FirstOrDefaultAsync(d => d.DepartmentName.ToLower() == dto.DepartmentName.ToLower());

            // Hash the password
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            // Create user
            var user = new User
            {
                Name = dto.Name,
                Email = dto.Email,
                PasswordHash = hashedPassword,
                DepartmentId = department?.DepartmentId,
                CreatedOn = DateTime.UtcNow,
                UpdatedOn = DateTime.UtcNow,
                CreatedBy = createdBy,
                UpdatedBy = createdBy
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // If user self-registers, set createdBy to their own ID
            if (createdBy == null)
            {
                user.CreatedBy = user.UserId;
                user.UpdatedBy = user.UserId;
                _context.Users.Update(user);
                await _context.SaveChangesAsync();
            }

            // Assign default role: "Contributor"
            var contributorRole = await _context.Roles
                .FirstOrDefaultAsync(r => EF.Functions.ILike(r.RoleName, "contributor"));

            if (contributorRole == null)
                throw new InvalidOperationException("Default role 'Contributor' not found in Roles table.");

            var userRole = new UserRole
            {
                UserId = user.UserId,
                RoleId = contributorRole.RoleId,
                CreatedOn = DateTime.UtcNow,
                UpdatedOn = DateTime.UtcNow,
                CreatedBy = createdBy ?? user.UserId,
                UpdatedBy = createdBy ?? user.UserId
            };

            _context.UserRoles.Add(userRole);
            await _context.SaveChangesAsync();

            _logger.LogInformation("User registered successfully: {Email}", dto.Email);
            return true;
        }

        /// <summary>
        /// Logs in a user and returns a JWT token.
        /// </summary>
        public async Task<AuthResponseDto?> LoginAsync(LoginDto dto)
        {
            _logger.LogInformation("Login attempt for user: {Email}", dto.Email);

            // Find user and include roles
            var user = await _context.Users
                .Include(u => u.UserRoleUsers)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Email.ToLower() == dto.Email.ToLower());

            if (user == null)
            {
                _logger.LogWarning("Login failed: user not found for {Email}", dto.Email);
                return null;
            }

            // Verify password
            if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            {
                _logger.LogWarning("Login failed: invalid password for {Email}", dto.Email);
                return null;
            }

            // Get JWT secret
            var secretKey = _configuration["JwtSettings:SecretKey"];
            if (string.IsNullOrEmpty(secretKey))
                throw new InvalidOperationException("JWT SecretKey is missing in configuration.");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

            // Claims
            var roles = user.UserRoleUsers.Select(ur => ur.Role.RoleName).ToList();
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.Name)
            };

            // Add roles to claims
            foreach (var role in roles)
                claims.Add(new Claim(ClaimTypes.Role, role));

            // Create token
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(
                    Convert.ToDouble(_configuration["JwtSettings:ExpiresInMinutes"] ?? "60")),
                Issuer = _configuration["JwtSettings:Issuer"],
                Audience = _configuration["JwtSettings:Audience"],
                SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return new AuthResponseDto
            {
                Token = tokenHandler.WriteToken(token),
                Name = user.Name,
                Email = user.Email,
                Roles = roles,
                UserId = user.UserId
            };
        }
    }
}
