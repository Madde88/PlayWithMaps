namespace PlayWithMaps.Repositories;

public class PositionRepository : IPositionRepository
{
    private readonly MyDbContext _dbContext;

    public PositionRepository(MyDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task DeleteItem(int id)
    {
        var position = await _dbContext.Position.SingleOrDefaultAsync(x => x.Id == id);
        if (position != null)
        {
            _dbContext.Remove(position);
            await _dbContext.SaveChangesAsync();
        }
    }

    public Task<Position?> GetItem(int id)
    {
        return _dbContext.Position.Include(x => x.PositionRec).FirstOrDefaultAsync(x => x.Id == id);
    }

    public Task<List<Position>> GetItems()
    {
        return _dbContext.Position.ToListAsync();
    }

    public async Task<Position> SaveItem(Position item)
    {
        await _dbContext.Position.AddAsync(item);
        await _dbContext.SaveChangesAsync();
        return item;
    }

    public async Task<List<Position>> SaveItems(List<Position> positions)
    {
        await _dbContext.Position.AddRangeAsync(positions);
        await _dbContext.SaveChangesAsync();
        return positions;
    }

    public Task<Position> UpdateItem(Position item)
    {
        throw new NotImplementedException();
    }
}

public interface IPositionRepository : IBaseRepository<Position>
{

}