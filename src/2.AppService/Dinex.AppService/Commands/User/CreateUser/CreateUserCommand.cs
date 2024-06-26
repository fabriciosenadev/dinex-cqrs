﻿namespace Dinex.AppService;

public class CreateUserCommand : IRequest<OperationResult<Guid>>
{
    public string FullName { get; set; }
    public required string Email { get; set; }
    public required string Password { get; set; }
    public required string ConfirmPassword { get; set; }
}
