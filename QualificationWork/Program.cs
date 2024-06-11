using MathNet.Numerics.LinearAlgebra;
using System;
using System.Text.RegularExpressions;
using static QualificationWork.Program;

namespace QualificationWork {
    public record Group(string Name) {
        public Vector<float> Availability { get; set; }
        public Vector<float> OriginalAvailability { get; set; }
    }

    public record Professor(string Name) {
        public Vector<float> Availability { get; set; }
        public Vector<float> OriginalAvailability { get; set; }
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
            //Example1p4();
            realData();


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
        
        private static void realData() {
            // Define hours and days
            int hours = 3, days = 5;

            // Read the CSV file
            string[] lines = File.ReadAllLines("data.csv");

            // Initialize lists for groups and professors
            var groups = new List<Group>();
            var professors = new List<Professor>();

            // Dictionary to store the hours matrix data
            var matrixData = new Dictionary<(string professor, string group), int>();

            // Process the CSV file, skipping the header line
            foreach (var line in lines.Skip(1)) {
                var parts = line.Split(';');
                string professor = parts[0].Replace("977", "").Replace("@.com.ua", "").Trim();
                string group = parts[1].Trim();

                // Add the professor if not already in the list
                if (!professors.Any(p => p.Name == professor)) {
                    professors.Add(new Professor(professor));
                }

                // Add the group if not already in the list
                if (!groups.Any(g => g.Name == group)) {
                    groups.Add(new Group(group));
                }

                // Increment the count for the professor-group pair
                var key = (professor, group);
                if (!matrixData.ContainsKey(key)) {
                    matrixData[key] = 0;
                }
                matrixData[key]++;
            }

            // Create the matrix with columns as professors and rows as groups
            float[,] array = new float[groups.Count, professors.Count];
            foreach (var kvp in matrixData) {
                var professorIndex = professors.FindIndex(p => p.Name == kvp.Key.professor);
                var groupIndex = groups.FindIndex(g => g.Name == kvp.Key.group);
                array[groupIndex, professorIndex] = kvp.Value;
            }

            // Convert the 2D array to a MathNet matrix
            var matrix = Matrix<float>.Build.DenseOfArray(array);

            // Build the TimetableTask
            var task = new TimetableTask.Builder()
                .Groups(groups)
                .Professors(professors)
                .HoursPerDay(hours)
                .Days(days)
                .PlannedHours(matrix)
                .Build();

            Console.WriteLine("Timetable limitations created successfully.");
            Console.WriteLine(task.PlanMatrix);

            // Create the weights.
            //var priority = Matrix<float>.Build.DenseOfArray(new float[,] {
            //    { 4, 1 }, // Professor A's hours for Group 1 and Group 2
            //    { 3, 2 }  // Professor B's hours for Group 1 and Group 2
            //});

            var priority = Matrix<float>.Build.Dense(groups.Count, professors.Count, 1);

            // Calculate the maximum weight
            var maxWeight = priority.Enumerate().Max();

            // Output weights and solve the task
            Console.WriteLine("Weights:");
            Console.WriteLine(priority);

            task.Solve(priority);
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
            Console.WriteLine(task.PlanMatrix);
            
            // Create the weights.
            var prioraty = Matrix<float>.Build.DenseOfArray(new float[,] {
                { 4, 1 }, // Professor A's hours for Group 1 and Group 2
                { 3, 2 }  // Professor B's hours for Group 1 and Group 2
                //{ 1, 1 },  // Professor A's hours for Group 1 and Group 2
                //{ 1, 1 }   // Professor B's hours for Group 1 and Group 2
            });

            // SEE: playing with setting the priority as vectors of prof priority and group priority
            //var vecotorPrioratyProfs = Vector<float>.Build.DenseOfArray(new float[] { 2, 1 });
            //var vecotorPrioratySubjects = Vector<float>.Build.DenseOfArray(new float[] { 1, 2 });
            //var m_pr_Profs = Matrix<float>.Build.DenseOfColumnVectors(vecotorPrioratyProfs);
            //var m_pr_Groups = Matrix<float>.Build.DenseOfRowVectors(vecotorPrioratySubjects);
            //Console.WriteLine(m_pr_Profs * m_pr_Groups);
            //Console.WriteLine("While it is:");
            //Console.WriteLine(prioraty);


            //Matrix<float> result = prioraty.Clone();
            var m = prioraty.Enumerate().Max();
            //prioraty.Map(x => (1 / x) * m, result);
            Console.WriteLine("Wights:");
            Console.WriteLine(prioraty);

            task.Solve(prioraty);

            //this will solve one prof by one
            //for (int i = 0; i < professors.Count; i++) {
            //    var items = task.ItemizeByProfessor(prioraty,i);
            //    var solution = KnapSackProblem.Solve(items.ToArray(), hours * days);

            //    int groupC = task.Groups.Count;
            //    var res = Matrix<float>.Build.DenseOfIndexed(hours * days, groupC * task.Professors.Count,
            //        solution.Select((x, i) => Tuple.Create(i, groupC * x.Group + x.Prof, 1f)));
            //    Console.WriteLine(res);
            //    foreach (var item in solution) {
            //        Console.WriteLine($"m{item.Group + 1} p{item.Prof + 1}");
            //    }
            //}


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