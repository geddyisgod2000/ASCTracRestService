namespace ASCTracRestService.Filters
{
    internal interface IUserServices
    {
        int Authenticate(string userName, string password);
    }
}