using System.Threading;
using System.Threading.Tasks;
using ContosoUniversity.Data;
using ContosoUniversity.Models;
using MediatR;
using MediatR.Caching;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ContosoUniversity.Pages.Courses;

public class Create : PageModel
{
    private readonly IMediator _mediator;

    public Create(IMediator mediator) => _mediator = mediator;

    [BindProperty]
    public Command Data { get; set; }

    public async Task<IActionResult> OnPostAsync()
    {
        await _mediator.Send(Data);

        return this.RedirectToPageJson("Index");
    }

    [InvalidateCache(typeof(Index.Query))]
	public record Command : IRequest<int>
    {
        public int Number { get; init; }
        public string Title { get; init; }
        public int Credits { get; init; }
        public Department Department { get; init; }
    }

    public class CommandHandler : IRequestHandler<Command, int>
    {
        private readonly SchoolContext _db;

        public CommandHandler(SchoolContext db) => _db = db;

        public async Task<int> Handle(Command message, CancellationToken token)
        {
            var course = new Course
            {
                Id = message.Number,
                Credits = message.Credits,
                Department = message.Department,
                Title = message.Title
            };

            await _db.Courses.AddAsync(course, token);

            await _db.SaveChangesAsync(token);

            return course.Id;
        }
    }
}