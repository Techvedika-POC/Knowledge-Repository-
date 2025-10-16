using BCrypt.Net;
using KnowLedger_Synaptix.Dtos;
using KnowLedger_Synaptix.Models;
using KnowLedger_Synaptix.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace KnowLedger_Synaptix.Services.Implementations
{
    /// <summary>
    /// Handles user authentication and registration, including password hashing,
    /// JWT generation, and role assignment.
    /// </summary>
    public class AuthService : IAuthService
    {
        private readonly Knowledge_Repository_dbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;

        public AuthService(Knowledge_Repository_dbContext context, IConfiguration configuration, ILogger<AuthService> logger)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// Registers a new user, hashes the password, assigns the default role, 
        /// and sets audit fields. Optionally allows creation by an admin.
        /// </summary>
        /// <param name="dto">Registration details</param>
        /// <param name="createdBy">Optional admin user who creates this account</param>
        /// <returns>True if registration succeeds, false if email already exists</returns>
        public async Task<bool> RegisterAsync(RegisterDto dto, Guid? createdBy = null)
        {
            _logger.LogInformation("Registering user: {Email}", dto.Email);

            // Check if a user with the same email already exists
            if (await _context.Users.AnyAsync(u => u.Email.ToLower() == dto.Email.ToLower()))
            {
                _logger.LogWarning("Registration failed: Email already exists - {Email}", dto.Email);
                return false;
            }

            // Retrieve department by name (optional)
            var department = await _context.Departments
                .FirstOrDefaultAsync(d => d.DepartmentName.ToLower() == dto.DepartmentName.ToLower());

            // Hash the user's password
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            // Create the user entity
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

            // If self-registration, set CreatedBy/UpdatedBy to user's own ID
            if (createdBy == null)
            {
                user.CreatedBy = user.UserId;
                user.UpdatedBy = user.UserId;
                _context.Users.Update(user);
                await _context.SaveChangesAsync();
            }

            // Assign default role "Contributor"
            var contributorRole = await _context.Roles
                .FirstOrDefaultAsync(r => EF.Functions.ILike(r.RoleName, "contributor"));

            if (contributorRole == null)
                throw new InvalidOperationException("Default role 'Contributor' not found in Roles table.");

            _context.UserRoles.Add(new UserRole
            {
                UserId = user.UserId,
                RoleId = contributorRole.RoleId,
                CreatedOn = DateTime.UtcNow,
                UpdatedOn = DateTime.UtcNow,
                CreatedBy = createdBy ?? user.UserId,
                UpdatedBy = createdBy ?? user.UserId
            });

            await _context.SaveChangesAsync();

            _logger.LogInformation("User registered successfully: {Email}", dto.Email);
            return true;
        }

        /// <summary>
        /// Authenticates a user, validates password, and generates a JWT token
        /// containing claims and roles.
        /// </summary>
        /// <param name="dto">Login credentials</param>
        /// <returns>AuthResponseDto containing JWT and user info, or null if authentication fails</returns>
        public async Task<AuthResponseDto?> LoginAsync(LoginDto dto)
        {
            _logger.LogInformation("Login attempt for user: {Email}", dto.Email);

            // Retrieve user and roles by email
            var user = await _context.Users
                .Include(u => u.UserRoleUsers)
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Email.ToLower() == dto.Email.ToLower());

            if (user == null)
            {
                _logger.LogWarning("Login failed: User not found - {Email}", dto.Email);
                return null;
            }

            // Verify password
            if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            {
                _logger.LogWarning("Login failed: Incorrect password - {Email}", dto.Email);
                return null;
            }

            _logger.LogInformation("Login successful for user: {Email}", dto.Email);

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
            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

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
