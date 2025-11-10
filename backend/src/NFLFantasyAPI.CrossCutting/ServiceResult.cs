namespace NFLFantasyAPI.CrossCutting
{
    public class ServiceResult
    {
        public int StatusCode { get; set; }
        public object Data { get; set; } = null!;

        public static ServiceResult Ok(object data) => new() { StatusCode = 200, Data = data };
        public static ServiceResult BadRequest(string mensaje) => new() { StatusCode = 400, Data = new { mensaje } };
        public static ServiceResult Error(string mensaje) => new() { StatusCode = 500, Data = new { mensaje } };
    }
}
