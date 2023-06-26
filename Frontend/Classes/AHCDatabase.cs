using Frontend.Context;
using Frontend.Interfaces;

namespace Frontend.Classes
{
    public class AHCDatabase : IAHCDatabase
    {
        readonly DataContext _context = new DataContext();
    }
}
