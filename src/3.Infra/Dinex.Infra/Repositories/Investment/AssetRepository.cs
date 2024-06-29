namespace Dinex.Infra;

public interface IAssetRepository : IRepository<Asset>
{

}

public class AssetRepository : Repository<Asset>, IAssetRepository
{
    public AssetRepository(DinexApiContext context) : base(context)
    {
    }
}
