namespace PlayWithMaps;

public partial class MainPageViewModel
{
    private readonly IPositionRecRepository positionRecRepository;
    private readonly IPositionRepository positionRepository;

    public MainPageViewModel(IPositionRecRepository positionRecRepository, IPositionRepository positionRepository)
    {
        this.positionRecRepository = positionRecRepository;
        this.positionRepository = positionRepository;
    }

    public async Task<PositionRec> SavePositionRec(PositionRec positionRec, bool isReTrace)
    {
        try
        {
            var traceType = isReTrace ? TraceType.ReTrace : TraceType.Trace;
            positionRec.Date = DateTime.UtcNow;
            positionRec.TraceType = traceType;
            return await positionRecRepository.SaveItem(positionRec);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);

            return null;
        }

    }

    public async Task SavePosition(Position position)
    {
        try
        {
            await positionRepository.SaveItem(position);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);

        }

    }

    public async Task<List<PositionRec>> GetAllPositionRecs()
    {
        try
        {
            return await positionRecRepository.GetItems();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return null;
        }
        
    }

    public async Task DeletePositionRecording(int id)
    {
        try
        {
            await positionRecRepository.DeleteItem(id);
        }
        catch (Exception ex)
        {

        }
    }
}


