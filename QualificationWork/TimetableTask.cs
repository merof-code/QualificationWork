using MathNet.Numerics.LinearAlgebra;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

[assembly: InternalsVisibleTo("TestProject1")]
namespace QualificationWork {
    public partial class TimetableTask {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        private TimetableTask() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        //todo: do a converter to this List<ProfGroup> PlannedHours { get; set; }
        public int Days { get; private set; } = 5;
        public int HoursPerDay { get; private set; } = 6;
        public List<Professor> Professors { get; private set; }
        public List<Group> Groups { get; private set; }
        public Matrix<float> Matrix { get; private set; }
        /// <summary>
        /// columns = profs * group
        /// rows = days * hours 
        /// TODO: add accessors to it.
        /// </summary>
        public Matrix<float> Solution { get; internal set; }

        #region Accessors
        // TODO: tests for these 2.
        /// <summary>
        /// to get a solution vector for the column
        /// </summary>
        /// <exception cref="ArgumentException">when prog or group not found</exception>
        public Vector<float> this[Group group, Professor professor] {
            get {
                int professorIndex = Professors.IndexOf(professor);
                int groupIndex = Groups.IndexOf(group);

                return this[groupIndex, professorIndex];
            }
        }

        /// <summary>
        /// to get a solution vector for the column
        /// </summary>
        /// <exception cref="ArgumentException">when prog or group out of bounds</exception>
        public Vector<float> this[int groupId, int professorId] {
            get {
                if (professorId < 0 || professorId >= Professors.Count || groupId < 0 || groupId >= Groups.Count) {
                    throw new ArgumentException("Invalid Professor or Group ID.");
                }

                int columnIndex = GetSolutionColumnIndex(groupId, professorId);

                return Solution.Column(columnIndex);
            }
        }

        private int GetSolutionColumnIndex(int groupId, int professorId) {
            return groupId * (Groups.Count) + professorId;
        }
        #endregion

        public void Solve(Matrix<float> weights) {
            var totalTime = Days * HoursPerDay;
            var items = ItemizeByProfessor(weights, 0);
            items.ForEach(x => Console.WriteLine(x));
            var solution = KnapSackProblem.Solve(items.ToArray(), totalTime);

            foreach(var (x,i) in solution.Select((x, i) => ( x, i ))) {
                Solution[i, GetSolutionColumnIndex(x.Group, x.Prof)] = 1f;
            }

            foreach (var item in solution) {
                Console.WriteLine($"m{item.Group + 1} p{item.Prof + 1}");
            }
            Console.WriteLine(Solution);
            PartialSolutionVerification();
            //int groupC = task.Groups.Count;
            //var res = Matrix<float>.Build.DenseOfIndexed(hours * days, groupC * task.Professors.Count,
            //    );
            //Console.WriteLine(res);

            //VerifySolution();
        }
        #region coditions
        public void PartialSolutionVerification() {
            VerifyOutputConditionGroupSingleLecture();
            VerifyOutputConditionProfessorSingleLecture();
        }
        public void VerifySolution() {
            PartialSolutionVerification();
            VerifyOutputConditionLectureHours();
        }

        /// <summary>
        /// Verify Constraint 1: Each group can only attend one lecture at a time.
        /// </summary>
        /// <exception cref="Exception"></exception>
        public void VerifyOutputConditionGroupSingleLecture() {
            int M = Groups.Count;
            int P = Professors.Count;
            int q = Days;
            int h = HoursPerDay;

            for (int t = 0; t < q * h; t++) {
                for (int i = 0; i < M; i++) {
                    float sum = 0;
                    for (int j = 0; j < P; j++) {
                        sum += Solution[t, GetSolutionColumnIndex(i, j)];
                    }
                    if (sum > 1) {
                        throw new Exception($"Group i= {i} {Groups[i].Name} has more than one lecture at time {t}.");
                    }
                }
            }
        }

        /// <summary>
        /// Verify Constraint 2: Each professor can only conduct one lecture or no lectures at a time.
        /// </summary>
        /// <exception cref="Exception"></exception>
        public void VerifyOutputConditionProfessorSingleLecture() {
            int M = Groups.Count;
            int P = Professors.Count;
            int q = Days;
            int h = HoursPerDay;

            for (int t = 0; t < q * h; t++) {
                foreach (var (prof, j) in Professors.Select((prof, index) => (prof, index))) {
                    float sum = 0;
                    for (int i = 0; i < M; i++) {
                        sum += Solution[t, GetSolutionColumnIndex(i, j)];
                    }
                    if (sum > prof.Availability[t])
                        throw new Exception($"Professor {j} {prof.Name} is scheduled for more lectures than available at time {t}.");
                }
            }
        }

        /// <summary>
        /// Verify Constraint 3: The planned lecture hours for each group and professor match the scheduled hours.
        /// </summary>
        /// <exception cref="Exception"></exception>
        public void VerifyOutputConditionLectureHours() {
            int M = Groups.Count;
            int P = Professors.Count;
            int q = Days;
            int h = HoursPerDay;

            for (int i = 0; i < M; i++) {
                for (int j = 0; j < P; j++) {
                    float sum = 0;
                    for (int t = 0; t < q * h; t++) {
                        sum += Solution[t, GetSolutionColumnIndex(i, j)];
                    }
                    if (sum != Matrix[i, j]) {
                        throw new Exception($"Group i={i} {Groups[i].Name} and Professor j={j} {Professors[j].Name} do not match the planned lecture hours. Expected {Matrix[i, j]}, but got {sum}.");
                    }
                }
            }
        } 
        #endregion
        
        // perhaps move solve to an external class to have multiple ways to solve the thing
        // have itemize by prof by day, by group? by prof total
        // have weights for the different days
        // like first prof gets priority, is fully set in all timetable
        // perhaps run partial verification to see that nothing is wrong in the process
        // have like a loop solve or something live that, to that matter.

        public List<Item> ItemizeByProfessor(Matrix<float> weights, int j) {
            List<Item> list = new List<Item>((int)Matrix.RowSums().Sum());
            foreach (var (prof, hourCountRaw) in Matrix.EnumerateColumnsIndexed(j,1)) {
                foreach (var (group,hours) in hourCountRaw.EnumerateIndexed()) {
                    for (int i = 1; i <= (int)hours; i++) {
                        list.Add(new Item { Value = (int)weights[group, prof], Weight = 1, Prof = prof, Group = group });

                    }
                }
            }
            return list;
        }
        public List<Item> Itemize(Matrix<float> weights) {
            List<Item> list = new List<Item> ((int)Matrix.RowSums().Sum());
            foreach (var (group,prof,hourCountRaw) in Matrix.EnumerateIndexed()) {
                int hourCount = (int)hourCountRaw;
                for (int i = 0; i < hourCount; i++) {
                    list.Add(new Item { Value = (int)weights[group, prof], Weight=1, Prof = prof, Group = group });
                }
            }
            return list;
        }

    }
}