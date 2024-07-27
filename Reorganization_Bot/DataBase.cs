using System.Data.SqlClient;
using Dapper;

public class DataBase
{
    private string connectionString;
    public void Connection(string server, string name)
        => connectionString = $"Data source={server}; Initial Catalog={name}; Integrated Security=True";

    public async Task<IEnumerable<Job>> GetJobsAsync()
    {
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            await connection.OpenAsync();
            var jobs = await connection.QueryAsync<Job>("SELECT Id, name AS JobTitle, description AS Description FROM Jobs");
            return jobs;
        }
    }

    public async Task InsertApplicationAsync(string name, string link, string job)
    {
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            await connection.OpenAsync();
            var query = "INSERT INTO Applications (name, link, job) VALUES (@Name, @Link, @Job)";
            await connection.ExecuteAsync(query, new { Name = name, Link = link, Job = job });
        }
    }

    //КОМАНДЫ ПОСЛЕ АВТОРИЗАЦИИ
    public async Task<(string Name, string Login, string Role)> GetUserInfoAsync(string login)
    {
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            await connection.OpenAsync();
            var query = "SELECT name, login, role FROM Users WHERE login = @Login";
            var userInfo = await connection.QuerySingleOrDefaultAsync<(string Name, string Login, string Role)>(query, new { Login = login });
            return userInfo;
        }
    }

    public async Task<(string Login, string Role)> CheckUserCredentialsAsync(string login, string password)
    {
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            await connection.OpenAsync();
            var query = "SELECT login, role FROM Users WHERE login = @Login AND password = @Password";
            var user = await connection.QuerySingleOrDefaultAsync<(string Login, string Role)>(query, new { Login = login, Password = password });
            return user;
        }
    }

    public async Task<string> GetLeaderByLoginAsync(string login)
    {
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            await connection.OpenAsync();
            var query = "SELECT name FROM Users WHERE login = @Login";
            var leader = await connection.QuerySingleOrDefaultAsync<string>(query, new { Login = login });
            return leader;
        }
    }

    public async Task InsertOrderAsync(string leader, string name, string order, string description, string link)
    {
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            await connection.OpenAsync();
            var query = "INSERT INTO Orders (leader, name, orders, description, link) VALUES (@Leader, @Name, @Order, @Description, @Link)";
            await connection.ExecuteAsync(query, new { Leader = leader, Name = name.Trim(), Order = order, Description = description, Link = link });
        }
    }

    public async Task<string> GetLinkByNameAsync(string name)
    {
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            await connection.OpenAsync();
            var query = "SELECT link FROM Members WHERE name = @Name";
            var link = await connection.QuerySingleOrDefaultAsync<string>(query, new { Name = name.Trim() });
            return link;
        }
    }

    public async Task<IEnumerable<Order>> GetOrdersByNameAsync(string name)
    {
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            await connection.OpenAsync();
            var query = "SELECT leader, name, orders, description, link FROM Orders WHERE name = @Name";
            var orders = await connection.QueryAsync<Order>(query, new { Name = name.Trim() });
            return orders;
        }
    }

    public async Task<IEnumerable<Member>> GetAllMembersAsync()
    {
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            await connection.OpenAsync();
            var query = "SELECT * FROM Members";
            var members = await connection.QueryAsync<Member>(query);
            return members;
        }
    }

    public class UserInfo
    {
        public string Name { get; set; }
        public string Login { get; set; }
        public string Role { get; set; }
    }

    public class Job
    {
        public int Id { get; set; }
        public string JobTitle { get; set; }
        public string Description { get; set; }
    }

    public class Order
    {
        public string Leader { get; set; }
        public string Name { get; set; }
        public string Orders { get; set; }
        public string Description { get; set; }
        public string Link { get; set; }
    }

    public class Member
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Link { get; set; }
    }
}
