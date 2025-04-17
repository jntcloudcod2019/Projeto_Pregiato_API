namespace Pregiato.API.Response
{
    public class UsersResponse
    {

        public bool SUCESS { get; set; }
        public string MESSAGE { get; set; }
        public List<ResultUsersResponse>?  DATA { get; set; }
    }

    public class ResultUsersResponse
    {
        public string ID { get; set; }
        public string? NAME { get; set; }
        public string EMAIL { get; set; }
        public string? POSITION { get; set; }
   
    }
}
