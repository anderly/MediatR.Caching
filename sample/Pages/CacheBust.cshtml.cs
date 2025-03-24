using MediatR;
using MediatR.Caching;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Sample.Data;

namespace Sample.Pages
{
    public class CacheBustModel : PageModel
    {
        private readonly IMediator _mediator;

        public CacheBustModel(IMediator mediator) => _mediator = mediator;

        public async Task OnGetAsync() => await _mediator.Send(new Query());

        [InvalidateCache(typeof(CachingWithAttributesModel.Query))]
        //[InvalidateCache(typeof(CachingWithPoliciesModel.Query))]
		public record Query : IRequest<Result>
        {
			public string? Search { get; set; }
			public bool? CacheBust { get; set; } = false;
        }

        public class CacheInvalidationPolicy(IEnumerable<ICachePolicy<CachingWithPoliciesModel.Query, CachingWithPoliciesModel.Result>> cachePolicies)
	        : AbstractCacheInvalidationPolicy<Query, CachingWithPoliciesModel.Query, CachingWithPoliciesModel.Result>(cachePolicies);

		public record Result
        {
            
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
                return new Result
                {

                };
            }
        }
    }
}
