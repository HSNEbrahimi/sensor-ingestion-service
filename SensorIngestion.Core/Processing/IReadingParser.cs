using SensorIngestion.Core.Entities;

namespace SensorIngestion.Core.Processing;

public interface IReadingParser
{
    Reading Parse(string line);
}

public class InvalidReadingException : Exception
{
    public InvalidReadingException(string message) : base(message) { }
}