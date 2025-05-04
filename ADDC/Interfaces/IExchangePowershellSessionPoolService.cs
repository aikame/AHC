namespace ADDC.Interfaces
{
    public interface IExchangePowershellSessionPoolService
    {
        Task<string> ExecuteFunction(string functionName, params (string Name, object Value)[] parameters);
    }
}
