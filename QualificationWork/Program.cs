using MathNet.Numerics.LinearAlgebra;
using System;
using static QualificationWork.Program;

namespace QualificationWork {
    public class Item {
        public int Weight { get; set; }
        public int Value { get; set; } = -1;
        public string Name { get; set; } = "unset";

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
            var groups = new List<Group> {
                new Group("m1"),
                new Group("m2")
            };

            var professors = new List<Professor> {
                new Professor("p1"),
                new Professor("p2"),
            };

            float[,] array = new float[,] {
                { 2, 1 },  // Professor A's hours for Group 1 and Group 2
                { 3, 3 }   // Professor B's hours for Group 1 and Group 2
            };

            // Convert the 2D array to a MathNet matrix
            var matrix = Matrix<float>.Build.DenseOfArray(array);

            var task = new TimetableTask.Builder()
                .Groups(groups).Professors(professors)
                .HoursPerDay(6).Days(2)
                .PlannedHours(matrix)
                .Build();

            Console.WriteLine("Timetable limitations created successfully.");
            // Create the weights.
            //KnapSackProblem knapSackProblem = new(items, volume)
        }
    }
}


//create KnapSack problem for day one for prof 

//var s = new KnapSackProblem(items.ToArray(), backpackVolume);
//var solution = s.Solve();
//Console.WriteLine(string.Join(" ", solution));
//Console.WriteLine(s.MaxValue);