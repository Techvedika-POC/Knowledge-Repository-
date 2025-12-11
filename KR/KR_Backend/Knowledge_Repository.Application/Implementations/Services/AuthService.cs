using Knowledge_Repository.Application.Dtos;
using Knowledge_Repository.Application.Interfaces.Repositories;
using Knowledge_Repository.Application.Interfaces.Services;
using Knowledge_Repository.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using BCryptNet = BCrypt.Net.BCrypt;

namespace Knowledge_Repository.Application.Implementations.Services
{

    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IDepartmentRepository _departmentRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IUserRoleRepository _userRoleRepository;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;
        private readonly IEventJuryRepository _eventJuryRepo;
        public AuthService(
            IUserRepository userRepository,
            IDepartmentRepository departmentRepository,
            IRoleRepository roleRepository,
            IUserRoleRepository userRoleRepository,
            IConfiguration configuration,
            
            ILogger<AuthService> logger,
            IEventJuryRepository eventJuryRepo)

        {
            _userRepository = userRepository;
            _departmentRepository = departmentRepository;
            _roleRepository = roleRepository;
            _userRoleRepository = userRoleRepository;
            _configuration = configuration;
            _logger = logger;
            _eventJuryRepo = eventJuryRepo;
        }


        public async Task<bool> RegisterAsync(RegisterDto dto, Guid? createdBy = null)
        {
    
            if (await _userRepository.ExistsByEmailAsync(dto.Email))
            {
                _logger.LogWarning("Registration failed: Email already exists ({Email})", dto.Email);
                return false;
            }

            var department = await _departmentRepository.GetByNameAsync(dto.DepartmentName);
            var hashedPassword = BCryptNet.HashPassword(dto.Password);

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

            await _userRepository.AddAsync(user);

            if (createdBy == null)
            {
                user.CreatedBy = user.UserId;
                user.UpdatedBy = user.UserId;
                await _userRepository.UpdateAsync(user);
            }

            var contributorRole = await _roleRepository.GetByNameAsync("Contributor");
            if (contributorRole == null)
                throw new InvalidOperationException("Default role 'Contributor' not found.");

            var userRole = new UserRole
            {
                UserId = user.UserId,
                RoleId = contributorRole.RoleId,
                CreatedOn = DateTime.UtcNow,
                UpdatedOn = DateTime.UtcNow,
                CreatedBy = createdBy ?? user.UserId,
                UpdatedBy = createdBy ?? user.UserId
            };

            await _userRoleRepository.AddAsync(userRole);

            _logger.LogInformation("User registered successfully: {Email}", dto.Email);
            return true;
        }

        public async Task<AuthResponseDto?> LoginAsync(LoginDto dto)
        {
            _logger.LogInformation("Login attempt for user: {Email}", dto.Email);

            var user = await _userRepository.GetUserWithRolesByEmailAsync(dto.Email);
            if (user == null || !BCryptNet.Verify(dto.Password, user.PasswordHash))
            {
                _logger.LogWarning("Login failed for user: {Email}", dto.Email);
                return null;
            }

  
            var eventIds = await _eventJuryRepo.GetEventIdsForUserAsync(user.UserId);

            var secretKey = _configuration["JwtSettings:SecretKey"];
            var issuer = _configuration["JwtSettings:Issuer"];
            var audience = _configuration["JwtSettings:Audience"];
            var expiresMinutes = int.Parse(_configuration["JwtSettings:ExpiresInMinutes"]);

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
    {
        new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
        new Claim(JwtRegisteredClaimNames.Email, user.Email),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        new Claim(ClaimTypes.Name, user.Name)
    };

        
            foreach (var role in user.UserRoleUsers.Select(ur => ur.Role.RoleName))
                claims.Add(new Claim(ClaimTypes.Role, role));

      
            foreach (var eid in eventIds)
                claims.Add(new Claim("event_jury", eid.ToString()));

        
            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expiresMinutes),
                signingCredentials: creds
            );

 
            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            _logger.LogInformation("Login successful for user: {Email}", dto.Email);

            return new AuthResponseDto
            {
                Token = tokenString,
                Name = user.Name,
                Email = user.Email,
                Roles = user.UserRoleUsers.Select(ur => ur.Role.RoleName).ToList(),
                UserId = user.UserId,
                EventJuryIds = eventIds
            };
        }

    }
}
