﻿using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using ContosoUniversity.Data;
using ContosoUniversity.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ContosoUniversity.Pages.Students;

public class Delete : PageModel
{
    private readonly IMediator _mediator;

    public Delete(IMediator mediator) => _mediator = mediator;

    [BindProperty]
    public Command Data { get; set; }

    public async Task OnGetAsync(Query query) => Data = await _mediator.Send(query);

    public async Task<IActionResult> OnPostAsync()
    {
        await _mediator.Send(Data);

        return this.RedirectToPageJson(nameof(Index));
    }

    public record Query : IRequest<Command>
    {
        public int Id { get; init; }
    }

    public record Command : IRequest
    {
        public int Id { get; init; }
        [Display(Name = "First Name")]
        public string FirstMidName { get; init; }
        public string LastName { get; init; }
        public DateTime EnrollmentDate { get; init; }
    }

    public class MappingProfile : Profile
    {
        public MappingProfile() => CreateProjection<Student, Command>();
    }

    public class QueryHandler : IRequestHandler<Query, Command>
    {
        private readonly SchoolContext _db;
        private readonly IConfigurationProvider _configuration;

        public QueryHandler(SchoolContext db, IConfigurationProvider configuration)
        {
            _db = db;
            _configuration = configuration;
        }

        public async Task<Command> Handle(Query message, CancellationToken token) => await _db
            .Students
            .Where(s => s.Id == message.Id)
            .ProjectTo<Command>(_configuration)
            .SingleOrDefaultAsync(token);
    }

    public class CommandHandler : IRequestHandler<Command>
    {
        private readonly SchoolContext _db;

        public CommandHandler(SchoolContext db) => _db = db;

        public async Task Handle(Command message, CancellationToken token)
        {
            _db.Students.Remove(await _db.Students.FindAsync(message.Id));
        }
    }

}