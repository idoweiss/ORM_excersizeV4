namespace ORM.Models
{
    public class User : IBaseModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        //can also do =" ";

        public string Username { get; set; } = string.Empty;
        //can also do =" ";

        public string Password { get; set; } = string.Empty;
        //can also do =" ";

        public string Role { get; set; } = string.Empty;
        //can also do =" ";

        public string Email { get; set; } = string.Empty;
        //can also do =" ";

        public int Age { get; set; }

    }

}