namespace Dinex.AppService;

internal class GetUserQueryHandler : IQueryHandler, IRequestHandler<GetUserQuery, OperationResult<UserDTO>>
{
    private readonly IUserRepository _userRepository;

    public GetUserQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<OperationResult<UserDTO>> Handle(GetUserQuery request, CancellationToken cancellationToken)
    {
        var result = new OperationResult<UserDTO>();

        var user = await _userRepository.GetByIdAsync(request.UserId);
        if (user == null)
        {
            result.AddError("usuário não encontrado").SetAsNotFound();
            return result;
        }

        result.SetData(new UserDTO
        {
            Id = user.Id,
            Email = user.Email,
            FullName = user.FullName,
        });
        return result;
    }

}
