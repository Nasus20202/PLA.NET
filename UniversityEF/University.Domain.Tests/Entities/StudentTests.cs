using System;
using System.Collections.Generic;
using University.Domain.Entities;
using Xunit;

#nullable enable

namespace University.Domain.Tests.Entities;

public class StudentTests
{
    [Fact]
    public void DefaultValues_AreInitialized()
    {
        var student = new Student();

        Assert.NotNull(student.ResidenceAddress);
        Assert.NotNull(student.Enrollments);
        Assert.Equal(0, student.YearOfStudy);
        Assert.Equal(string.Empty, student.FirstName);
    }

    [Fact]
    public void CanAddEnrollmentToStudent_EnrollmentsListWorks()
    {
        var student = new Student();
        var enrollment = new Enrollment { Id = 1, StudentId = 0 };

        student.Enrollments.Add(enrollment);

        Assert.Contains(enrollment, student.Enrollments);
    }
}
