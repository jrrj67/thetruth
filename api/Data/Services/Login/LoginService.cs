﻿using AutoMapper;
using JogandoBack.API.Data.Models.Entities;
using JogandoBack.API.Data.Models.Requests;
using JogandoBack.API.Data.Models.Responses;
using JogandoBack.API.Data.Repositories.Users;
using JogandoBack.API.Data.Services.PasswordHasher;
using JogandoBack.API.Data.Services.RefreshTokensEntityService;
using JogandoBack.API.Data.Services.Token;
using System.Threading.Tasks;

namespace JogandoBack.API.Data.Services.Login
{
    public class LoginService : ILoginService<LoginResponse, LoginRequest>
    {
        private readonly IUsersRepository _repository;
        private readonly IMapper _mapper;
        private readonly ITokenService _tokenService;
        private readonly IRefreshTokenService _refreshTokenService;
        private readonly IRefreshTokensEntityService<RefreshTokensResponse, RefreshTokensRequest> _refreshTokenEntityService;
        private readonly IPasswordHasher _passwordHasher;

        public LoginService(IUsersRepository repository, IMapper mapper, ITokenService tokenService, IRefreshTokenService refreshTokenService,
            IPasswordHasher passwordHasher, IRefreshTokensEntityService<RefreshTokensResponse, RefreshTokensRequest> refreshTokenEntityService)
        {
            _repository = repository;
            _mapper = mapper;
            _tokenService = tokenService;
            _passwordHasher = passwordHasher;
            _refreshTokenService = refreshTokenService;
            _refreshTokenEntityService = refreshTokenEntityService;
        }

        public async Task<LoginResponse> Login(LoginRequest loginRequest)
        {
            var userEntity = _repository.GetUserByEmail(loginRequest.Email);

            return await Authenticate(userEntity);
        }

        public async Task<LoginResponse> Authenticate(UsersEntity userEntity)
        {
            var token = _tokenService.GenerateToken(userEntity);

            var refreshToken = _refreshTokenService.GenerateToken();

            var userResponse = _mapper.Map<UsersResponse>(userEntity);

            // Refresh token
            var refreshTokenRequest = new RefreshTokensRequest()
            {
                Token = refreshToken,
                UserId = userEntity.Id
            };

            var refreshTokenResponse = _refreshTokenEntityService.GetByUserId(userEntity.Id);

            if (refreshTokenResponse != null)
            {
                await _refreshTokenEntityService.DeleteAsync(refreshTokenResponse.Id);
            }

            await _refreshTokenEntityService.SaveAsync(refreshTokenRequest);

            return new LoginResponse
            {
                User = userResponse,
                Token = token,
                RefreshToken = refreshToken
            };
        }

        public bool VerifyEmailAndPassword(string email, string password)
        {
            var user = _repository.GetUserByEmail(email);

            if (user == null)
            {
                return false;
            }

            if (!_passwordHasher.Verify(password, user.Password))
            {
                return false;
            }

            return true;
        }
    }
}
