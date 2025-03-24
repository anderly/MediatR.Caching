using MediatR;
using MediatR.Caching;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Sample.Data;
using DateTime = System.DateTime;

namespace Sample.Pages
{
    public class CachingWithPoliciesModel : PageModel
    {
        private readonly IMediator _mediator;

        public CachingWithPoliciesModel(IMediator mediator) => _mediator = mediator;

        public Result Data { get; private set; }

        public async Task OnGetAsync() => Data = await _mediator.Send(new Query());

        public record Query : IRequest<Result>
        {
	        [FromQuery] public string? Search { get; set; }
			[FromQuery] public bool? CacheBust { get; set; } = false;
            //public string GetCacheKey(Query request)
            //{
	           // return "Query.CustomCacheKey";
            //}
        }

        public class CachePolicy : ICachePolicy<Query, Result>
        {
			// Optionally, change defaults
			//public TimeSpan? AbsoluteExpirationRelativeToNow => TimeSpan.FromMinutes(1);
			public bool AutoReload => true;
			//public string GetCacheKey(Query request)
			//{
			//    return "Query.CustomCacheKey";
			//}
        }

        public record Result
        {
            public List<Todo> Todos { get; init; }

            public record Todo
            {
                public int Id { get; init; }
                public string Name { get; init; }
                public bool IsComplete { get; init; }
                public DateTime RetrievedAt { get; init; }
            }
        }

        public class QueryHandler : IRequestHandler<Query, Result>
        {
            private readonly AppDbContext _db;

            public QueryHandler(AppDbContext db)
            {
                _db = db;
            }

            public async Task<Result> Handle(Query message, CancellationToken token)
            {
                var todos = await _db.Todos
                    .OrderBy(d => d.Id)
                    .Select(t => new Result.Todo
                    {
                        Id = t.Id,
                        Name = t.Name,
                        IsComplete = t.IsComplete,
                        RetrievedAt = DateTime.Now
                    })
                    .ToListAsync(token);

                return new Result
                {
                    Todos = todos
                };
            }
        }
    }
}
