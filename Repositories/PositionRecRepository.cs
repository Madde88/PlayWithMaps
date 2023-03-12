namespace PlayWithMaps.Repositories;

public class PositionRecRepository : IPositionRecRepository
{
    private readonly MyDbContext _dbContext;

    public PositionRecRepository(MyDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task DeleteItem(int id)
    {
        var column = await _dbContext.PositionRec.SingleOrDefaultAsync(x => x.Id == id);
        if (column != null)
        {
            _dbContext.Remove(column);
            await _dbContext.SaveChangesAsync();
        }
    }

    public Task<PositionRec?> GetItem(int id)
    {
        return _dbContext.PositionRec.Include(x => x.Positions).FirstOrDefaultAsync(x => x.Id == id);
    }

    public Task<List<PositionRec>> GetItems()
    {
        return _dbContext.PositionRec.Include(x => x.Positions).ToListAsync();
    }

    public async Task<PositionRec?> UpdateItem(PositionRec item)
    {
        var column = await _dbContext.PositionRec.SingleOrDefaultAsync(x => x.Id == item.Id);
        if (column == null)
        {
            return column;
        }

        column.Name = item.Name;

        await _dbContext.SaveChangesAsync();
        return column;
    }

    public async Task<PositionRec> SaveItem(PositionRec item)
    {
        await _dbContext.PositionRec.AddAsync(item);
        await _dbContext.SaveChangesAsync();
        return item;
    }
}

public interface IPositionRecRepository : IBaseRepository<PositionRec>
{ }