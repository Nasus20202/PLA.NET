using System;
using System.Collections.Generic;
using University.Domain.Entities;
using Xunit;

#nullable enable

namespace University.Domain.Tests.Entities;

public class CourseTests
{
    [Fact]
    public void DefaultCollections_AreInitialized()
    {
        var course = new Course();

        Assert.NotNull(course.Enrollments);
        Assert.NotNull(course.Prerequisites);
        Assert.NotNull(course.RequiredByCourses);
    }

    [Fact]
    public void CanAddPrerequisite_AddsProperly()
    {
        var course = new Course { Id = 1 };
        var prereq = new Course { Id = 2 };

        course.Prerequisites.Add(prereq);

        Assert.Contains(prereq, course.Prerequisites);
    }
}
