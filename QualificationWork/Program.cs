using MathNet.Numerics.LinearAlgebra;
using System;
using System.Text.RegularExpressions;
using static QualificationWork.Program;

namespace QualificationWork {
    public class Item {
        public int Weight { get; set; }
        public int Value { get; set; } = -1;
        public string Name { get; set; } = "unset";
        public int Group { get; internal set; }
        public int Prof { get; internal set; }

        public override string ToString() {
            return $"({Weight}|{Value})";
        }
    }
    public record Group(string Name);
    public record Professor(string Name) {
        // TODO: make availability
        public Vector<float> Availability { get; set; }
    }

    public record ProfGroup(Professor Professor, Group Group, int Hours);



    //create KnapSack problem for day one for prof 

    //var s = new KnapSackProblem(items.ToArray(), backpackVolume);
    //var solution = s.Solve();
    //Console.WriteLine(string.Join(" ", solution));
    //Console.WriteLine(s.MaxValue);

    internal class Program {
        //ProfGroup
        // perhaps, we can go each day each prof, that way the professors will be full.
        // we need to maintain limitations somehow
        // perhaps add empty slot if needed
        static void Main(string[] args) {
            //var itemsCount = 20000;
            //int backpackVolume = 400;
            //List<Item> items = new(itemsCount);
            //Random random = new Random();
            //for (int i = 0; i < itemsCount; i++) {
            //    items.Add(new() { Weight = random.Next(1 + 10), Value = random.Next(10 + 300) });
            //}
            Example1p4();


            //// Create a list of 5 Groups
            //var groups = new List<Group> {
            //    new Group("Group 1"),
            //    new Group("Group 2"),
            //    new Group("Group 3"),
            //    new Group("Group 4"),
            //    new Group("Group 5")
            //};

            //// Create a list of 3 Professors
            //var professors = new List<Professor> {
            //    new Professor("Professor A"),
            //    new Professor("Professor B"),
            //    new Professor("Professor C")
            //};

            //// Create a list of ProfGroup entries
            //var plannedHours = new List<ProfGroup> {
            //    new ProfGroup(professors[0], groups[0], 4),
            //    new ProfGroup(professors[0], groups[1], 3),
            //    new ProfGroup(professors[0], groups[3], 2),
            //    new ProfGroup(professors[0], groups[4], 2),
            //    new ProfGroup(professors[0], groups[5], 2),

            //    new ProfGroup(professors[1], groups[1], 3),
            //    new ProfGroup(professors[1], groups[4], 6),
            //    new ProfGroup(professors[2], groups[2], 5),
            //};

            //// Initialize the TimetableLimitations class
            //var timetable = new TimetableLimitations(professors, groups, plannedHours);

            //Console.WriteLine("Timetable limitations created successfully.");

            // create items
            // lets first do it by hand
            
        }

        private static void Example1p4() {
            int hours = 3, days = 2;
            var groups = new List<Group> {
                new Group("m1"),
                new Group("m2")
            };

            var professors = new List<Professor> {
                new Professor("p1") { Availability = Vector<float>.Build.Dense(hours * days, 1)},
                new Professor("p2") { Availability = Vector<float>.Build.Dense(hours * days, 1)},
            };

            float[,] array = new float[,] {
                { 2, 1 },  // Professor A's hours for Group 1 and Group 2
                { 3, 3 }   // Professor B's hours for Group 1 and Group 2
            };

            // Convert the 2D array to a MathNet matrix
            var matrix = Matrix<float>.Build.DenseOfArray(array);

            var task = new TimetableTask.Builder()
                .Groups(groups).Professors(professors)
                .HoursPerDay(hours).Days(days)
                .PlannedHours(matrix)
                .Build();

            Console.WriteLine("Timetable limitations created successfully.");
            Console.WriteLine(task.Matrix);
            
            // Create the weights.
            var prioraty = Matrix<float>.Build.DenseOfArray(new float[,] {
                { 2, 4 },  // Professor A's hours for Group 1 and Group 2
                { 1, 3 }   // Professor B's hours for Group 1 and Group 2
            });

            Matrix<float> result = prioraty.Clone();
            prioraty.Map(x => 1 / x, result);
            Console.WriteLine("Wights:");
            Console.WriteLine(result);


            //this will solve one prof by one
            for (int i = 0; i < professors.Count; i++) {
                var items = task.ItemizeByProfessor(prioraty,i);
                var solution = KnapSackProblem.Solve(items.ToArray(), hours * days);

                int groupC = task.Groups.Count;
                var res = Matrix<float>.Build.DenseOfIndexed(hours * days, groupC * task.Professors.Count,
                    solution.Select((x, i) => Tuple.Create(i, groupC * x.Group + x.Prof, 1f)));
                Console.WriteLine(res);
                foreach (var item in solution) {
                    Console.WriteLine($"m{item.Group + 1} p{item.Prof + 1}");
                }
            }


            ////this solves it all in one go, but this is only as if it was only one prof
            //KnapSackProblem knapSackProblem = new(items.ToArray(), hours * days);
            //var solution = KnapSackProblem.Solve(items.ToArray(), hours * days);
            //int groupC = task.Groups.Count;
            //var res = Matrix<float>.Build.DenseOfIndexed(hours * days, groupC * task.Professors.Count,
            //    solution.Select((x,i) => Tuple.Create(i, groupC * x.Group + x.Prof, 1f)));
            //Console.WriteLine(res);
            //foreach (var item in solution) {
            //    Console.WriteLine($"m{item.Group + 1} p{item.Prof + 1}");
            //}
        }
    }
}


//create KnapSack problem for day one for prof 

//var s = new KnapSackProblem(items.ToArray(), backpackVolume);
//var solution = s.Solve();
//Console.WriteLine(string.Join(" ", solution));
//Console.WriteLine(s.MaxValue);