using BCrypt.Net;

using KnowLedger_Synaptix.Dtos;

using KnowLedger_Synaptix.Models;

using KnowLedger_Synaptix.Services.Interfaces;

using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

using System;

using System.Collections.Generic;

using System.IdentityModel.Tokens.Jwt;

using System.Linq;

using System.Security.Claims;

using System.Text;

namespace KnowLedger_Synaptix.Services.Implementations

{
    public class AuthService : IAuthService

    {

        private readonly Knowledge_Repository_dbContext _context;

        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;

        {

            _context = context;

            _configuration = configuration;
        }
        //Registration
        public async Task<bool> RegisterAsync(RegisterDto dto, Guid? createdBy = null)

        /// <summary>
        /// Registers a new user, hashes the password, assigns the default role, 
        /// and sets audit fields. Optionally allows creation by an admin.
        /// </summary>
        /// <param name="dto">Registration details</param>
        /// <param name="createdBy">Optional admin user who creates this account</param>
        /// <returns>True if registration succeeds, false if email already exists</returns>
        public async Task<bool> RegisterAsync(RegisterDto dto, Guid? createdBy = null)
        {

            // Check if a user with the same email already exists
            if (await _context.Users.AnyAsync(u => u.Email.ToLower() == dto.Email.ToLower()))
                return false;

            // Retrieve department by name (optional)
            var department = await _context.Departments

                .FirstOrDefaultAsync(d => d.DepartmentName.ToLower() == dto.DepartmentName.ToLower());

            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(dto.Password);

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

            if (createdBy == null)

            {

                user.CreatedBy = user.UserId;

                user.UpdatedBy = user.UserId;

                _context.Users.Update(user);

                await _context.SaveChangesAsync();

            }

            var contributorRole = await _context.Roles
                .FirstOrDefaultAsync(r => EF.Functions.ILike(r.RoleName, "contributor"));

            if (contributorRole == null)
            {
                throw new InvalidOperationException("Default role 'Contributor' not found in Roles table.");
            }

            {
                UserId = user.UserId,
                RoleId = contributorRole.RoleId,
                CreatedOn = DateTime.UtcNow,
                UpdatedOn = DateTime.UtcNow,
                CreatedBy = createdBy ?? user.UserId,
                UpdatedBy = createdBy ?? user.UserId

            _context.UserRoles.Add(userRole);


            await _context.SaveChangesAsync();

            _logger.LogInformation("User registered successfully: {Email}", dto.Email);
            return true;
        }
        public async Task<AuthResponseDto?> LoginAsync(LoginDto dto)

        {
            _logger.LogInformation("Login attempt for user: {Email}", dto.Email);

            // Retrieve user and roles by email
            var user = await _context.Users

                .Include(u => u.UserRoleUsers)

                    .ThenInclude(ur => ur.Role)

                .FirstOrDefaultAsync(u => u.Email.ToLower() == dto.Email.ToLower());

                return null;
            }



            // Prepare JWT token
            var secretKey = _configuration["JwtSettings:SecretKey"];

            if (string.IsNullOrEmpty(secretKey))

                throw new InvalidOperationException("JWT SecretKey is missing in configuration.");

            var key = Encoding.UTF8.GetBytes(secretKey);

            // Prepare claims including user roles
            var roles = user.UserRoleUsers.Select(ur => ur.Role.RoleName).ToList();
            var claims = new List<Claim>

            {

                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),

                new Claim(ClaimTypes.Email, user.Email),

                new Claim(ClaimTypes.Name, user.Name)

            };

            // Create token descriptor
            var tokenDescriptor = new SecurityTokenDescriptor

            {

                Subject = new ClaimsIdentity(claims),

                Expires = DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["JwtSettings:ExpiresInMinutes"] ?? "60")),

                Issuer = _configuration["JwtSettings:Issuer"],

                Audience = _configuration["JwtSettings:Audience"],

                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)

            };

            // Generate JWT token
            var tokenHandler = new JwtSecurityTokenHandler();

            var token = tokenHandler.CreateToken(tokenDescriptor);

            // Return authentication response
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
