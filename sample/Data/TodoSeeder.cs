namespace Sample.Data
{
	public class TodoSeeder
	{
		private AppDbContext _db;

		public TodoSeeder(AppDbContext db)
		{
			_db = db;
		}

		public void SeedData()
		{
			_db.Todos.AddRange(GetTestData());
			_db.SaveChanges();
		}

		private List<Todo> GetTestData()
		{
			//return new List<Todo>() {new Todo()
			//{
			//	Id = 1,
			//	Name = "Todo 1"
			//}};
			return Enumerable.Range(1, 5).Select(t => new Todo
			{
				Id = t,
				Name = $"Todo {t}",
				IsComplete = false
			}).ToList();
		}
	}
}
