namespace Dinex.Infra;

public interface IUserRepository : IRepository<User>
{

}

public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(DinexApiContext context) : base(context)
    {
    }
}
