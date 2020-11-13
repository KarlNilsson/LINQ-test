using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Threading;

namespace LINQ_test
{
    class MyLinq
    {
        static long ListExecutionTime = 0;
        static long DictExecutionTime = 0;
        static int Memberships = 128000;
        static int Students = 7500;
        static int Schools = 10;
        private static void getListThread(object data)
        {
            Console.WriteLine("Starting list thread");
            Array args = new object[3];
            args = (Array)data;
            var GroupUserList = (List<GroupUser>)args.GetValue(0);
            var StudentList = (List<Student>)args.GetValue(1);
            var output = (List<GroupUser>)args.GetValue(2);
            var sw = Stopwatch.StartNew();

            getList(GroupUserList, StudentList, output);

            sw.Stop();
            ListExecutionTime = sw.ElapsedMilliseconds;
            Console.WriteLine("Finished list thread");
        }

        private static void getDictThread(object data)
        {
            Console.WriteLine("Starting dictionary thread");
            Array args = new object[3];
            args = (Array)data;
            var GroupUserDict = (Dictionary<string, List<GroupUser>>)args.GetValue(0);
            var StudentDict = (Dictionary<string, List<Student>>)args.GetValue(1);
            var output = (List<GroupUser>)args.GetValue(2);
            var sw = Stopwatch.StartNew();

            getListFromDict(GroupUserDict, StudentDict, output);

            sw.Stop();
            DictExecutionTime = sw.ElapsedMilliseconds;
            Console.WriteLine("Finished dictionary thread");
        }
        private static void getList(List<GroupUser> GroupUserList, List<Student> StudentList, List<GroupUser> output)
        {
            var myEntry = GroupUserList.Where(
              a => !StudentList.Exists(
                b => b.skolon_schoolUnitCode.Equals(
                  a.Skolon_schoolUnitCode,
                  StringComparison.OrdinalIgnoreCase
                )
                && b.skolon_extID.Equals(
                  a.Skolon_extUserId
                )
              )
            );
            output.AddRange(myEntry.ToList());
        }

        private static void getListFromDict(
            Dictionary<string, List<GroupUser>> GroupUserDict,
            Dictionary<string, List<Student>> StudentDict,
            List<GroupUser> output)
        {
            var missingEntries = new List<GroupUser>();
            foreach(KeyValuePair<string, List<GroupUser>> entry in GroupUserDict)
            {
                var studentList = StudentDict[entry.Key];
                var groupUserList = entry.Value;
                var entries = groupUserList.Except(studentList, (a, b) => {
                    return a.Skolon_schoolUnitCode == b.skolon_schoolUnitCode;
                }).ToList();
                missingEntries.AddRange(entries);
            }
            output.AddRange(missingEntries);
        }

        static void Main(string[] args)
        {
            Console.WriteLine($"Number of memberships per list (studentList/groupUserList): {Memberships}");
            Console.WriteLine($"Number of unique students: {Students}");
            Console.WriteLine($"Number of unique schools: {Schools}");
            Console.WriteLine(new String('-', 40));

            var StudentList = new List<Student>();
            var GroupUserList = new List<GroupUser>();
            var StudentDict = new Dictionary<string, List<Student>>();
            var GroupUserDict = new Dictionary<string, List<GroupUser>>();
            var rand = new Random();


            for (int i = 0; i < Memberships; i++)
            {
                var studentEntry = new Student(
                    $"S-{rand.Next(0, Students)}-5-21-3413381747-2981958099-1218169479-1221",
                    $"School-{rand.Next(0, Schools)}-5-21-3413381747-2981958099-1218169479-1221"
                );
                var groupUserEntry = new GroupUser(
                    $"S-{rand.Next(0, Students)}-5-21-3413381747-2981958099-1218169479-1221",
                    $"School-{rand.Next(0, Schools)}-5-21-3413381747-2981958099-1218169479-1221"
                );
                StudentList.Add(studentEntry);
                GroupUserList.Add(groupUserEntry);

                if (!StudentDict.TryGetValue(studentEntry.skolon_extID, out List<Student> studentEntryList))
                {
                    studentEntryList = new List<Student>();
                    StudentDict.Add(studentEntry.skolon_extID, studentEntryList);
                }
                if (!GroupUserDict.TryGetValue(groupUserEntry.Skolon_extUserId, out List<GroupUser> groupUserEntryList))
                {
                    groupUserEntryList = new List<GroupUser>();
                    GroupUserDict.Add(groupUserEntry.Skolon_extUserId, groupUserEntryList);
                }

                studentEntryList.Add(studentEntry);
                groupUserEntryList.Add(groupUserEntry);
            }
            var t1 = new Thread(new ParameterizedThreadStart(getListThread));
            var t1Output = new List<GroupUser>();
            t1.Start(new object[3] { GroupUserList, StudentList, t1Output });
            var t2 = new Thread(new ParameterizedThreadStart(getDictThread));
            var t2Output = new List<GroupUser>();
            t2.Start(new object[3] { GroupUserDict, StudentDict, t2Output });

            var threads = new List<Thread> { t1, t2 };
            threads.ForEach(thread => thread.Join());
            Console.WriteLine(new String('-', 40));
            Console.WriteLine($"Diffing entries for list:\t{t1Output.Count}");
            Console.WriteLine($"Diffing entries for dictionary:\t{t2Output.Count}");
            Console.WriteLine($"Execution time for list:      \t{ListExecutionTime}ms");
            Console.WriteLine($"Execution time for dictionary:\t{DictExecutionTime}ms");

            Console.ReadKey();
        }
    }
}
