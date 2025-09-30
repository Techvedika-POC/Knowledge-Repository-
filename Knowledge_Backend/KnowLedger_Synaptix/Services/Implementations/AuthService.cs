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

        public AuthService(Knowledge_Repository_dbContext context, IConfiguration configuration)

        {

            _context = context;

            _configuration = configuration;

        }

        public async Task<bool> RegisterAsync(RegisterDto dto, Guid? createdBy = null)

        {

            // 1. Check if email already exists

            if (await _context.Users.AnyAsync(u => u.Email.ToLower() == dto.Email.ToLower()))

                return false;

            // 2. Find the department

            var department = await _context.Departments

                .FirstOrDefaultAsync(d => d.DepartmentName.ToLower() == dto.DepartmentName.ToLower());

            // 3. Hash password

            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            // 4. Create User

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

            // 5. If self-registration, set CreatedBy/UpdatedBy to self

            if (createdBy == null)

            {

                user.CreatedBy = user.UserId;

                user.UpdatedBy = user.UserId;

                _context.Users.Update(user);

                await _context.SaveChangesAsync();

            }

            // 6. Assign default role "Contributor"

            var contributorRole = await _context.Roles
                .FirstOrDefaultAsync(r => EF.Functions.ILike(r.RoleName, "contributor"));

            if (contributorRole == null)
            {
                throw new InvalidOperationException("Default role 'Contributor' not found in Roles table.");
            }

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

            return true;
       }

        public async Task<AuthResponseDto?> LoginAsync(LoginDto dto)

        {

            var user = await _context.Users

                .Include(u => u.UserRoleUsers)

                    .ThenInclude(ur => ur.Role)

                .FirstOrDefaultAsync(u => u.Email.ToLower() == dto.Email.ToLower());

            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))

                return null;

            // Collect role names (already strings in DB)
            var roles = user.UserRoleUsers
                .Select(ur => ur.Role.RoleName)


                .ToList();

            // Read JWT secret safely

            var secretKey = _configuration["JwtSettings:SecretKey"];

            if (string.IsNullOrEmpty(secretKey))

                throw new InvalidOperationException("JWT SecretKey is missing in configuration.");

            var key = Encoding.UTF8.GetBytes(secretKey);

            var claims = new List<Claim>

            {

                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),

                new Claim(ClaimTypes.Email, user.Email),

                new Claim(ClaimTypes.Name, user.Name)

            };

            foreach (var role in roles)

                claims.Add(new Claim(ClaimTypes.Role, role));

            var tokenDescriptor = new SecurityTokenDescriptor

            {

                Subject = new ClaimsIdentity(claims),

                Expires = DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["JwtSettings:ExpiresInMinutes"] ?? "60")),

                Issuer = _configuration["JwtSettings:Issuer"],

                Audience = _configuration["JwtSettings:Audience"],

                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)

            };

            var tokenHandler = new JwtSecurityTokenHandler();

            var token = tokenHandler.CreateToken(tokenDescriptor);

            return new AuthResponseDto

            {

                Token = tokenHandler.WriteToken(token),

                Name = user.Name,

                Email = user.Email,

                Roles = roles

            };

        }

    }

}