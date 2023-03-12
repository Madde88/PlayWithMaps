namespace PlayWithMaps.Models;

	public class PositionRec
	{
        public PositionRec()
        {
            Positions = new ObservableCollection<Position>();
            Name = String.Empty;
            Date = DateTime.UtcNow;
        }
        public int Id { get; set; }
        public string Name { get; set; }
        public TraceType TraceType { get; set; }
        public DateTime Date { get; set; }

        public ObservableCollection<Position> Positions { get; set; }
    }


