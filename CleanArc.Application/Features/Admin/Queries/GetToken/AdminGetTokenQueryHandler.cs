﻿using CleanArc.Application.Contracts;
using CleanArc.Application.Contracts.Identity;
using CleanArc.Application.Models.Common;
using CleanArc.Application.Models.Jwt;
using Mediator;

namespace CleanArc.Application.Features.Admin.Queries.GetToken;

public class AdminGetTokenQueryHandler:IRequestHandler<AdminGetTokenQuery,OperationResult<AccessToken>>
{
    private readonly IAppUserManager _userManager;
    private readonly IJwtService _jwtService;
    public AdminGetTokenQueryHandler(IAppUserManager userManager, IJwtService jwtService)
    {
        _userManager = userManager;
        _jwtService = jwtService;
    }

    public async ValueTask<OperationResult<AccessToken>> Handle(AdminGetTokenQuery request, CancellationToken cancellationToken)
    {
        var user = await _userManager.GetByUserName(request.UserName);

        if(user is null)
            return OperationResult<AccessToken>.FailureResult("User not found");

        var passwordValidator = await _userManager.AdminLogin(user, request.Password);

        if (passwordValidator.IsLockedOut)
            if (user.LockoutEnd != null)
                return OperationResult<AccessToken>.FailureResult(
                    $"User is locked out. Try in {(DateTimeOffset.Now - user.LockoutEnd).Value.Minutes} Minutes");

        if (!passwordValidator.Succeeded)
            return OperationResult<AccessToken>.FailureResult("Password is not correct");

        var token= await _jwtService.GenerateAsync(user);


        return OperationResult<AccessToken>.SuccessResult(token);
    }
}