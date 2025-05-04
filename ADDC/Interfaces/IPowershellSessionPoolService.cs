namespace ADDC.Interfaces
{
    public interface IPowershellSessionPoolService
    {
        Task<string> ExecuteFunction(string functionName, params (string, object)[]? parameters);
    }
}
