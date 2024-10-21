using Microsoft.EntityFrameworkCore;

namespace Api;

class PeopleDb(DbContextOptions<PeopleDb> options): DbContext(options)
{
    public DbSet<Person> People { get; set; }
}

public class Person
{
    public int Id { get; set; }
    public string Name { get; set; }
}