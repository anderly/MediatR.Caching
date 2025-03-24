using System;
using System.Threading.Tasks;
using ContosoUniversity.Models;
using Shouldly;
using Xunit;

namespace ContosoUniversity.IntegrationTests.Pages.Courses;

using ContosoUniversity.Pages.Courses;

[Collection(nameof(SliceFixture))]
public class CacheTests
{
    private readonly SliceFixture _fixture;

    public CacheTests(SliceFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task Should_return_cached_course_title_when_update_does_not_go_through_command()
    {
		var adminId = await _fixture.SendAsync(
            new ContosoUniversity.Pages.Instructors.CreateEdit.Command
            {
                FirstMidName = "George",
                LastName = "Jones",
                HireDate = DateTime.Today
            });

        var englishDept = new Department
        {
            Name = "English",
            InstructorId = adminId,
            Budget = 123m,
            StartDate = DateTime.Today
        };
        var english = new Course
        {
            Credits = 4,
            Department = englishDept,
            Id = _fixture.NextCourseNumber(),
            Title = "English 101"
        };
        await _fixture.InsertAsync(
	        englishDept,
	        english);
        
        var cachedResult = await _fixture.SendAsync(new Details.Query { Id = english.Id });

		cachedResult.ShouldNotBeNull();

        var cachedTitle = english.Title;
        cachedTitle.ShouldBe(english.Title);

		await _fixture.ExecuteDbContextAsync(async (ctxt, mediator) =>
		{
			var course = await _fixture.FindAsync<Course>(english.Id);

			course.Title = "English 202";
			ctxt.Courses.Update(course);

		});

		var result = await _fixture.SendAsync(new Details.Query { Id = english.Id });

		result.Title.ShouldBe(cachedTitle);

		Edit.Command command = default;

		await _fixture.ExecuteDbContextAsync(async (ctxt, mediator) =>
		{
			command = new Edit.Command
			{
				Id = english.Id,
				Credits = english.Credits,
				Title = "English 202",
			};

			await mediator.Send(command);

		});

		var edited = await _fixture.SendAsync(new Details.Query { Id = english.Id });

		edited.ShouldNotBeNull();
		edited.Credits.ShouldBe(command.Credits.GetValueOrDefault());
		edited.Title.ShouldBe(command.Title);
	}

    [Fact]
    public async Task Should_return_updated_course_title_when_update_goes_through_command()
    {
	    var adminId = await _fixture.SendAsync(
		    new ContosoUniversity.Pages.Instructors.CreateEdit.Command
		    {
			    FirstMidName = "George",
			    LastName = "Jones",
			    HireDate = DateTime.Today
		    });

	    var englishDept = new Department
	    {
		    Name = "English",
		    InstructorId = adminId,
		    Budget = 123m,
		    StartDate = DateTime.Today
	    };
	    var english = new Course
	    {
		    Credits = 4,
		    Department = englishDept,
		    Id = _fixture.NextCourseNumber(),
		    Title = "English 101"
	    };
	    await _fixture.InsertAsync(
		    englishDept,
		    english);

	    var cachedResult = await _fixture.SendAsync(new Details.Query { Id = english.Id });

	    cachedResult.ShouldNotBeNull();

	    var cachedTitle = english.Title;
	    cachedTitle.ShouldBe(english.Title);

	    await _fixture.ExecuteDbContextAsync(async (ctxt, mediator) =>
	    {
		    var course = await _fixture.FindAsync<Course>(english.Id);

		    course.Title = "English 202";
		    ctxt.Courses.Update(course);

	    });

	    var result = await _fixture.SendAsync(new Details.Query { Id = english.Id });

	    result.Title.ShouldBe(cachedTitle);

	    Edit.Command command = default;

	    await _fixture.ExecuteDbContextAsync(async (ctxt, mediator) =>
	    {
		    command = new Edit.Command
		    {
			    Id = english.Id,
			    Credits = english.Credits,
			    Title = "English 202",
		    };

		    await mediator.Send(command);

	    });

	    var edited = await _fixture.SendAsync(new Details.Query { Id = english.Id });

	    edited.ShouldNotBeNull();
	    edited.Credits.ShouldBe(command.Credits.GetValueOrDefault());
	    edited.Title.ShouldBe(command.Title);
    }
}