using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LINQ_test
{
    public class Student
    {
        public string skolon_extID { get; }
        public string skolon_schoolUnitCode { get; }
        public Student(string extId, string schoolUnitCode)
        {
            skolon_extID = extId;
            skolon_schoolUnitCode = schoolUnitCode;
        }
    }
    public class GroupUser
    {
        public string Skolon_extUserId { get; }
        public string Skolon_schoolUnitCode { get; }
        public GroupUser(string extId, string schoolUnitCode)
        {
            Skolon_extUserId = extId;
            Skolon_schoolUnitCode = schoolUnitCode;
        }
    }

    public static class Extensions
    {
        public static IEnumerable<GroupUser> Except<GroupUser, Student>(
            this IEnumerable<GroupUser> first,
            IEnumerable<Student> second,
            Func<GroupUser, Student, bool> comparer)
        {
            return first.Where(x => second.Count(y => comparer(x, y)) == 0);
        }
    }
}
