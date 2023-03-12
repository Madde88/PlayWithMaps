namespace PlayWithMaps.Models;
	public class Position
	{
        public Position()
        {
            Long = 0;
            Lat = 0;
        }
        public int Id { get; set; }
        public double Long { get; set; }
        public double Lat { get; set; }
        public int PositionRecId { get; set; }

        public PositionRec? PositionRec { get; set; }

    }


